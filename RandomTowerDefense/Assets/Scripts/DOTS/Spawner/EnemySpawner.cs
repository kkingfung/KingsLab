using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

using Unity.MLAgents;

public class EnemySpawner : MonoBehaviour
{
    private readonly int count = 100;
    public static EnemySpawner Instance { get; private set; }
    //public List<GameObject> PrefabObject;
    //[Header("MonsterVFX")]
    //public GameObject DieEffect;
    //public GameObject DropEffect;

    [Header("MonsterAsset")]
    public GameObject MetalonGreen;
    public GameObject MetalonPurple;
    public GameObject MetalonRed;
    
    public GameObject AttackBot;
    public GameObject RobotSphere;
    
    public GameObject Dragon;
    public GameObject Bull;
    public GameObject StoneMonster;
    
    public GameObject FreeLichS;
    public GameObject FreeLich;
    public GameObject GolemS;
    public GameObject Golem;
    public GameObject SkeletonArmed;
    public GameObject SpiderGhost;
    
    public GameObject Skeleton;
    public GameObject GruntS;
    public GameObject FootmanS;
    public GameObject Grunt;
    public GameObject Footman;
    
    public GameObject TurtleShell;
    public GameObject Mushroom;
    public GameObject Slime;

    public GameObject PigChef;
    public GameObject PhoenixChick;
    public GameObject RockCritter;

    public Dictionary<string, GameObject> allMonsterList;
    private EntityManager EntityManager;

    //Array
    [HideInInspector]
    //public TransformAccessArray TransformAccessArray;
    public NativeArray<float> healthArray;
    public NativeArray<float> petrifyArray;
    public NativeArray<float> slowArray;

    //Bridge
    [HideInInspector]
    public GameObject[] GameObjects;
    public NativeArray<Entity> Entities;

    //For input
    //private Transform[] transforms;

    public FilledMapGenerator mapGenerator;

    public EffectSpawner effectManager;
    // private CastleSpawner castleSpawner;
    public AgentScript agent;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        allMonsterList = new Dictionary<string, GameObject>();

        //Bonus
        allMonsterList.Add("MetalonGreen", MetalonGreen);
        allMonsterList.Add("MetalonPurple", MetalonPurple);
        allMonsterList.Add("MetalonRed", MetalonRed);

        //Stage 4
        allMonsterList.Add("AttackBot", AttackBot);
        allMonsterList.Add("RobotSphere", RobotSphere);

        //Bosses
        allMonsterList.Add("Dragon", Dragon);
        allMonsterList.Add("Bull", Bull);
        allMonsterList.Add("StoneMonster", StoneMonster);

        //Stage 3
        allMonsterList.Add("FreeLichS", FreeLichS);
        allMonsterList.Add("FreeLich", FreeLich);
        allMonsterList.Add("GolemS", GolemS);
        allMonsterList.Add("Golem", Golem);
        allMonsterList.Add("SkeletonArmed", SkeletonArmed);
        allMonsterList.Add("SpiderGhost", SpiderGhost);

        //Stage 2
        allMonsterList.Add("Skeleton", Skeleton);
        allMonsterList.Add("GruntS", GruntS);
        allMonsterList.Add("FootmanS", FootmanS);
        allMonsterList.Add("Grunt", Grunt);
        allMonsterList.Add("Footman", Footman);

        //Stage 1
        allMonsterList.Add("TurtleShell", TurtleShell);
        allMonsterList.Add("Mushroom", Mushroom);
        allMonsterList.Add("Slime", Slime);

        //Addition
        allMonsterList.Add("PigChef", PigChef);
        allMonsterList.Add("PhoenixChick", PhoenixChick);
        allMonsterList.Add("RockCritter", RockCritter);
    }
    private void OnDisable()
    {
        if (Entities.IsCreated)
            Entities.Dispose();

        //if (TransformAccessArray.isCreated)
        //    TransformAccessArray.Dispose();

        //Disposing Array
        if (healthArray.IsCreated)
            healthArray.Dispose();
        if (slowArray.IsCreated)
            slowArray.Dispose();
        if (petrifyArray.IsCreated)
            petrifyArray.Dispose();
    }

    private void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //castleSpawner = FindObjectOfType<CastleSpawner>();
       // mapGenerator = FindObjectOfType<FilledMapGenerator>();

        //Prepare input
        GameObjects = new GameObject[count];
        //transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
             typeof(Health), typeof(Damage), typeof(Speed),
           typeof(Radius), typeof(PetrifyAmt),  typeof(Lifetime), typeof(SlowRate), 
            typeof(BuffTime),typeof(PathFollow), typeof(LocalToWorld),
            ComponentType.ReadOnly<Translation>()
            //ComponentType.ReadOnly<Translation>(),
            //    ComponentType.ReadOnly<RotationEulerXYZ>(),
            //ComponentType.ReadOnly<Hybrid>()
            );
        EntityManager.CreateEntity(archetype, Entities);

        for (int i = 0; i< count;++i) {
            EntityManager.AddBuffer<PathPosition>(Entities[i]);
        }

       // TransformAccessArray = new TransformAccessArray(transforms);
        healthArray = new NativeArray<float>(count, Allocator.Persistent);
        slowArray = new NativeArray<float>(count, Allocator.Persistent);
        petrifyArray = new NativeArray<float>(count, Allocator.Persistent);
    }

    private void Update() {
        for (int i = 0; i < GameObjects.Length; ++i) {
            if (GameObjects[i] == null) continue;
                GameObjects[i].transform.position = EntityManager.GetComponentData<Translation>(Entities[i]).Value;
        }
        UpdateArrays();
    }

    public void UpdateArrays()
    {
        for (int i = 0; i < GameObjects.Length; ++i)
        {
            if (GameObjects[i] == null) continue;
            healthArray[i] = EntityManager.GetComponentData<Health>(Entities[i]).Value;
            slowArray[i] = EntityManager.GetComponentData<SlowRate>(Entities[i]).Value;
            petrifyArray[i] = EntityManager.GetComponentData<PetrifyAmt>(Entities[i]).Value;
        }
    }

    public int[] Spawn(string Name, float3 Position, float3 Rotation, float health, int money,
        float damage, float radius,float speed, float lifetime, int num = 1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];
        for (int i = 0; i < count && spawnCnt < num; ++i)
        {
            if (GameObjects[i] != null) continue;
            GameObjects[i] = Instantiate(allMonsterList[Name], transform);
            GameObjects[i].transform.position = Position;
            GameObjects[i].transform.localRotation = Quaternion.identity;
            GameObjects[i].GetComponent<Enemy>().Init(this, effectManager, i, money,agent);
            //transforms[i] = GameObjects[i].transform;
            healthArray[i] = health;
            slowArray[i] = 0;
            petrifyArray[i] = 0;

            //AddtoEntities
            EntityManager.SetComponentData(Entities[i], new Health
            {
                Value = health,
            });
            EntityManager.SetComponentData(Entities[i], new Damage
            {
                Value = damage,
            });
            EntityManager.SetComponentData(Entities[i], new Speed
            {
                Value = speed,
            });
            EntityManager.SetComponentData(Entities[i], new Radius
            {
                Value = radius,
            });
            EntityManager.SetComponentData(Entities[i], new Lifetime
            {
                Value = lifetime,
            });
            EntityManager.SetComponentData(Entities[i], new PetrifyAmt
            {
                Value = 0,
            });
            EntityManager.SetComponentData(Entities[i], new SlowRate
            {
                Value = 0,
            });
            EntityManager.SetComponentData(Entities[i], new BuffTime
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
                typeEnum = QuadrantEntity.TypeEnum.EnemyTag
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

            if (EntityManager.HasComponent<EnemyTag>(Entities[i]) == false)
                EntityManager.AddComponent<EnemyTag>(Entities[i]);

            if (EntityManager.HasComponent<PathfindingParams>(Entities[i]) == false)
            {
                EntityManager.AddComponentData(Entities[i], new PathfindingParams
                {
                    startPosition = mapGenerator.GetTileIDFromPosition(Position),
                    endPosition = new int2(0,0),
                });
            }
            

            spawnIndexList[spawnCnt++] = i;
        }

        //Change Whenever Spawned (Not Needed?)
        //TransformAccessArray = new TransformAccessArray(transforms);
        return spawnIndexList;
    }

    public List<GameObject> AllAliveMonstersList() {
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject i in GameObjects)
        {
            if (i != null)
                result.Add(i);
        }
        return result;
    }
}
