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
using Unity.Rendering;
using Unity.Mathematics;
using System;

public class DOTSManager : MonoBehaviour
{
    private readonly int DynamicBufferNum = 1000;
    private bool UseEntityComponentSystem = false;
    private bool UseJobSystem = false;
    private bool UseEventSystem = false;
    private bool UseDynamicBuffer = false;

    public List<Transform> Objs;

    // Start is called before the first frame update
    void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (UseEntityComponentSystem)
        {
            EntityArchetype entityArchetype = entityManager.CreateArchetype(typeof(MyEntityComponentSystem.MyComponent), typeof(LocalToWorld));
            NativeArray<Entity> entityArray = new NativeArray<Entity>(Objs.Count, Allocator.Temp);
            entityManager.CreateEntity(entityArchetype, entityArray);

            for (int i = 0; i < entityArray.Length; i++)
            {
                Entity entity = entityArray[i];
                entityManager.SetComponentData(entity, new MyEntityComponentSystem.MyComponent { any = 0 });

                //If render is needed
                //entityManager.SetSharedComponentData(entity,new RenderMesh { mesh=??,Material=??});
            }

            entityArray.Dispose();
        }

        if (UseEventSystem) 
        {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MyEventSystem>().OnMyEvent += ProceedEvent;
        }

        if (UseDynamicBuffer)
        {
            Entity entity = entityManager.CreateEntity();
            entityManager.AddBuffer<EnmBufferElement>(entity);
            DynamicBuffer<EnmBufferElement> dynamicBuffer = entityManager.GetBuffer<EnmBufferElement>(entity);

            //For Sample ONLY
            //for (int i = 0; i < DynamicBufferNum; ++i)
            //    dynamicBuffer.Add(new IntBufferElement { targetEntity = Entity.Null }); ;

            //When USE
            //DynamicBuffer<int> intDynamicBuffer = dynamicBuffer.Reinterpret<int>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (UseJobSystem)
        {
            //Not necessary for IJobParallelForTransform
            NativeArray<float3> positionArray = new NativeArray<float3>(Objs.Count, Allocator.TempJob);
            TransformAccessArray transformAccessArray = new TransformAccessArray(Objs.Count);

            for (int i = 0; i < Objs.Count; i++)
            {
                //Not necessary for IJobParallelForTransform
                positionArray[i] = Objs[i].transform.position;
                transformAccessArray.Add(Objs[i].transform);
            }

            MyJobSystem.MyJob Job = new MyJobSystem.MyJob
            {
                //If exists
            };
            
            JobHandle jobHandle = Job.Schedule();
            jobHandle.Complete();

            for (int i = 0; i < Objs.Count; i++)
            {
                //Not necessary for IJobParallelForTransform
                Objs[i].transform.position = positionArray[i];
            }

            positionArray.Dispose();
            transformAccessArray.Dispose();
        }

    }
    private void ProceedEvent(object sender, System.EventArgs e) {

    }
}

#region EventSystem

#if true
public class MyEventSystem : JobComponentSystem
{
    public event EventHandler OnMyEvent;
    public struct EventComponent : IComponentData {
        public double ElapsedTime;
    }
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    //Can be directly use properties instead
    public struct MyEntityStruct:IComponentData
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;
        double elapseTime = Time.ElapsedTime;

        EntityCommandBuffer entityCommandBuffer = //new EntityCommandBuffer(Allocator.TempJob);
            endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        EntityCommandBuffer.ParallelWriter entityCommandBufferConcurrent = entityCommandBuffer.AsParallelWriter();
        EntityArchetype entityArchetype = EntityManager.CreateArchetype(typeof(EventComponent));

        double ElapsedTime = Time.ElapsedTime;

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref MyEntityStruct myEntity) =>
        {
            Entity eventEntity =  entityCommandBufferConcurrent.CreateEntity(entityInQueryIndex, entityArchetype);
            entityCommandBufferConcurrent.SetComponent(entityInQueryIndex, eventEntity, new EventComponent
            {
                ElapsedTime = ElapsedTime
            });
        }).Schedule(inputDeps);

        //jobHandle.Complete();
        //entityCommandBuffer.Playback(EntityManager);
        //entityCommandBuffer.Dispose();
        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        EntityCommandBuffer captureEventsEntityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        Entities.WithoutBurst().ForEach((Entity entity, ref EventComponent eventComponent) =>
        {
            OnMyEvent?.Invoke(this,EventArgs.Empty);
            captureEventsEntityCommandBuffer.DestroyEntity(entity);
        }).Run();

        EntityManager.DestroyEntity(GetEntityQuery(typeof(EventComponent)));

        return jobHandle;
    }
}

#else

public class MyEventSystem : JobComponentSystem
{
    public event EventHandler OnMyEvent;
    public struct MyEvent
    {
    }

    //Can be directly use properties instead
    public struct MyEntityStruct
    {
    }

    private NativeQueue<MyEvent> eventQueue;

    protected override void OnCreate()
    {
        eventQueue = new NativeQueue<MyEvent>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        eventQueue.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        NativeQueue<MyEvent>.ParallelWriter eventQueueParallel = eventQueue.AsParallelWriter();

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref MyEntityStruct myEntity) =>
        {

            if (true)
            {
                eventQueueParallel.Enqueue(new MyEvent { });
            }
        }).Schedule(inputDeps);

        jobHandle.Complete();

        while (eventQueue.TryDequeue(out MyEvent myEvent))
        {
            OnMyEvent?.Invoke(this, EventArgs.Empty);
        }

        return jobHandle;
    }
}

//[UpdateInGroup(typeof(LateSimulationSystemGroup))]
//public class MySubEventSystem : JobComponentSystem
//{
//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {

//    }
//}
#endif

#endregion

#region EntityComponentSystem

public class MyEntityComponentSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref MyComponent component) => {
            //any update
        });
    }

    public struct MyComponent : IComponentData
    {
        public int any;
    }
}

public class BufferFromEntitySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<Tag_Player>().ForEach((Entity playerEntity) => {

            BufferFromEntity<EnmBufferElement> intBufferFromEntity = GetBufferFromEntity<EnmBufferElement>();

            Entity enemyEntity = Entity.Null;

            Entities.WithAll<Tag_Enemy>().ForEach((Entity enemyEntityTmp) =>
            {
                enemyEntity = enemyEntityTmp;
            });

            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            Entities.ForEach((DynamicBuffer<EnmBufferElement> targetDynamicBuffer) =>
            {
                for (int i = 0; i < targetDynamicBuffer.Length; i++)
                {
                    Entity targetEntity = targetDynamicBuffer[i].targetEntity;

                    if (targetEntity != Entity.Null && EntityManager.Exists(targetEntity))
                    {
                        // Has Target
                        ComponentDataFromEntity<Translation> translationComponentData = GetComponentDataFromEntity<Translation>(true);
                        float3 targetPosition = translationComponentData[targetEntity].Value;

                        //Entity kunaiEntity = entityCommandBuffer.Instantiate();
                        //entityCommandBuffer.SetComponent();

                    }
                }
            });
            entityCommandBuffer.Playback(EntityManager);

        });
    }
}

#endregion

#region JobSystem
public class MyJobSystem
{
    [BurstCompile]
    public struct MyJob : IJob
    {
        public void Execute()
        {
            //any update
        }
    }

    [BurstCompile]
    public struct MyJobParallel : IJobParallelFor
    {
        public NativeArray<float3> Array;
        [ReadOnly] public float deltaTime;
        public void Execute(int index)//Execute()//Execute(int index) // Execute(int index,TransformAccess transform)
        {
            //any update for Array[index]
        }
    }

    [BurstCompile]
    public struct MyJobParallelTransform : IJobParallelForTransform
    {
        public NativeArray<float3> Array;
        [ReadOnly] public float deltaTime;

        public void Execute(int index, TransformAccess transform)
        {
            //any update for transform on Array[index]
        }

    }

    public class BufferJobSystem : JobComponentSystem
    {
        public struct BufferJob : IJobForEachWithEntity_EB<EnmBufferElement>
        {

            public void Execute(Entity entity, int index, DynamicBuffer<EnmBufferElement> dynamicBuffer)
            {
                for (int i = 0; i < dynamicBuffer.Length; i++)
                {
                    EnmBufferElement intBufferElement = dynamicBuffer[i];

                    //code

                    dynamicBuffer[i] = intBufferElement;
                }
            }

        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new BufferJob().Schedule(this, inputDeps);
        }
    }


    #endregion
}
