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


public class EnemyToPetrification : JobComponentSystem
{
    EntityQuery enemyGroup;
    EntityQuery PetrificationGroup;

    protected override void OnCreate()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        enemyGroup = GetEntityQuery(typeof(Health), typeof(Radius), typeof(Damage),
            typeof(PetrifyAmt), typeof(BuffTime),
            ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<EnemyTag>());

        PetrificationGroup = GetEntityQuery(typeof(Radius), typeof(Damage), typeof(WaitingTime), typeof(SlowRate), typeof(BuffTime), typeof(SkillTag),
            ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PetrificationTag>());

        var healthType = GetComponentTypeHandle<Health>(false);
        var petrifyType = GetComponentTypeHandle<PetrifyAmt>(false);
        var buffType = GetComponentTypeHandle<BuffTime>(false);

        JobHandle jobHandle = inputDependencies;

        //enemy by petrification
        if (PetrificationGroup.CalculateEntityCount() > 0 && enemyGroup.CalculateEntityCount() > 0)
        {
            var jobEvSP = new CollisionJobEvSP()
            {
                petrifyType = petrifyType,
                buffType = buffType,
                healthType = healthType,
                targetPetrify = PetrificationGroup.ToComponentDataArray<SlowRate>(Allocator.TempJob),
                targetBuff = PetrificationGroup.ToComponentDataArray<BuffTime>(Allocator.TempJob),
            };
            jobHandle = jobEvSP.Schedule(enemyGroup, inputDependencies);
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

    //enemy by petrification
    #region JobEvSP
    [BurstCompile]
    struct CollisionJobEvSP : IJobChunk
    {
        public ComponentTypeHandle<Health> healthType;
        public ComponentTypeHandle<PetrifyAmt> petrifyType;
        public ComponentTypeHandle<BuffTime> buffType;

        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<SlowRate> targetPetrify;
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<BuffTime> targetBuff;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkHP = chunk.GetNativeArray(healthType);
            var chunkPetrify = chunk.GetNativeArray(petrifyType);
            var chunkBuff = chunk.GetNativeArray(buffType);
            for (int i = 0; i < chunk.Count; ++i)
            {
                Health health = chunkHP[i];
                if (health.Value <= 0) continue;
                PetrifyAmt petrifyAmt = chunkPetrify[i];
                BuffTime buff = chunkBuff[i];
                if (buff.Value >= 1) continue;

                petrifyAmt.Value = targetPetrify[0].Value;
                buff.Value += targetBuff[0].Value;

                //Constraint according to FPS
                if (buff.Value > 1)
                    buff.Value = 1;
                //Debug.Log("Petrified");

                chunkPetrify[i] = petrifyAmt;
                chunkBuff[i] = buff;
            }
        }
    }
    #endregion

}
