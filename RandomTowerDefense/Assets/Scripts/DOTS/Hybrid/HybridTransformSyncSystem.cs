//using System.Diagnostics;

//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Transforms;
//using UnityEngine.Jobs;

//[BurstCompile]
//struct TransformSyncJob : IJobParallelForTransform
//{
//    [DeallocateOnJobCompletion]
//    public NativeArray<LocalToWorld> LocalToWorldArray;

//    public void Execute(int index, TransformAccess transform)
//    {
//        transform.position = LocalToWorldArray[index].Position;
//        transform.rotation = LocalToWorldArray[index].Rotation;
//    }
//}

//[UpdateAfter(typeof(TransformSystemGroup))]
//public class HybridTransformSyncSystem : JobComponentSystem
//{
//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
        //var assignPositionsJobHandle=inputDeps;

        ////Enemy
        //if (EnemySpawner.Instance != null)
        //{
        //    var transformAccessArray = EnemySpawner.Instance.TransformAccessArray;
        //    var entities = EnemySpawner.Instance.Entities;
        //    var localToWorldData = GetComponentDataFromEntity<LocalToWorld>(true);
        //    var hybridData = GetComponentDataFromEntity<Hybrid>(true);
        //    var localToWorldArray = new NativeArray<LocalToWorld>(entities.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        //    var gatherPositionsJobHandle = Job.WithCode(() =>
        //    {
        //        for (int i = 0; i < entities.Length; i++)
        //        {
        //            if (EnemySpawner.Instance.GameObjects[i] != null)
        //            {
        //                localToWorldArray[hybridData[entities[i]].Index] = localToWorldData[entities[i]];
        //            }
        //        }
        //    })
        //       .WithReadOnly(hybridData)
        //       .WithReadOnly(localToWorldData)
        //       .WithoutBurst()
        //       .Schedule(inputDeps);

        //    assignPositionsJobHandle = new TransformSyncJob
        //    {
        //        LocalToWorldArray = localToWorldArray,
        //    }.Schedule(transformAccessArray, gatherPositionsJobHandle);
        //}

        ////Tower
        //if (TowerSpawner.Instance != null)
        //{
        //    var transformAccessArray = TowerSpawner.Instance.TransformAccessArray;
        //    var entities = TowerSpawner.Instance.Entities;
        //    var localToWorldData = GetComponentDataFromEntity<LocalToWorld>(true);
        //    var hybridData = GetComponentDataFromEntity<Hybrid>(true);
        //    var localToWorldArray = new NativeArray<LocalToWorld>(entities.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        //    var gatherPositionsJobHandle = Job.WithCode(() =>
        //    {
        //        for (int i = 0; i < entities.Length; i++)
        //        {
        //            if (TowerSpawner.Instance.GameObjects[i] != null)
        //                localToWorldArray[hybridData[entities[i]].Index] = localToWorldData[entities[i]];
        //        }
        //    })
        //       .WithReadOnly(hybridData)
        //       .WithReadOnly(localToWorldData)
        //       .WithoutBurst()
        //       .Schedule(inputDeps);

        //    assignPositionsJobHandle = new TransformSyncJob
        //    {
        //        LocalToWorldArray = localToWorldArray,
        //    }.Schedule(transformAccessArray, gatherPositionsJobHandle);
        //}
//        return inputDeps;
//    }
//}

//EntityManager.AddComponentObject(entity, gameObject.GetComponent<Transform>());

//// Choose which is right for you
//EntityManager.AddComponentData(entity, new CopyTransformToGameObject());

//// Or
//EntityManager.AddComponentData(entity, new CopyTransformFromGameObject());