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

public class BoidDataUpdateSystem : JobComponentSystem
{
    EntityQuery boidGroup;

    protected override void OnCreate()
    {
        boidGroup = GetEntityQuery(typeof(BoidData), typeof(Velocity),
            typeof(OriPos), typeof(BoidSettingDots), typeof(LocalToWorld),
            ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<BoidTag>());
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        var transformType = GetComponentTypeHandle<Translation>(true);
        var dataType = GetComponentTypeHandle<BoidData>(true);
        var settingType = GetComponentTypeHandle<BoidSettingDots>(false);

        JobHandle jobHandle = inputDeps;

        if (boidGroup.CalculateEntityCount() > 0)
        {
            var jobData = new UpdateBoidData()
            {
                dataType = dataType,
                translationType = transformType,
                settingType = settingType,
                targetTrans = boidGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
                targetVec = boidGroup.ToComponentDataArray<Velocity>(Allocator.TempJob)
            };
            jobHandle = jobData.Schedule(boidGroup, inputDeps);
            jobHandle.Complete();
        }
        return jobHandle;
    }

    [BurstCompile]
    struct UpdateBoidData : IJobChunk
    {
        public ComponentTypeHandle<BoidData> dataType;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        [ReadOnly] public ComponentTypeHandle<BoidSettingDots> settingType;

        [DeallocateOnJobCompletion]
        public NativeArray<Translation> targetTrans;
        [DeallocateOnJobCompletion]
        public NativeArray<Velocity> targetVec;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkData = chunk.GetNativeArray(dataType);
            var chunkTranslations = chunk.GetNativeArray(translationType);
            var chunkSetting = chunk.GetNativeArray(settingType);

            for (int i = 0; i < chunk.Count; ++i)
            {
                Translation translation = chunkTranslations[i];
                BoidData data = chunkData[i];
                BoidSettingDots setting = chunkSetting[i];

                for (int j = 0; j < targetTrans.Length; j++)
                {
                    if (i == j) continue;
                    float3 offset = translation.Value - targetTrans[j].Value;
                    float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                    if (sqrDst < setting.perceptionRadius * setting.perceptionRadius)
                    {
                        data.numFlockmates += 1;
                        Vector3 forward= new Vector3(targetVec[j].Value.x,
                            targetVec[j].Value.y, targetVec[j].Value.z).normalized;
                        data.flockHeading += new float3(forward.x, forward.y, forward.z);
                        data.flockCentre += targetTrans[j].Value;
                        if (sqrDst < setting.avoidanceRadius * setting.avoidanceRadius)
                        {
                            data.avoidanceHeading -= offset / sqrDst;
                        }
                    }
                }
                chunkData[i] = data;
            }
        }
    }
}
