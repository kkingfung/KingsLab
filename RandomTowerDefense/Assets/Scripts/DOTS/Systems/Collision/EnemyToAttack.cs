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
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Systems;

/// <summary>
/// 敵エンティティとタワーの攻撃エンティティの衝突を検出し処理するシステム
/// タワー攻撃による敵へのダメージ適用を管理
/// </summary>
public class EnemyToAttack : JobComponentSystem
{
    EntityQuery enemyGroup;

    EntityQuery AttackGroup;
    protected override void OnCreate()
    {
    }

    /// <summary>
    /// 敵エンティティとタワー攻撃の衝突を処理
    /// </summary>
    /// <param name="inputDependencies">入力依存関係</param>
    /// <returns>ジョブハンドル</returns>
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        enemyGroup = GetEntityQuery(typeof(Health), typeof(Radius), typeof(Damage), typeof(SlowRate),
            typeof(PetrifyAmt), typeof(BuffTime),
            ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<EnemyTag>());

        AttackGroup = GetEntityQuery(typeof(Radius), typeof(Damage), typeof(ActionTime), typeof(WaitingTime),
            ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<AttackTag>());

        var transformType = GetComponentTypeHandle<Translation>(true);

        var healthType = GetComponentTypeHandle<Health>(false);
        var radiusType = GetComponentTypeHandle<Radius>(true);

        JobHandle jobHandle = inputDependencies;

        //enemy by Attack
        if (AttackGroup.CalculateEntityCount() > 0 && enemyGroup.CalculateEntityCount() > 0)
        {
            var jobEvA = new CollisionJobEvA()
            {
                radiusType = radiusType,
                healthType = healthType,
                translationType = transformType,

                targetRadius = AttackGroup.ToComponentDataArray<Radius>(Allocator.TempJob),
                targetDamage = AttackGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
                targetTrans = AttackGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
                targetAction = AttackGroup.ToComponentDataArray<ActionTime>(Allocator.TempJob),
                targetWait = AttackGroup.ToComponentDataArray<WaitingTime>(Allocator.TempJob)
            };
            jobHandle = jobEvA.Schedule(enemyGroup, inputDependencies);
        }
        return jobHandle;
    }


    #region JobEvA
    [BurstCompile]
    struct CollisionJobEvA : IJobChunk
    {
        [ReadOnly] public ComponentTypeHandle<Radius> radiusType;
        public ComponentTypeHandle<Health> healthType;
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
        public NativeArray<ActionTime> targetAction;
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<WaitingTime> targetWait;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkHealths = chunk.GetNativeArray(healthType);
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkRadius = chunk.GetNativeArray(radiusType);

            for (int i = 0; i < chunk.Count; ++i)
            {
                float damage = 0;
                Health health = chunkHealths[i];
                if (health.Value <= 0) continue;
                Radius radius = chunkRadius[i];
                Translation pos = chunkTranslations[i];

                for (int j = 0; j < targetTrans.Length; j++)
                {
                    if (targetAction[j].Value <= 0) continue;
                    if (targetWait[j].Value > 0) continue;

                    Translation pos2 = targetTrans[j];

                    if (CollisionUtilities.CheckCollision(pos.Value, pos2.Value, targetRadius[j].Value + radius.Value))
                    {
                        //Debug.DrawLine(pos.Value, pos.Value + new float3(0, 1, 0), Color.red);
                        damage += targetDamage[j].Value;
                    }
                    else
                    {
                        //Debug.DrawLine(pos.Value, pos.Value + new float3(0, 1, 0), Color.green);
                    }
                }

                if (damage > 0)
                {
                    health.Value -= damage;
                    chunkHealths[i] = health;
                }
            }
        }
    }
    #endregion
}