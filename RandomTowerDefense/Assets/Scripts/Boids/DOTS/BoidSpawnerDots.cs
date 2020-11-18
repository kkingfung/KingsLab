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
    public float BoundingRadius = 100;

    private EntityManager EntityManager;

    //Array
    //[HideInInspector]
    //public TransformAccessArray TransformAccessArray;

    //Bridge
    [HideInInspector]
    public GameObject[] GameObjects;
    public NativeArray<Entity> Entities;

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

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
              typeof(BoidData), typeof(Velocity), typeof(OriPos), 
              typeof(BoidRotation),
              typeof(BoidSettingDots), typeof(BoidDataAvg)
            );
        EntityManager.CreateEntity(archetype, Entities);

        Spawn(spawnCount);
    }
    private void Update()
    {
        for (int i = 0; i < GameObjects.Length; ++i)
        {
            if (GameObjects[i] == null) continue;
            BoidData data = EntityManager.GetComponentData<BoidData>(Entities[i]);
            GameObjects[i].transform.position = data.position;
            GameObjects[i].transform.forward = data.direction;

            EntityManager.SetComponentData<BoidRotation>(Entities[i], new BoidRotation
            {
                rotation = GameObjects[i].transform.rotation
            });
        }

        Debug.DrawLine(this.transform.position + new Vector3(-BoundingRadius, BoundingRadius, BoundingRadius),
            this.transform.position + new Vector3(BoundingRadius, BoundingRadius, BoundingRadius));
        Debug.DrawLine(this.transform.position + new Vector3(-BoundingRadius, -BoundingRadius, BoundingRadius),
                   this.transform.position + new Vector3(BoundingRadius, -BoundingRadius, BoundingRadius));

        Debug.DrawLine(this.transform.position + new Vector3(-BoundingRadius, BoundingRadius, -BoundingRadius),
            this.transform.position + new Vector3(BoundingRadius, BoundingRadius, -BoundingRadius));
        Debug.DrawLine(this.transform.position + new Vector3(-BoundingRadius, -BoundingRadius, -BoundingRadius),
                   this.transform.position + new Vector3(BoundingRadius, -BoundingRadius, -BoundingRadius));
    }

    void OnDisable()
    {
        if (Entities.IsCreated)
            Entities.Dispose();

        //if (TransformAccessArray.isCreated)
        //    TransformAccessArray.Dispose();
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
            GameObjects[i] = Instantiate(prefab[rand]);
            GameObjects[i].transform.position = pos;
            GameObjects[i].transform.forward = UnityEngine.Random.insideUnitSphere;
            GameObjects[i].transform.parent = this.transform;

            //AddtoEntities
            EntityManager.SetComponentData(Entities[i], new BoidData
            {
                position = GameObjects[i].transform.position,
                direction = GameObjects[i].transform.forward,
                flockHeading = new float3(),
                flockCentre = new float3(),
                avoidanceHeading = new float3(),
                numFlockmates = 0
            });

            EntityManager.SetComponentData(Entities[i], new BoidRotation
            {
                rotation = GameObjects[i].transform.rotation
            });

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

            EntityManager.SetComponentData(Entities[i], new OriPos
            {
                Value = pos,
                BoundingRadius = BoundingRadius

            });

            EntityManager.SetComponentData(Entities[i], new Velocity
            {
                Value = GameObjects[i].transform.forward  * ((settings.minSpeed + settings.maxSpeed) / 2)
            });

            if (EntityManager.HasComponent<BoidTag>(Entities[i]) == false)
                EntityManager.AddComponent<BoidTag>(Entities[i]);

            spawnIndexList[spawnCnt++] = i;
        }

        return spawnIndexList;
    }

}