using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public class TowerSpawner : MonoBehaviour
{
    public const int MonsterMaxRank = 4;
    private readonly int count = 100;
    public static TowerSpawner Instance { get; private set; }
    public List<GameObject> PrefabObject;

    private EntityManager EntityManager;

    //ToTowerSpawner
    public List<GameObject> TowerNightmareRank1;
    public List<GameObject> TowerNightmareRank2;
    public List<GameObject> TowerNightmareRank3;
    public List<GameObject> TowerNightmareRank4;

    public List<GameObject> TowerSoulEaterRank1;
    public List<GameObject> TowerSoulEaterRank2;
    public List<GameObject> TowerSoulEaterRank3;
    public List<GameObject> TowerSoulEaterRank4;

    public List<GameObject> TowerTerrorBringerRank1;
    public List<GameObject> TowerTerrorBringerRank2;
    public List<GameObject> TowerTerrorBringerRank3;
    public List<GameObject> TowerTerrorBringerRank4;

    public List<GameObject> TowerUsurperRank1;
    public List<GameObject> TowerUsurperRank2;
    public List<GameObject> TowerUsurperRank3;
    public List<GameObject> TowerUsurperRank4;

    //Array
    [HideInInspector]
    //public TransformAccessArray TransformAccessArray;
    public NativeArray<float3> targetArray;
    public NativeArray<bool> hastargetArray;

    //Bridge
    [HideInInspector]
    public GameObject[] GameObjects;
    public NativeArray<Entity> Entities;

    //For input
    //private Transform[] transforms;

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

        //if (TransformAccessArray.isCreated)
        //TransformAccessArray.Dispose();

        //Disposing Array
        if (targetArray.IsCreated)
            targetArray.Dispose();
        if (hastargetArray.IsCreated)
            hastargetArray.Dispose();
    }
    private void Start()
    {
        TowerNightmareRank1 = new List<GameObject>();
        TowerNightmareRank2 = new List<GameObject>();
        TowerNightmareRank3 = new List<GameObject>();
        TowerNightmareRank4 = new List<GameObject>();

        TowerSoulEaterRank1 = new List<GameObject>();
        TowerSoulEaterRank2 = new List<GameObject>();
        TowerSoulEaterRank3 = new List<GameObject>();
        TowerSoulEaterRank4 = new List<GameObject>();

        TowerTerrorBringerRank1 = new List<GameObject>();
        TowerTerrorBringerRank2 = new List<GameObject>();
        TowerTerrorBringerRank3 = new List<GameObject>();
        TowerTerrorBringerRank4 = new List<GameObject>();

        TowerUsurperRank1 = new List<GameObject>();
        TowerUsurperRank2 = new List<GameObject>();
        TowerUsurperRank3 = new List<GameObject>();
        TowerUsurperRank4 = new List<GameObject>();

        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //Prepare input
        GameObjects = new GameObject[count];
        //transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
             typeof(WaitingTime), typeof(Radius), typeof(CastlePos),
             typeof(Damage), typeof(LocalToWorld),
            ComponentType.ReadOnly<Translation>()
            );
        EntityManager.CreateEntity(archetype, Entities);


        //TransformAccessArray = new TransformAccessArray(transforms);
        targetArray = new NativeArray<float3>(count, Allocator.Persistent);
        hastargetArray = new NativeArray<bool>(count, Allocator.Persistent);
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
            if (GameObjects[i].activeSelf == false) continue;
            if (EntityManager.HasComponent<Target>(Entities[i]))
            {
                Target target = EntityManager.GetComponentData<Target>(Entities[i]);
                targetArray[i] = target.targetPos;
                hastargetArray[i] = EntityManager.HasComponent<EnemyTag>(target.targetEntity);
                Debug.DrawLine(target.targetPos, GameObjects[i].transform.position, Color.cyan);
            }
            else
            {
                hastargetArray[i] = false;
            }
        }
    }

    public int[] Spawn(int prefabID, float3 Position, float3 CastlePosition, int num = 1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];
        for (int i = 0; i < count && spawnCnt < num; ++i)
        {
            if (GameObjects[i] != null && GameObjects[i].activeSelf) continue;

            GameObjects[i] = SpawnFromList(prefabID);

            if (GameObjects[i] == null)
            {
                GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
                AddToList(GameObjects[i], prefabID);
            }
            else {
                GameObjects[i].SetActive(true);
            }
            GameObjects[i].transform.position = Position;
            GameObjects[i].transform.localRotation = Quaternion.identity;
            //transforms[i] = GameObjects[i].transform;
            hastargetArray[i] = false;
            targetArray[i] = Position;

            //AddtoEntities
            EntityManager.SetComponentData(Entities[i], new WaitingTime
            {
                Value = float.MaxValue,
            });

            EntityManager.SetComponentData(Entities[i], new Radius
            {
                Value = 0,
            });

            EntityManager.SetComponentData(Entities[i], new Translation
            {
                Value = Position
            });

            if (EntityManager.HasComponent<QuadrantEntity>(Entities[i]) == false)
                EntityManager.AddComponent<QuadrantEntity>(Entities[i]);
            EntityManager.SetComponentData(Entities[i], new QuadrantEntity
            {
                typeEnum = QuadrantEntity.TypeEnum.PlayerTag
            });

            EntityManager.SetComponentData(Entities[i], new CastlePos
            {
                Value = CastlePosition,
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
            if (i != null && i.activeSelf)
                result.Add(i);
        }
        return result;
    }
    private GameObject SpawnFromList(int prefabID)
    {
        switch (prefabID)
        {
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerNightmare + 0:
                foreach (GameObject j in TowerNightmareRank1)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerNightmare + 1:
                foreach (GameObject j in TowerNightmareRank2)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerNightmare + 2:
                foreach (GameObject j in TowerNightmareRank3)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerNightmare + 3:
                foreach (GameObject j in TowerNightmareRank4)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater + 0:
                foreach (GameObject j in TowerSoulEaterRank1)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater + 1:
                foreach (GameObject j in TowerSoulEaterRank2)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater + 2:
                foreach (GameObject j in TowerSoulEaterRank3)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater + 3:
                foreach (GameObject j in TowerSoulEaterRank4)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer + 0:
                foreach (GameObject j in TowerTerrorBringerRank1)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer + 1:
                foreach (GameObject j in TowerTerrorBringerRank2)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer + 2:
                foreach (GameObject j in TowerTerrorBringerRank3)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer + 3:
                foreach (GameObject j in TowerTerrorBringerRank4)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerUsurper + 0:
                foreach (GameObject j in TowerUsurperRank1)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerUsurper + 1:
                foreach (GameObject j in TowerUsurperRank2)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerUsurper + 2:
                foreach (GameObject j in TowerUsurperRank3)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerUsurper + 3:
                foreach (GameObject j in TowerUsurperRank4)
                {
                    if (j.activeSelf) continue;
                    return j;
                }
                break;
        }
        return null;
    }

    private void AddToList(GameObject obj, int prefabID) {
        switch (prefabID)
        {
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerNightmare + 0:
                TowerNightmareRank1.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerNightmare + 1:
                TowerNightmareRank2.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerNightmare + 2:
                TowerNightmareRank3.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerNightmare + 3:
                TowerNightmareRank4.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater + 0:
                TowerSoulEaterRank1.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater + 1:
                TowerSoulEaterRank2.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater + 2:
                TowerSoulEaterRank3.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater + 3:
                TowerSoulEaterRank4.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer + 0:
                TowerTerrorBringerRank1.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer + 1:
                TowerTerrorBringerRank2.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer + 2:
                TowerTerrorBringerRank3.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer + 3:
                TowerTerrorBringerRank4.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerUsurper + 0:
                TowerUsurperRank1.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerUsurper + 1:
                TowerUsurperRank2.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerUsurper + 2:
                TowerUsurperRank3.Add(obj);
                break;
            case MonsterMaxRank * (int)TowerInfo.TowerInfoID.Enum_TowerUsurper + 3:
                TowerUsurperRank4.Add(obj);
                break;
        }

    }
}
