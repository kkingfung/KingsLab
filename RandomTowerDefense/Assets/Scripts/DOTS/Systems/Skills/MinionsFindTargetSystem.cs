using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

[UpdateAfter(typeof(QuadrantSystem))]
public class MinionsFindTargetSystem : JobComponentSystem
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

        public void Execute(Entity entity, int index, ref Translation transform)
        {
            targetArray[index] = new EntityWithPosition
            {
                entity = entity,
                position = transform.Value
            };
        }
    }


    [RequireComponentTag(typeof(MinionsTag))]
    [ExcludeComponent(typeof(HasTarget))]
    // Add HasTarget Component to Entities that have a Closest Target
    private struct AddComponentJob_Skill : IJobForEachWithEntity<Translation>
    {

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<EntityWithPosition> closestTargetEntityArray;
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        public void Execute(Entity entity, int index, ref Translation transform)
        {
            if (closestTargetEntityArray[index].entity != Entity.Null)
            {
                entityCommandBuffer.AddComponent(index, entity, new Target
                {
                    targetEntity = closestTargetEntityArray[index].entity,
                    targetPos = closestTargetEntityArray[index].position
                });
            }
        }
    }

    [RequireComponentTag(typeof(MinionsTag))]
    [ExcludeComponent(typeof(HasTarget))]
    [BurstCompile]
    private struct FindTargetQuadrantSystemJob : IJobForEachWithEntity<Translation, Radius, QuadrantEntity>
    {

        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;
        public NativeArray<EntityWithPosition> closestTargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation transform, [ReadOnly] ref Radius radius, [ReadOnly] ref QuadrantEntity quadrantEntity)
        {
            float3 unitPosition = transform.Value;
            Entity closestTargetEntity = Entity.Null;
            float closestTargetDistance = float.MaxValue;
            float3 closestTargetPosition = transform.Value;
            int hashMapKey = QuadrantSystem.GetPositionHashMapKey(transform.Value);

            FindTarget(hashMapKey, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);

            if (closestTargetEntity == Entity.Null)
            {
                FindTarget(hashMapKey + 1, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                FindTarget(hashMapKey - 1, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                FindTarget(hashMapKey + QuadrantSystem.quadrantYMultiplier, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                FindTarget(hashMapKey - QuadrantSystem.quadrantYMultiplier, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            }

            if (closestTargetEntity == Entity.Null)
            {
                FindTarget(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                FindTarget(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                FindTarget(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                FindTarget(hashMapKey - 1 - QuadrantSystem.quadrantYMultiplier, unitPosition, radius.Value, quadrantEntity, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
            }

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
                    if (quadrantData.quadrantEntity.typeEnum == QuadrantEntity.TypeEnum.EnemyTag)
                    {
                        float distSq = math.distancesq(unitPosition, quadrantData.position);
                        if (closestTargetEntity == Entity.Null)
                        {
                            // No target
                            closestTargetEntity = quadrantData.entity;
                            closestTargetDistance = distSq;
                            closestTargetPosition = quadrantData.position;
                        }
                        else
                        {
                            if (distSq < closestTargetDistance)
                            {
                                // This target is closer
                                closestTargetEntity = quadrantData.entity;
                                closestTargetDistance = distSq;
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
        EntityQuery targetQuery = GetEntityQuery(typeof(EnemyTag), ComponentType.ReadOnly<Translation>());

        EntityQuery unitQuery = GetEntityQuery(typeof(MinionsTag), typeof(Radius)/*, ComponentType.Exclude<Target>()*/);
        NativeArray<EntityWithPosition> closestTargetEntityArray = new NativeArray<EntityWithPosition>(unitQuery.CalculateEntityCount(), Allocator.TempJob);

        FindTargetQuadrantSystemJob findTargetQuadrantSystemJob = new FindTargetQuadrantSystemJob
        {
            quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap,
            closestTargetEntityArray = closestTargetEntityArray,
        };
        JobHandle jobHandle = findTargetQuadrantSystemJob.Schedule(this, inputDeps);

        // Add HasTarget Component to Entities that have a Closest Target
        AddComponentJob_Skill addComponentJob = new AddComponentJob_Skill
        {
            closestTargetEntityArray = closestTargetEntityArray,
            entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
        };
        jobHandle = addComponentJob.Schedule(this, jobHandle);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }

}
