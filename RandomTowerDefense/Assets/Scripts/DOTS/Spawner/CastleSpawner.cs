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

    EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

    //Array
    [HideInInspector]
    public TransformAccessArray TransformAccessArray;
    public NativeArray<int> castleHPArray;
    public NativeArray<float> radiusArray;

    //Bridge
    [HideInInspector]
    public GameObject[] GameObjects;
    public NativeArray<Entity> Entities;

    //For input
    private Transform[] transforms;
    private int[] castleHP;
    private float[] radius;

    private InGameOperation sceneManager;

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
        castleHPArray.Dispose();
        radiusArray.Dispose();
    }
    void Start()
    {
        //Prepare input
        GameObjects = new GameObject[count];
        transforms = new Transform[count];
        castleHP = new int[count];
        radius = new float[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
            typeof(CastleTag), typeof(Health), typeof(Radius),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Hybrid>());
        EntityManager.CreateEntity(archetype, Entities);

        TransformAccessArray = new TransformAccessArray(transforms);
        castleHPArray = new NativeArray<int>(count, Allocator.Persistent);
        radiusArray = new NativeArray<float>(count, Allocator.Persistent);

        sceneManager = FindObjectOfType<InGameOperation>();
    }

    public int[] Spawn(float3 Position, Quaternion Rotation, int castleHP, float radius,
        int prefabID = 0, int num=1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];
        for (int i = 0; i < count && spawnCnt < num; i++)
        {
            if (GameObjects[i] != null) continue;

            int HpCal = castleHP;
            if (sceneManager && (sceneManager.GetCurrIsland() == StageInfo.IslandNum - 1))
            {
                HpCal = StageInfo.hpMaxEx;
            }

            GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
            GameObjects[i].transform.position = Position;
            GameObjects[i].transform.rotation = Rotation;
            transforms[i] = GameObjects[i].transform;
            castleHPArray[i] = HpCal;
            radiusArray[i] = radius;

            //AddtoEntities
            EntityManager.SetComponentData(Entities[i], new Health
            {
                Value = HpCal,
            });
            EntityManager.SetComponentData(Entities[i], new Radius
            {
                Value = radius,
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

    public GameObject GetObjectByID(int entityID) {
        return GameObjects[entityID];
    }
}
