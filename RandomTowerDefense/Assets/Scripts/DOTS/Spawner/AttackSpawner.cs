using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.VFX;

public class AttackSpawner : MonoBehaviour
{
    private readonly int count=200;
    public static AttackSpawner Instance { get; private set; }
    public List<GameObject> PrefabObject;

    private EntityManager EntityManager;

    //ToAttackSpawner
    public List<GameObject> TowerNightmareAttack;
    public List<GameObject> TowerSoulEaterAttack;
    public List<GameObject> TowerTerrorBringerAttack;
    public List<GameObject> TowerUsurperAttack;

    //Bridge
    [HideInInspector]
    public GameObject[] GameObjects;
    public NativeArray<Entity> Entities;

    //For input
    //private Transform[] transforms;
    private void Update()
    {
    }

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
    }
    void Start()
    {
        //TowerNightmareAttack = new List<GameObject>();
        //TowerSoulEaterAttack = new List<GameObject>();
        //TowerTerrorBringerAttack = new List<GameObject>();
        //TowerUsurperAttack = new List<GameObject>();

        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //Prepare input
        GameObjects = new GameObject[count];
        //transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
             typeof(Radius), typeof(Damage), typeof(Velocity),
             typeof(WaitingTime), typeof(Lifetime), typeof(ActionTime),
            ComponentType.ReadOnly<Translation>()
            //ComponentType.ReadOnly<Hybrid>()
            );
        EntityManager.CreateEntity(archetype, Entities);
    }

    public int[] Spawn(int prefabID, float3 position, float3 entityposition, Quaternion rotation, float3 velocity, float damage, 
        float radius, float wait, float lifetime, float action=0.2f, int num = 1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];

        for (int i = 0; i < count && spawnCnt < num; ++i)
        {
            if (GameObjects[i] != null && GameObjects[i].activeSelf) continue;
            bool reuse = false;

            switch (prefabID)
            {
                case 0:
                    foreach (GameObject j in TowerNightmareAttack)
                    {
                        if (j.activeSelf) continue;
                        GameObjects[i] = j;
                        reuse = true;
                        break;
                    }
                    break;
                case 1:
                    foreach (GameObject j in TowerSoulEaterAttack)
                    {
                        if (j.activeSelf) continue;
                        GameObjects[i] = j;
                        reuse = true;
                        break;
                    }
                    break;
                case 2:
                    foreach (GameObject j in TowerTerrorBringerAttack)
                    {
                        if (j.activeSelf) continue;
                        GameObjects[i] = j;
                        reuse = true;
                        break;
                    }
                    break;
                case 3:
                    foreach (GameObject j in TowerUsurperAttack)
                    {
                        if (j.activeSelf) continue;
                        GameObjects[i] = j;
                        reuse = true;
                        break;
                    }
                    break;
            }
            if (reuse == false)
            {
                GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
                switch (prefabID)
                {
                    case 0:
                        TowerNightmareAttack.Add(GameObjects[i]);
                        break;
                    case 1:
                        TowerSoulEaterAttack.Add(GameObjects[i]);
                        break;
                    case 2:
                        TowerTerrorBringerAttack.Add(GameObjects[i]);
                        break;
                    case 3:
                        TowerUsurperAttack.Add(GameObjects[i]);
                        break;
                }
            }
            else
            {
                GameObjects[i].SetActive(true);
                GameObjects[i].GetComponent<VisualEffect>().Play();
            }
            //GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
            GameObjects[i].transform.position = position;
            AutoDestroyVFX autoDestroy = GameObjects[i].GetComponent<AutoDestroyVFX>();
            if (autoDestroy) autoDestroy.Timer = lifetime;
            GameObjects[i].transform.localRotation = rotation;
           // transforms[i] = GameObjects[i].transform;
           
            //AddtoEntities
            EntityManager.SetComponentData(Entities[i], new Damage
            {
                Value = damage,
            });
            EntityManager.SetComponentData(Entities[i], new Radius
            {
                Value = radius,
            });
            EntityManager.SetComponentData(Entities[i], new Velocity
            {
                Value = velocity,
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
               Value= entityposition,
            });

            //EntityManager.SetComponentData(Entities[i], new Hybrid
            //{
            //    Index = i,
            //});

            if (EntityManager.HasComponent<AttackTag>(Entities[i]) == false)
                EntityManager.AddComponent<AttackTag>(Entities[i]);

            spawnIndexList[spawnCnt++] = i;
        }

        //Change Whenever Spawned (Not Needed?)
        //TransformAccessArray = new TransformAccessArray(transforms);
        return spawnIndexList;
    }
}