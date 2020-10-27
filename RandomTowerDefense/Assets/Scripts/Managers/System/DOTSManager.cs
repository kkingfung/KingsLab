using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Burst;

public class DOTSManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        #region ECS (with componentsystem, no class but struct)
        //EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //EntityArchetype entityArchetype = entityManager.CreateArchetype( typeof(LocalToWorld));
        //NativeArray<Entity> entityArray = new NativeArray<Entity>(1, Allocator.Temp);
        //entityManager.CreateEntity(entityArchetype, entityArray);


        //for (int i = 0; i < entityArray.Length; i++) {
        //    Entity entity = entityArray[i];
        //    entityManager.SetComponentData(entity, new );
        //    entityManager.SetSharedComponentData(entity,);

        //}

        //entityArray.Dispose();
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        #region JobSsytem
        ToughJob job = new ToughJob();
        job.Schedule(); // Give JobHandle (JobHandle.Complete();JobHandle.CompleteAll(NativeList<JobHandle>);)
        #endregion

    }


}

#region JobSystem

[BurstCompile]
public struct ToughJob : IJob
{//IJobParallelFor // IJobParallelForTransform(TransformAccessArray-ref:AutoUpdated)
    public void Execute()//Execute(int index) // Execute(int index,TransformAccess transform)
    { 

    }
}
    #endregion