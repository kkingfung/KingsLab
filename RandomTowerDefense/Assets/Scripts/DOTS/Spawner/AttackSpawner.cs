using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public class AttackSpawner : MonoBehaviour
{
    private readonly int count=1000;
    public static AttackSpawner Instance { get; private set; }
    public List<GameObject> PrefabObject;

    private EntityManager EntityManager;

    //Array
    [HideInInspector]
    public TransformAccessArray TransformAccessArray;
    public NativeArray<float> damageArray;
    public NativeArray<float> radiusArray;
    public NativeArray<float> waitArray;
    public NativeArray<float> lifetimeArray;
    public NativeArray<float> actionArray;

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
        if (damageArray.IsCreated)
            damageArray.Dispose();
        if (radiusArray.IsCreated)
            radiusArray.Dispose();
        if (waitArray.IsCreated) 
            waitArray.Dispose();
        if (lifetimeArray.IsCreated) 
            lifetimeArray.Dispose();
        if (actionArray.IsCreated) 
            actionArray.Dispose();
    }
    void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //Prepare input
        GameObjects = new GameObject[count];
        transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
             typeof(Radius), typeof(Damage),
             typeof(WaitingTime), typeof(Lifetime), typeof(ActionTime),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Hybrid>());
        EntityManager.CreateEntity(archetype, Entities);

        TransformAccessArray = new TransformAccessArray(transforms);
        damageArray = new NativeArray<float>(count, Allocator.Persistent);
        radiusArray = new NativeArray<float>(count, Allocator.Persistent);
        waitArray = new NativeArray<float>(count, Allocator.Persistent);
        lifetimeArray = new NativeArray<float>(count, Allocator.Persistent);
        actionArray = new NativeArray<float>(count, Allocator.Persistent);
    }

    public int[] Spawn(int prefabID, float3 Position, Quaternion Rotation, float damage, 
        float radius, float wait, float lifetime, float action, int num = 1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];
        for (int i = 0; i < count && spawnCnt < num; i++)
        {
            if (GameObjects[i] != null) continue;
            GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
            GameObjects[i].transform.position = Position;
            GameObjects[i].transform.localRotation = Rotation;
            transforms[i] = GameObjects[i].transform;
            damageArray[i] = damage;
            radiusArray[i] = radius;
            waitArray[i] = wait;
            lifetimeArray[i] = lifetime;
            actionArray[i] = action;
           
            //AddtoEntities
            EntityManager.SetComponentData(Entities[i], new Damage
            {
                Value = damage,
            });
            EntityManager.SetComponentData(Entities[i], new Radius
            {
                Value = radius,
            });
            EntityManager.SetComponentData(Entities[i], new WaitingTime
            {
                Value = wait,
            });
            EntityManager.SetComponentData(Entities[i], new Lifetime
            {
                Value = lifetime,
            });
            EntityManager.SetComponentData(Entities[i], new ActionTime
            {
                Value = action,
            });

            EntityManager.SetComponentData(Entities[i], new Translation
            {
                Value = Position,
            });

            EntityManager.SetComponentData(Entities[i], new Hybrid
            {
                Index = i,
            });

            if (EntityManager.HasComponent<AttackTag>(Entities[i]) == false)
                EntityManager.AddComponent<AttackTag>(Entities[i]);

            spawnIndexList[spawnCnt++] = i;
        }

        //Change Whenever Spawned (Not Needed?)
        //TransformAccessArray = new TransformAccessArray(transforms);
        return spawnIndexList;
    }
}