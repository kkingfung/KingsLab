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
    private struct FillArrayEntityWithPositionJob : IJobForEachWithEntity<Translation>
    {
        public NativeArray<EntityWithPosition> targetArray;

        public void Execute(Entity entity, int index, ref Translation translation)
        {
            targetArray[index] = new EntityWithPosition
            {
                entity = entity,
                position = translation.Value
            };
        }
    }

    [RequireComponentTag(typeof(PlayerTag))]
    [BurstCompile]
    // Find Closest Target
    private struct FindTargetBurstJob : IJobForEachWithEntity<Translation, Area>
    {

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> targetArray;
        public NativeArray<EntityWithPosition> closestTargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref Area radius)
        {
            float3 unitPosition = translation.Value;
            float3 closestTargetPosition = translation.Value;
            Entity closestTargetEntity = Entity.Null;
            float closestTargetDistance = float.MaxValue;

            for (int i = 0; i < targetArray.Length; i++)
            {
                // Cycling through all target entities
                EntityWithPosition targetEntityWithPosition = targetArray[i];

                if (closestTargetEntity == Entity.Null)
                {
                    // No target
                    if (math.distancesq(unitPosition, targetEntityWithPosition.position) < radius.Value * radius.Value)
                    {
                        closestTargetEntity = targetEntityWithPosition.entity;
                        closestTargetDistance = math.distancesq(unitPosition, targetEntityWithPosition.position);
                        closestTargetPosition = targetEntityWithPosition.position;
                    }
                }
                else
                {
                    if (math.distancesq(unitPosition, targetEntityWithPosition.position) < closestTargetDistance)
                    {
                        // This target is closer
                        closestTargetEntity = targetEntityWithPosition.entity;
                        closestTargetDistance = math.distancesq(unitPosition, targetEntityWithPosition.position);
                        closestTargetPosition = targetEntityWithPosition.position;
                    }
                }
            }
            EntityWithPosition targetDetail = new EntityWithPosition {entity= closestTargetEntity,position= closestTargetPosition };
            closestTargetEntityArray[index] = targetDetail;
        }

    }

    [RequireComponentTag(typeof(PlayerTag))]
    [ExcludeComponent(typeof(Target))]
    // Add Target Component to Entities that have a Closest Target
    private struct AddComponentJob : IJobForEachWithEntity<Translation>
    {

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> closestTargetEntityArray;
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        public void Execute(Entity entity, int index, ref Translation translation)
        {
            if (closestTargetEntityArray[index].entity != Entity.Null)
            {
                entityCommandBuffer.AddComponent(index, entity, new Target { targetEntity = closestTargetEntityArray[index].entity,targetPos= closestTargetEntityArray[index].position });
            }
        }

    }

    [RequireComponentTag(typeof(PlayerTag))]
    [ExcludeComponent(typeof(Target))]
    [BurstCompile]
    private struct FindTargetQuadrantSystemJob : IJobForEachWithEntity<Translation, Area, QuadrantEntity>
    {

        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;
        public NativeArray<EntityWithPosition> closestTargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, [ReadOnly] ref Area radius, [ReadOnly] ref QuadrantEntity quadrantEntity)
        {
            float3 unitPosition = translation.Value;
            Entity closestTargetEntity = Entity.Null;
            float closestTargetDistance = float.MaxValue;
            float3 closestTargetPosition = translation.Value;
            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(translation.Value);

            FindTarget(hashMapKey, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey + 1, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey - 1, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey + QuadrantSystem.quadrantYMultiplier, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey - QuadrantSystem.quadrantYMultiplier, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier, unitPosition,radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier, unitPosition,radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier, unitPosition,radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            FindTarget(hashMapKey - 1 - QuadrantSystem.quadrantYMultiplier, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);

            EntityWithPosition targetDetail = new EntityWithPosition { entity = closestTargetEntity, position = closestTargetPosition };
            closestTargetEntityArray[index] = targetDetail;
        }

        private void FindTarget(int hashMapKey, float3 unitPosition, float maxdist, QuadrantEntity quadrantEntity, ref Entity closestTargetEntity, ref float closestTargetDistance, ref float3 closestTargetPosition)
        {
            QuadrantData quadrantData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
            {
                do
                {
                    if (quadrantEntity.typeEnum != quadrantData.quadrantEntity.typeEnum)
                    {
                        if (closestTargetEntity == Entity.Null)
                        {
                            // No target
                            if (math.distancesq(unitPosition, quadrantData.position) < maxdist)
                            {
                                closestTargetEntity = quadrantData.entity;
                                closestTargetDistance = math.distancesq(unitPosition, quadrantData.position);
                                closestTargetPosition = quadrantData.position;
                            }
                        }
                        else
                        {
                            if (math.distancesq(unitPosition, quadrantData.position) < closestTargetDistance)
                            {
                                // This target is closer
                                closestTargetEntity = quadrantData.entity;
                                closestTargetDistance = math.distancesq(unitPosition, quadrantData.position);
                                closestTargetPosition = quadrantData.position;
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
        EntityQuery targetQuery = GetEntityQuery(typeof(Target), ComponentType.ReadOnly<Translation>());

        bool useQuadrantSystem = true;
        if (useQuadrantSystem)
        {
            EntityQuery unitQuery = GetEntityQuery(typeof(PlayerTag), typeof(Area), ComponentType.Exclude<Target>());
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

            EntityQuery unitQuery = GetEntityQuery(typeof(PlayerTag), ComponentType.Exclude<Target>());
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

