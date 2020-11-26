using Unity;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;


public class CastleToEnemy : JobComponentSystem
{
    EntityQuery castleGroup;

    EntityQuery enemyGroup;

    protected override void OnCreate()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        castleGroup = GetEntityQuery(typeof(Health), typeof(Radius), typeof(Damage),
        ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<CastleTag>());

        enemyGroup = GetEntityQuery(typeof(Health), typeof(Radius), typeof(Damage),
            ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<EnemyTag>());

        JobHandle jobHandle = inputDependencies;

        if (enemyGroup.CalculateEntityCount() > 0)
        {
            var transformType = GetComponentTypeHandle<Translation>(true);
            var healthType = GetComponentTypeHandle<Health>(false);
            var radiusType = GetComponentTypeHandle<Radius>(true);
            var damageType = GetComponentTypeHandle<Damage>(false);

            //castle by enemy
            var jobCvE = new CollisionJobCvE()
            {
                healthType = healthType,
                translationType = transformType,
                radius = radiusType,
                damageRecord = damageType,
                targetDamage = enemyGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
                targetRadius = enemyGroup.ToComponentDataArray<Radius>(Allocator.TempJob),
                targetTrans = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
                targetHealth = enemyGroup.ToComponentDataArray<Health>(Allocator.TempJob)
            };
            jobHandle = jobCvE.Schedule(castleGroup, inputDependencies);
            jobHandle.Complete();

            //enemy by castle

            var jobEvC = new CollisionJobEvC()
            {
                healthType = healthType,
                translationType = transformType,
                radius = radiusType,
                targetRecord = castleGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
                targetRadius = castleGroup.ToComponentDataArray<Radius>(Allocator.TempJob),
                targetTrans = castleGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
            };
            jobHandle = jobEvC.Schedule(enemyGroup, inputDependencies);
            jobHandle.Complete();
        }

        //For GameOver
        //if (Settings.IsPlayerDead())
        //	return jobHandle;
        return jobHandle;
    }

    //Common Function
    static float GetDistance(float3 posA, float3 posB)
    {
        float3 delta = posA - posB;
        return delta.x * delta.x + delta.z * delta.z;
    }

    static bool CheckCollision(float3 posA, float3 posB, float radius)
    {
        return GetDistance(posA, posB) <= radius * radius;
    }

    //Collision Job
    #region JobEvC
    [BurstCompile]
    struct CollisionJobEvC : IJobChunk
    {
        public ComponentTypeHandle<Health> healthType;
        [ReadOnly] public ComponentTypeHandle<Radius> radius;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;

        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<Damage> targetRecord;
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<Radius> targetRadius;
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<Translation> targetTrans;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkHealths = chunk.GetNativeArray(healthType);
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkRadius = chunk.GetNativeArray(radius);

            for (int j = 0; j < targetRecord.Length; j++)
            {
                int counter = (int)targetRecord[j].Value;
                if (counter < 0)
                    continue;
                Translation pos2 = targetTrans[j];

                for (int i = 0; i < chunk.Count; ++i)
                {
                    Health health = chunkHealths[i];
                    if (health.Value <= 0) continue;
                    Radius radius = chunkRadius[i];
                    Translation pos = chunkTranslations[i];
                    if (CheckCollision(pos.Value, pos2.Value, targetRadius[j].Value*0.5f + radius.Value))
                    {
                        counter--;
                        health.Value = 0;
                        chunkHealths[i] = health;
                    }

                    if (counter < 0)
                        break;
                }

            }
        }
    }
    #endregion

    #region JobCvE
    [BurstCompile]
    struct CollisionJobCvE : IJobChunk
    {
        [ReadOnly] public ComponentTypeHandle<Radius> radius;
        public ComponentTypeHandle<Health> healthType;
        public ComponentTypeHandle<Damage> damageRecord;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
    
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<Damage> targetDamage;
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<Radius> targetRadius;
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<Translation> targetTrans;
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<Health> targetHealth;
    
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkHealths = chunk.GetNativeArray(healthType);
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkRadius = chunk.GetNativeArray(radius);
            var chunkDamage = chunk.GetNativeArray(damageRecord);
    
            for (int i = 0; i < chunk.Count; ++i)
            {
                Health health = chunkHealths[i];
                if (health.Value <= 0) continue;
                Radius radius = chunkRadius[i];
                Translation pos = chunkTranslations[i];
                Damage damageRec = chunkDamage[i];
                damageRec.Value = 0;
    
                for (int j = 0; j < targetTrans.Length; j++)
                {
                    if (targetHealth[j].Value <= 0) continue;
                    Translation pos2 = targetTrans[j];
                    if (CheckCollision(pos.Value, pos2.Value, targetRadius[j].Value + radius.Value))
                    {
                        damageRec.Value += 1;
                        health.Value -= targetDamage[j].Value;
                    }
                }
    
                chunkHealths[i] = health;
                chunkDamage[i] = damageRec;
            }
        }
    }
    #endregion
}
