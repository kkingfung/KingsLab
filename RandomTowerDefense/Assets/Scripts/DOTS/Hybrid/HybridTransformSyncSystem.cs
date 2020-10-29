using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine.Jobs;

[BurstCompile]
struct TransformSyncJob : IJobParallelForTransform
{
    [DeallocateOnJobCompletion]
    public NativeArray<LocalToWorld> LocalToWorldArray;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position = LocalToWorldArray[index].Position;
        transform.rotation = LocalToWorldArray[index].Rotation;
    }
}

[UpdateAfter(typeof(TransformSystemGroup))]
public class HybridTransformSyncSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (HybridSpawner.Instance == null)
            return inputDeps;

        var transformAccessArray = HybridSpawner.Instance.TransformAccessArray;
        var entities = HybridSpawner.Instance.Entities;
        var localToWorldData = GetComponentDataFromEntity<LocalToWorld>(true);
        var hybridData = GetComponentDataFromEntity<Hybrid>(true);
        var localToWorldArray = new NativeArray<LocalToWorld>(entities.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        var gatherPositionsJobHandle = Job.WithCode(() =>
        {
            for (int i = 0; i < entities.Length; i++)
            {
                localToWorldArray[hybridData[entities[i]].Index] = localToWorldData[entities[i]];
            }
        })
           .WithReadOnly(hybridData)
           .WithReadOnly(localToWorldData)
           .WithBurst()
           .Schedule(inputDeps);

        var assignPositionsJobHandle = new TransformSyncJob
        {
            LocalToWorldArray = localToWorldArray,
        }.Schedule(transformAccessArray, gatherPositionsJobHandle);

        return assignPositionsJobHandle;
    }
}