using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

//exclude if necessary
//[ExcludeComponent(typeof())]

[UpdateAfter(typeof(QuadrantSystem))]
public class TowerFindTargetSystem : JobComponentSystem
{
    private struct EntityWithPosition
    {
        public Entity entity;
        public float3 position;
    }

    [RequireComponentTag(typeof(EnemyTag))]
    [BurstCompile]
    // Fill single array with Target Entity and Position
    private struct FillArrayEntityWithPositionJob : IJobForEachWithEntity<CustomTransform>
    {
        public NativeArray<EntityWithPosition> targetArray;

        public void Execute(Entity entity, int index, ref CustomTransform transform)
        {
                targetArray[index] = new EntityWithPosition
                {
                    entity = entity,
                    position = transform.translation
                };
        }
    }

    [RequireComponentTag(typeof(PlayerTag))]
    //[ExcludeComponent(typeof(Target))]
    [BurstCompile]
    // Find Closest Enemy
    private struct FindTargetBurstJob : IJobForEachWithEntity<CustomTransform, Radius, TargetFound,TargetPos>
    {

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
        public NativeArray<EntityWithPosition> closestTargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref CustomTransform transform, [ReadOnly] ref Radius radius,
            ref TargetFound targetfound,  ref TargetPos targetpos)
        {
            float3 unitPosition = transform.translation;
            float3 closestTargetPosition = transform.translation;
            Entity closestTargetEntity = Entity.Null;
            float closestTargetDistance = float.MaxValue;
            bool found = targetfound.Value;
            for (int i = 0; i < targetArray.Length; i++)
            {
                // Cycling through all target entities
                EntityWithPosition targetEntityWithPosition = targetArray[i];
                float distSq = math.distancesq(unitPosition, targetEntityWithPosition.position);
                if (closestTargetEntity == Entity.Null)
                {
                    // No target  
                    if (distSq < radius.Value * radius.Value*1.5f)
                    {
                        closestTargetEntity = targetEntityWithPosition.entity;
                        closestTargetDistance = math.distancesq(unitPosition, targetEntityWithPosition.position);
                        closestTargetPosition = targetEntityWithPosition.position;
                        if (distSq < radius.Value * radius.Value) found = true;
                    }
                }
                else
                {
                    if (distSq < closestTargetDistance)
                    {
                        // This target is closer
                        closestTargetEntity = targetEntityWithPosition.entity;
                        closestTargetDistance = math.distancesq(unitPosition, targetEntityWithPosition.position);
                        closestTargetPosition = targetEntityWithPosition.position;
                        if (!found && distSq < radius.Value * radius.Value) found = true;
                    }
                }
            }
            if (closestTargetEntity != Entity.Null)
            {
                EntityWithPosition targetDetail = new EntityWithPosition { entity = closestTargetEntity, position = closestTargetPosition };
                closestTargetEntityArray[index] = targetDetail;
                targetpos.Value = closestTargetPosition;
            }
            targetfound.Value = found;
        }

    }

    [RequireComponentTag(typeof(PlayerTag))]
    // Add HasTarget Component to Entities that have a Closest Target
    private struct AddComponentJob : IJobForEachWithEntity<CustomTransform>
    {

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> closestTargetEntityArray;
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        public void Execute(Entity entity, int index, ref CustomTransform transfoem)
        {
            if (closestTargetEntityArray[index].entity != Entity.Null)
            {
                entityCommandBuffer.AddComponent(index, entity, new Target { targetEntity = closestTargetEntityArray[index].entity });
            }
        }
    }


        [RequireComponentTag(typeof(PlayerTag))]
    [BurstCompile]
    private struct FindTargetQuadrantSystemJob : IJobForEachWithEntity<CustomTransform, Radius, TargetFound, TargetPos, QuadrantEntity>
    {

        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;
        public NativeArray<EntityWithPosition> closestTargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref CustomTransform transform, [ReadOnly] ref Radius radius, ref TargetFound targetfound, ref TargetPos targetpos, [ReadOnly] ref QuadrantEntity quadrantEntity)
        {
            float3 unitPosition = transform.translation;
            Entity closestTargetEntity = Entity.Null;
            float closestTargetDistance = float.MaxValue;
            float3 closestTargetPosition = transform.translation;
            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(transform.translation);
            bool found = false;

            FindTarget(hashMapKey, unitPosition, radius.Value, ref found,quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey + 1, unitPosition, radius.Value, ref found, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey - 1, unitPosition, radius.Value, ref found, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey + QuadrantSystem.quadrantYMultiplier, unitPosition,  radius.Value, ref found, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey - QuadrantSystem.quadrantYMultiplier, unitPosition,  radius.Value, ref found, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier, unitPosition,radius.Value, ref found, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier, unitPosition,radius.Value, ref found, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier, unitPosition,radius.Value, ref found, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey - 1 - QuadrantSystem.quadrantYMultiplier, unitPosition, radius.Value, ref found, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);

            EntityWithPosition targetDetail = new EntityWithPosition { entity = closestTargetEntity, position = closestTargetPosition };
            closestTargetEntityArray[index] = targetDetail;
            targetpos.Value = closestTargetPosition;
            targetfound.Value = found;
        }

        private void FindTarget(int hashMapKey, float3 unitPosition, float maxdist, ref bool targetfound, QuadrantEntity quadrantEntity, ref Entity closestTargetEntity, ref float closestTargetDistance, ref float3 closestTargetPosition)
        {
            QuadrantData quadrantData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
            {
                do
                {
                    if (quadrantEntity.typeEnum != quadrantData.quadrantEntity.typeEnum)
                    {
                        float distSq = math.distancesq(unitPosition, quadrantData.position);
                        if (closestTargetEntity == Entity.Null)
                        {
                            // No target
                            if (distSq < maxdist* maxdist * 1.5f)
                            {
                                closestTargetEntity = quadrantData.entity;
                                closestTargetDistance = math.distancesq(unitPosition, quadrantData.position);
                                closestTargetPosition = quadrantData.position;
                                if (distSq < maxdist * maxdist) targetfound = true;
                            }
                        }
                        else
                        {
                            if (distSq < closestTargetDistance)
                            {
                                // This target is closer
                                closestTargetEntity = quadrantData.entity;
                                closestTargetDistance = math.distancesq(unitPosition, quadrantData.position);
                                closestTargetPosition = quadrantData.position;
                                targetfound = true;
                            }
                        }
                    }
                } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
            }
        }


    }

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery targetQuery = GetEntityQuery(typeof(EnemyTag), ComponentType.ReadOnly<CustomTransform>());

        bool useQuadrantSystem = true;
        if (useQuadrantSystem)
        {
            EntityQuery unitQuery = GetEntityQuery(typeof(PlayerTag), typeof(Radius)/*, ComponentType.Exclude<Target>()*/);
            NativeArray<EntityWithPosition> closestTargetEntityArray = new NativeArray<EntityWithPosition>(unitQuery.CalculateEntityCount(), Allocator.TempJob);

            FindTargetQuadrantSystemJob findTargetQuadrantSystemJob = new FindTargetQuadrantSystemJob
            {
                quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap,
                closestTargetEntityArray = closestTargetEntityArray,
            };
            JobHandle jobHandle = findTargetQuadrantSystemJob.Schedule(this, inputDeps);

            // Add HasTarget Component to Entities that have a Closest Target
            AddComponentJob addComponentJob = new AddComponentJob
            {
                closestTargetEntityArray = closestTargetEntityArray,
                entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            };
            jobHandle = addComponentJob.Schedule(this, jobHandle);

            endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
        else
        {
            NativeArray<EntityWithPosition> targetArray = new NativeArray<EntityWithPosition>(targetQuery.CalculateEntityCount(), Allocator.TempJob);

            // Fill single array with Entity and Position
            FillArrayEntityWithPositionJob fillArrayEntityWithPositionJob = new FillArrayEntityWithPositionJob
            {
                targetArray = targetArray
            };
            JobHandle jobHandle = fillArrayEntityWithPositionJob.Schedule(this, inputDeps);

            EntityQuery unitQuery = GetEntityQuery(typeof(PlayerTag));
            NativeArray<EntityWithPosition> closestTargetEntityArray = new NativeArray<EntityWithPosition>(unitQuery.CalculateEntityCount(), Allocator.TempJob);

            // Find Closest Target
            FindTargetBurstJob findTargetBurstJob = new FindTargetBurstJob
            {
                targetArray = targetArray,
                closestTargetEntityArray = closestTargetEntityArray
            };
            jobHandle = findTargetBurstJob.Schedule(this, jobHandle);

            // Add HasTarget Component to Entities that have a Closest Target
            AddComponentJob addComponentJob = new AddComponentJob
            {
                closestTargetEntityArray = closestTargetEntityArray,
                entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            };
            jobHandle = addComponentJob.Schedule(this, jobHandle);

            endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

            return jobHandle;
        }
    }
}

