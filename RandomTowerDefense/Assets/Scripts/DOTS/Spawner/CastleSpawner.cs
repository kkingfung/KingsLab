using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public class CastleSpawner : MonoBehaviour
{
    private readonly int count=1;
    public static CastleSpawner Instance { get; private set; }
    public List<GameObject> PrefabObject;

    private EntityManager EntityManager;

    //Array
    [HideInInspector]
    public TransformAccessArray TransformAccessArray;
    public NativeArray<int> castleHPArray;

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
        if (castleHPArray.IsCreated)
            castleHPArray.Dispose();
    }
    void Start()
    {

        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //Prepare input
        GameObjects = new GameObject[count];
        transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
             typeof(Health), typeof(Radius), 
            ComponentType.ReadOnly<Translation>()
            //ComponentType.ReadOnly<Hybrid>()
            );
        EntityManager.CreateEntity(archetype, Entities);
        TransformAccessArray = new TransformAccessArray(transforms);
        castleHPArray = new NativeArray<int>(count, Allocator.Persistent);
    }

    private void Update()
    {
        UpdateArrays();
    }

    public void UpdateArrays()
    {
        for (int i = 0; i < GameObjects.Length; ++i)
        {
            if (GameObjects[i] == null) continue;
            castleHPArray[i] = (int)EntityManager.GetComponentData<Health>(Entities[i]).Value;
        }
    }

    public int[] Spawn(float3 Position, Quaternion Rotation, int castleHP, float radius,
        int prefabID = 0, int num=1)
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
            castleHPArray[i] = castleHP;

            //AddtoEntities
            EntityManager.SetComponentData(Entities[i], new Health
            {
                Value = castleHP,
            });
            EntityManager.SetComponentData(Entities[i], new Radius
            {
                Value = radius,
            });
            EntityManager.SetComponentData(Entities[i], new Translation
            {
                Value = Position,
            });
            //EntityManager.SetComponentData(Entities[i], new Hybrid
            //{
            //    Index = i,
            //});

            if (EntityManager.HasComponent<CastleTag>(Entities[i]) == false)
                EntityManager.AddComponent<CastleTag>(Entities[i]);

            spawnIndexList[spawnCnt++] = i;

        }

        //Change Whenever Spawned (Not Needed?)
        //TransformAccessArray = new TransformAccessArray(transforms);
        return spawnIndexList;
    }

    public GameObject GetObjectByID(int entityID) {
        return GameObjects[entityID];
    }
}
