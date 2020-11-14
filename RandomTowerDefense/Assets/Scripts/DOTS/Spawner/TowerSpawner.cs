using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public class TowerSpawner : MonoBehaviour
{
    private readonly int count = 100;
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
             typeof(WaitingTime), typeof(Radius), 
             typeof(Damage), typeof(LocalToWorld),  typeof(QuadrantEntity),
            ComponentType.ReadOnly<Translation>()
            );
        EntityManager.CreateEntity(archetype, Entities);


        TransformAccessArray = new TransformAccessArray(transforms);
        targetArray = new NativeArray<float3>(count, Allocator.Persistent);
        hastargetArray = new NativeArray<bool>(count, Allocator.Persistent);
    }

    private void Update()
    {
        UpdateArrays();
    }

    public void UpdateArrays() {
        for (int i = 0; i < GameObjects.Length; ++i)
        {
            if (GameObjects[i] == null) continue;
            if (EntityManager.HasComponent<Target>(Entities[i]))
            {
                Target target = EntityManager.GetComponentData<Target>(Entities[i]);
                targetArray[i] = target.targetPos;
                hastargetArray[i] = EntityManager.HasComponent<EnemyTag>(target.targetEntity);
                Debug.DrawLine(target.targetPos, GameObjects[i].transform.position,Color.cyan);
            }
            else
            {
                hastargetArray[i] = false;
            }
        }
    }

    public int[] Spawn(int prefabID, float3 Position, int num = 1)
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
                Value =float.MaxValue,
            });

            EntityManager.SetComponentData(Entities[i], new Radius
            {
                Value = 0,
            });

            EntityManager.SetComponentData(Entities[i], new Translation
            {
                Value = Position
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

    public List<GameObject> AllAliveObjList()
    {
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject i in GameObjects)
        {
            if (i != null)
                result.Add(i);
        }
        return result;
    }
}
