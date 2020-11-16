using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public class BoidSpawnerDots : MonoBehaviour
{
    private readonly int count = 100;
    public static BoidSpawnerDots Instance { get; private set; }
    public List<GameObject> prefab;
    public BoidSettings settings;

    public float spawnRadius = 10;
    public int spawnCount = 10;

    private EntityManager EntityManager;

    //Array
    [HideInInspector]
    public TransformAccessArray TransformAccessArray;

    //Bridge
    [HideInInspector]
    public GameObject[] GameObjects;
    public NativeArray<Entity> Entities;

    private Transform[] transforms;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //Prepare input
        GameObjects = new GameObject[count];
        transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
              typeof(BoidData), typeof(Velocity), typeof(OriPos), typeof(BoidDirection),
              typeof(BoidSettingDots), typeof(LocalToWorld),
            ComponentType.ReadOnly<Translation>()
            );
        EntityManager.CreateEntity(archetype, Entities);

        TransformAccessArray = new TransformAccessArray(transforms);

        Spawn(spawnCount);
    }
    private void Update()
    {
        for (int i = 0; i < GameObjects.Length; ++i)
        {
            if (GameObjects[i] == null) continue;
            GameObjects[i].transform.position = EntityManager.GetComponentData<Translation>(Entities[i]).Value;
        }
    }

    void OnDisable()
    {
        if (Entities.IsCreated)
            Entities.Dispose();

        if (TransformAccessArray.isCreated)
            TransformAccessArray.Dispose();
    }

    public int[] Spawn(int num = 1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];
        for (int i = 0; i < count && spawnCnt < num; ++i)
        {
            if (GameObjects[i] != null) continue;
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
            int rand = UnityEngine.Random.Range(0, prefab.Count);
            GameObjects[i] = Instantiate(prefab[rand], transform);
            GameObjects[i].transform.position = pos;
            GameObjects[i].transform.forward = UnityEngine.Random.insideUnitSphere;
            GameObjects[i].GetComponent<BoidDots>().Init(this, i);
            GameObjects[i].transform.parent = this.transform;

            transforms[i] = GameObjects[i].transform;

            //AddtoEntities
            //??BoidData
            EntityManager.SetComponentData(Entities[i], new BoidSettingDots
            {
                minSpeed = settings.minSpeed,
                maxSpeed = settings.maxSpeed,
                perceptionRadius = settings.perceptionRadius,
                avoidanceRadius = settings.avoidanceRadius,
                maxSteerForce = settings.maxSteerForce,

                alignWeight = settings.alignWeight,
                cohesionWeight = settings.cohesionWeight,
                seperateWeight = settings.seperateWeight,

                targetWeight = settings.targetWeight,

                boundsRadius = settings.boundsRadius,
                avoidCollisionWeight = settings.avoidCollisionWeight,
                collisionAvoidDst = settings.collisionAvoidDst,
            });

            EntityManager.SetComponentData(Entities[i], new Translation
            {
                Value = pos
            });
            EntityManager.SetComponentData(Entities[i], new OriPos
            {
                Value = pos
            });
            EntityManager.SetComponentData(Entities[i], new BoidDirection
            {
                Value = GameObjects[i].transform.forward
            });
            EntityManager.SetComponentData(Entities[i], new Velocity
            {
                Value = GameObjects[i].transform.forward  * ((settings.minSpeed + settings.maxSpeed) / 2)
            });

            if (EntityManager.HasComponent<BoidTag>(Entities[i]) == false)
                EntityManager.AddComponent<BoidTag>(Entities[i]);

            spawnIndexList[spawnCnt++] = i;
        }

        //Change Whenever Spawned (Not Needed?)
        //TransformAccessArray = new TransformAccessArray(transforms);
        return spawnIndexList;
    }

}