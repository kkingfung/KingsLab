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


public class EnemyToMeteor : JobComponentSystem
{
    EntityQuery enemyGroup;

    EntityQuery MeteorGroup;

    protected override void OnCreate()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        enemyGroup = GetEntityQuery(typeof(Health), typeof(Radius), typeof(Damage), typeof(SlowRate),
            typeof(PetrifyAmt), typeof(BuffTime),
            ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<EnemyTag>());

        MeteorGroup = GetEntityQuery(typeof(Radius), typeof(Damage), typeof(WaitingTime), typeof(SkillTag),
            ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<MeteorTag>());

        var transformType = GetComponentTypeHandle<Translation>(true);

        var healthType = GetComponentTypeHandle<Health>(false);
        var radiusType = GetComponentTypeHandle<Radius>(true);


        JobHandle jobHandle = inputDependencies;

        //enemy by meteor
        if (MeteorGroup.CalculateEntityCount() > 0 && enemyGroup.CalculateEntityCount() > 0)
        {
            var JobEvSM1 = new CollisionJobEvSM()
            {
                radiusType = radiusType,
                healthType = healthType,
                translationType = transformType,

                targetRadius = MeteorGroup.ToComponentDataArray<Radius>(Allocator.TempJob),
                targetDamage = MeteorGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
                targetTrans = MeteorGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
                targetWait = MeteorGroup.ToComponentDataArray<WaitingTime>(Allocator.TempJob)
            };
            jobHandle = JobEvSM1.Schedule(enemyGroup, inputDependencies);
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

    //enemy by meteor/minions
    #region JobEvSM
    [BurstCompile]
    struct CollisionJobEvSM : IJobChunk
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
                    if (targetWait[j].Value > 0) continue;

                    Translation pos2 = targetTrans[j];

                    if (CheckCollision(pos.Value, pos2.Value, targetRadius[j].Value + radius.Value))
                    {
                        //Debug.DrawLine(pos.Value, pos.Value + new float3(0, 1, 0), Color.red);
                        damage += targetDamage[j].Value;
                        //Debug.Log("Damaged");
                    }
                    else
                    {
                        //Debug.DrawLine(pos.Value, pos.Value + new float3(0, 1, 0), Color.green);
                        //Debug.Log(GetDistance(pos.Value, pos2.Value));
                        //Debug.Log(pos.Value);
                        //Debug.Log(pos2.Value);
                        //Debug.Log("NotHitted");
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