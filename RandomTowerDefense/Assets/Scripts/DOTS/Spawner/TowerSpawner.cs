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

    private EntityManager EntityManager;

    //Array
    [HideInInspector]
    public TransformAccessArray TransformAccessArray;
    public NativeArray<float3> targetArray;
    public NativeArray<bool> hastargetArray;

    //Bridge
    [HideInInspector]
    public GameObject[] GameObjects;
    public NativeArray<Entity> Entities;

    //For input
    private Transform[] transforms;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void OnDisable()
    {
        if (Entities.IsCreated)
            Entities.Dispose();

        if (TransformAccessArray.isCreated)
            TransformAccessArray.Dispose();

        //Disposing Array
        if (targetArray.IsCreated)
            targetArray.Dispose();
        if (hastargetArray.IsCreated)
            hastargetArray.Dispose();
    }
    private void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //Prepare input
        GameObjects = new GameObject[count];
        transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
             typeof(WaitingTime), typeof(Lifetime), typeof(Radius), typeof(TargetPos),
               typeof(TargetFound), typeof(Damage), typeof(LocalToWorld), 
               typeof(QuadrantEntity),
            ComponentType.ReadOnly<CustomTransform>()
            //ComponentType.ReadOnly<Translation>(),
            //    ComponentType.ReadOnly<RotationEulerXYZ>(),
            //ComponentType.ReadOnly<Hybrid>()
            );
        EntityManager.CreateEntity(archetype, Entities);


        TransformAccessArray = new TransformAccessArray(transforms);
        targetArray = new NativeArray<float3>(count, Allocator.Persistent);
        hastargetArray = new NativeArray<bool>(count, Allocator.Persistent);
    }

    private void Update()
    {
        for (int i = 0; i < GameObjects.Length; ++i)
        {
            if (GameObjects[i] == null) continue;
            float angle = EntityManager.GetComponentData<CustomTransform>(Entities[i]).angle;
            GameObjects[i].transform.localEulerAngles = new Vector3(0, angle, 0);
        }

        UpdateArrays();
    }

    public void UpdateArrays() {
        for (int i = 0; i < GameObjects.Length; ++i)
        {
            if (GameObjects[i] == null) continue;
            targetArray[i] = EntityManager.GetComponentData<TargetPos>(Entities[i]).Value;
            hastargetArray[i] = EntityManager.GetComponentData<TargetFound>(Entities[i]).Value;
        }
    }

    public int[] Spawn(int prefabID, float3 Position, float3 Rotation, float radius,
        float wait, float lifetime,  int num = 1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];
        for (int i = 0; i < count && spawnCnt < num; i++)
        {
            if (GameObjects[i] != null) continue;

            GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
            GameObjects[i].transform.position = Position;
            GameObjects[i].transform.localRotation = Quaternion.identity;
            transforms[i] = GameObjects[i].transform;
            hastargetArray[i] = false;
            targetArray[i] = Position;

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
                Value = Position,
            });
            EntityManager.SetComponentData(Entities[i], new TargetFound
            {
                Value = false,
            });

            EntityManager.SetComponentData(Entities[i], new CustomTransform
            {
                translation = Position,
                angle = Rotation.y
            });

            EntityManager.SetComponentData(Entities[i], new QuadrantEntity
            {
                typeEnum = QuadrantEntity.TypeEnum.PlayerTag
            });

            //EntityManager.SetComponentData(Entities[i], new RotationEulerXYZ
            //{
            //    Value = Rotation,
            //});

            //EntityManager.SetComponentData(Entities[i], new Translation
            //{
            //    Value = Position,
            //});

            //EntityManager.SetComponentData(Entities[i], new Hybrid
            //{
            //    Index = i,
            //});

            if (EntityManager.HasComponent<PlayerTag>(Entities[i]) == false)
                EntityManager.AddComponent<PlayerTag>(Entities[i]);

            spawnIndexList[spawnCnt++] = i;
        }

        //Change Whenever Spawned (Not Needed?)
        //TransformAccessArray = new TransformAccessArray(transforms);
        return spawnIndexList;
    }
}
