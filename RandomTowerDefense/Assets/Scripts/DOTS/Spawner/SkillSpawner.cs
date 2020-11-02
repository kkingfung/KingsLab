using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public class SkillSpawner : MonoBehaviour
{
    private readonly int count = 30;
    public static SkillSpawner Instance { get; private set; }
    public List<GameObject> PrefabObject;

    EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

    //Array
    [HideInInspector]
    public TransformAccessArray TransformAccessArray;
    public NativeArray<float> damageArray;
    public NativeArray<float> radiusArray;
    public NativeArray<float> waitArray;
    public NativeArray<float> lifetimeArray;
    public NativeArray<float> actionArray;
    public NativeArray<float> slowArray;
    public NativeArray<float> buffArray;

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
        damageArray.Dispose();
        radiusArray.Dispose();
        waitArray.Dispose();
        lifetimeArray.Dispose();
        actionArray.Dispose();
        slowArray.Dispose();
        buffArray.Dispose();
    }
    void Start()
    {
        //Prepare input
        GameObjects = new GameObject[count];
        transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
            typeof(SkillTag), typeof(Damage), typeof(Radius),
            typeof(WaitingTime), typeof(ActionTime),
            typeof(Lifetime), typeof(SlowRate), typeof(BuffTime),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Hybrid>());
        EntityManager.CreateEntity(archetype, Entities);

        TransformAccessArray = new TransformAccessArray(transforms);
        damageArray = new NativeArray<float>(count, Allocator.Persistent);
        radiusArray = new NativeArray<float>(count, Allocator.Persistent);
        waitArray = new NativeArray<float>(count, Allocator.Persistent);
        lifetimeArray = new NativeArray<float>(count, Allocator.Persistent);
        actionArray = new NativeArray<float>(count, Allocator.Persistent);
        slowArray = new NativeArray<float>(count, Allocator.Persistent);
        buffArray = new NativeArray<float>(count, Allocator.Persistent);
    }

    public int[] Spawn(int prefabID, float3 Position, float3 Rotation, float damage, float radius,
        float wait, float lifetime,
        float action, float slow=0,float buff=0,
        int num = 1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];
        for (int i = 0; i < count && spawnCnt < num; i++)
        {
            if (GameObjects[i] != null) continue;
            GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
            GameObjects[i].transform.position = Position;
            GameObjects[i].transform.localRotation = Quaternion.Euler(Rotation);
            transforms[i] = GameObjects[i].transform;
            damageArray[i] = damage;
            radiusArray[i] = radius;
            waitArray[i] = wait;
            lifetimeArray[i] = lifetime;
            actionArray[i] = action;
            slowArray[i] = slow;
            buffArray[i] = buff;

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
            EntityManager.SetComponentData(Entities[i], new SlowRate
            {
                Value = slow,
            });
            EntityManager.SetComponentData(Entities[i], new BuffTime
            {
                Value = buff,
            });

            switch (prefabID) {
                case 0:
                    EntityManager.AddComponent(Entities[i],typeof(MeteorTag));
                    break;
                case 1:
                    EntityManager.AddComponent(Entities[i], typeof(BlizzardTag));
                    break;
                case 2:
                    EntityManager.AddComponent(Entities[i], typeof(PetrificationTag));
                    break;
                case 3:
                    EntityManager.AddComponent(Entities[i], typeof(MinionsTag));
                    break;
            }

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