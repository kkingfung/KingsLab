using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public class TowerSpawner : MonoBehaviour
{
    private readonly int count = 500;
    public static TowerSpawner Instance { get; private set; }
    public List<GameObject> PrefabObject;

    EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

    //Array
    [HideInInspector]
    public TransformAccessArray TransformAccessArray;
    public NativeArray<float> waitArray;
    public NativeArray<float> lifetimeArray;
    public NativeArray<float> radiusArray;
    public NativeArray<float3> targetArray;
    public NativeArray<bool> hastargetArray;

    //Bridge
    [HideInInspector]
    public GameObject[] GameObjects;
    public NativeArray<Entity> Entities;

    //For input
    private Transform[] transforms;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void OnDisable()
    {
        if (Entities.IsCreated)
            Entities.Dispose();

        if (TransformAccessArray.isCreated)
            TransformAccessArray.Dispose();

        //Disposing Array
        waitArray.Dispose();
        lifetimeArray.Dispose();
        radiusArray.Dispose();
        targetArray.Dispose();
        hastargetArray.Dispose();
    }
    void Start()
    {
        //Prepare input
        GameObjects = new GameObject[count];
        transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
            typeof(PlayerTag), typeof(WaitingTime), typeof(Lifetime), typeof(Radius),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Hybrid>());
        EntityManager.CreateEntity(archetype, Entities);

        TransformAccessArray = new TransformAccessArray(transforms);
        waitArray = new NativeArray<float>(count, Allocator.Persistent);
        lifetimeArray = new NativeArray<float>(count, Allocator.Persistent);
        radiusArray = new NativeArray<float>(count, Allocator.Persistent);
        targetArray = new NativeArray<float3>(count, Allocator.Persistent);
        hastargetArray = new NativeArray<bool>(count, Allocator.Persistent);
    }

    public int[] Spawn(int prefabID, float3 Position, float3 Rotation, float radius,
        float wait, float lifetime,  int num = 1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];
        for (int i = 0; i < count && spawnCnt < num; i++)
        {
            if (GameObjects[i] != null) continue;
            bool hastarget = new bool();
            hastarget = false;
            float3 targetpos = new float3(Position);

            GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
            GameObjects[i].transform.position = Position;
            GameObjects[i].transform.rotation = Quaternion.Euler(Rotation);
            transforms[i] = GameObjects[i].transform;
            waitArray[i] = wait;
            lifetimeArray[i] = lifetime;
            radiusArray[i] = radius;
            hastargetArray[i] = hastarget;
            targetArray[i] = targetpos;


            //AddtoEntities
            EntityManager.SetComponentData(Entities[i], new WaitingTime
            {
                Value = wait,
            });
            EntityManager.SetComponentData(Entities[i], new Lifetime
            {
                Value = lifetime,
            });
            EntityManager.SetComponentData(Entities[i], new Radius
            {
                Value = radius,
            });
            EntityManager.SetComponentData(Entities[i], new TargetPos
            {
                Value = targetpos,
            });
            EntityManager.SetComponentData(Entities[i], new TargetFound
            {
                Value = hastarget,
            });

            EntityManager.SetComponentData(Entities[i], new Translation
            {
                Value = Position,
            });

            EntityManager.SetComponentData(Entities[i], new Hybrid
            {
                Index = i,
            });

            spawnCnt++;
            spawnIndexList[i] = i;
        }

        //Change Whenever Spawned
        TransformAccessArray = new TransformAccessArray(transforms);
        return spawnIndexList;
    }
}
