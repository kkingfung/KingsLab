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
using Unity.Collections.LowLevel.Unsafe;

public class BoidDataUpdateSystem : ComponentSystem
{
    EntityQuery boidGroup;

    protected override void OnCreate()
    {
        boidGroup = GetEntityQuery(typeof(BoidData), 
            ComponentType.ReadOnly<BoidTag>());
    }

    protected override void OnUpdate()
    {
        NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);

        NativeArray<BoidData> targetDataArray = boidGroup.ToComponentDataArray<BoidData>(Allocator.Temp);

        if (boidGroup.CalculateEntityCount() > 0)
        {
            Entities.WithAll<BoidTag>().ForEach((Entity entity) =>
            {
                NativeArray<BoidData> targetData = new NativeArray<BoidData>(targetDataArray, Allocator.TempJob);

                var dataType = GetComponentTypeHandle<BoidData>(false);
                var settingType = GetComponentTypeHandle<BoidSettingDots>(true);

                var jobData = new UpdateBoidData()
                {
                    dataType = dataType,
                    settingType = settingType,
                    targetData = targetData
                };

                jobHandleList.Add(jobData.Schedule(boidGroup));
            });
        }

        JobHandle.CompleteAll(jobHandleList);
    }

    [BurstCompile]
    struct UpdateBoidData : IJobChunk
    {
        [NativeDisableContainerSafetyRestriction] public ComponentTypeHandle<BoidData> dataType;
        [ReadOnly] public ComponentTypeHandle<BoidSettingDots> settingType;

        [DeallocateOnJobCompletion]
        public NativeArray<BoidData> targetData;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkData = chunk.GetNativeArray(dataType);
            var chunkSetting = chunk.GetNativeArray(settingType);

            for (int i = 0; i < chunk.Count; ++i)
            {
                BoidData data = chunkData[i];
                BoidSettingDots setting = chunkSetting[i];

                for (int j = 0; j < targetData.Length; j++)
                {
                    if (i == j) continue;
                    float3 offset = targetData[j].position - data.position;
                    float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                    if (sqrDst < setting.perceptionRadius * setting.perceptionRadius)
                    {
                        data.numFlockmates += 1;
                        data.flockHeading += targetData[j].direction;
                        data.flockCentre += targetData[j].position;

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
