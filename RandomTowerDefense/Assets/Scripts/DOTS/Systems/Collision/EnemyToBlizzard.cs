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


public class EnemyToBlizzard : JobComponentSystem
{
    EntityQuery enemyGroup;

    EntityQuery BlizzardGroup;

    protected override void OnCreate()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        enemyGroup = GetEntityQuery(typeof(Health), typeof(Radius), typeof(Damage), typeof(SlowRate), typeof(BuffTime),
            ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<EnemyTag>());

        BlizzardGroup = GetEntityQuery(typeof(Radius), typeof(Damage), typeof(WaitingTime), typeof(SlowRate), typeof(BuffTime), typeof(SkillTag),
            ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<BlizzardTag>());

        var transformType = GetComponentTypeHandle<Translation>(true);

        var healthType = GetComponentTypeHandle<Health>(true);
        var radiusType = GetComponentTypeHandle<Radius>(true);

        var slowType = GetComponentTypeHandle<SlowRate>(false);
        var buffType = GetComponentTypeHandle<BuffTime>(false);

        JobHandle jobHandle = inputDependencies;
        //enemy by blizzard
        if (BlizzardGroup.CalculateEntityCount() > 0 && enemyGroup.CalculateEntityCount() > 0)
        {
            var jobEvSB = new CollisionJobEvSB()
            {
                radiusType = radiusType,
                healthType = healthType,
                translationType = transformType,

                targetRadius = BlizzardGroup.ToComponentDataArray<Radius>(Allocator.TempJob),
                targetDamage = BlizzardGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
                targetTrans = BlizzardGroup.ToComponentDataArray<Translation>(Allocator.TempJob),

                slowType = slowType,
                buffType = buffType,
                targetSlow = BlizzardGroup.ToComponentDataArray<SlowRate>(Allocator.TempJob),
                targetBuff = BlizzardGroup.ToComponentDataArray<BuffTime>(Allocator.TempJob)
            };
            jobHandle = jobEvSB.Schedule(enemyGroup, inputDependencies);
            jobHandle.Complete();
        }

        return jobHandle;
    }

    //Common Function
    static float GetDistance(float3 posA, float3 posB)
    {
        float3 delta = posA - posB;
        return delta.x * delta.x + delta.z * delta.z;
    }

    static bool CheckCollision(float3 posA, float3 posB, float radiusSqr)
    {
        return GetDistance(posA, posB) <= radiusSqr;
    }

    //enemy by blizzard
    #region JobEvSB
    [BurstCompile]
    struct CollisionJobEvSB : IJobChunk
    {

        [ReadOnly] public ComponentTypeHandle<Radius> radiusType;
        [ReadOnly] public ComponentTypeHandle<Health> healthType;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<SlowRate> slowType;
        public ComponentTypeHandle<BuffTime> buffType;

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
        public NativeArray<SlowRate> targetSlow;
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<BuffTime> targetBuff;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkHealths = chunk.GetNativeArray(healthType);
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkRadius = chunk.GetNativeArray(radiusType);

            var chunkSlow = chunk.GetNativeArray(slowType);
            var chunkBuff = chunk.GetNativeArray(buffType);

            for (int i = 0; i < chunk.Count; ++i)
            {
                float damage = 0;
                Health health = chunkHealths[i];
                if (health.Value <= 0) continue;
                Radius radius = chunkRadius[i];
                Translation pos = chunkTranslations[i];
                SlowRate slow = chunkSlow[i];
                BuffTime buff = chunkBuff[i];

                for (int j = 0; j < targetTrans.Length; j++)
                {
                    Translation pos2 = targetTrans[j];

                    if (CheckCollision(pos.Value, pos2.Value, targetRadius[j].Value + radius.Value))
                    {
                        //Debug.DrawLine(pos.Value, pos.Value + new float3(0, 1, 0), Color.red);
                        damage += 1;
                        slow.Value = Mathf.Clamp(slow.Value + targetSlow[j].Value, 0, 0.95f);
                        if (buff.Value < targetBuff[j].Value) buff.Value = targetBuff[j].Value;
                        //Debug.Log("Slowed");
                        break;
                    }
                    //else 
                    //Debug.DrawLine(pos.Value, pos.Value + new float3(0, 1, 0), Color.green);
                }

                if (damage > 0)
                {
                    //health.Value -= damage;
                    //chunkHealths[i] = health;
                    chunkSlow[i] = slow;
                    chunkBuff[i] = buff;
                }
            }
        }
    }
    #endregion
}