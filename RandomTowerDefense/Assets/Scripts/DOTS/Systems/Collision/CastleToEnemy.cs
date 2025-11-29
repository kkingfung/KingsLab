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
using RandomTowerDefense.DOTS.Components;

/// <summary>
/// 城と敵の双方向衝突を処理するECSシステム
/// 敵の接触による城のダメージと城の近接による敵の排除を両方処理
/// ダメージ記録システムを使用して城との敵の相互作用を追跡・制限
/// </summary>
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

            // 敵の衝突による城のダメージを処理
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

            // 城の衝突による敵のダメージを処理
            var jobEvC = new CollisionJobEvC()
            {
                healthType = healthType,
                translationType = transformType,
                radius = radiusType,
                targetRecord = castleGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
                targetRadius = castleGroup.ToComponentDataArray<Radius>(Allocator.TempJob),
                targetTrans = castleGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
            };
            jobHandle = jobEvC.Schedule(enemyGroup, jobHandle);
        }

        return jobHandle;
    }


    /// <summary>
    /// 敵対城の衝突を処理するジョブ
    /// </summary>
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
                    if (CollisionUtilities.CheckCollision(pos.Value, pos2.Value, (targetRadius[j].Value * 0.5f + radius.Value) * (targetRadius[j].Value * 0.5f + radius.Value)))
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

    /// <summary>
    /// 城対敵の衝突を処理するジョブ
    /// </summary>
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
                    if (CollisionUtilities.CheckCollision(pos.Value, pos2.Value, targetRadius[j].Value + radius.Value))
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
