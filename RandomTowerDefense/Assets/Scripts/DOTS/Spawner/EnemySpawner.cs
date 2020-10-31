using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public class EnemySpawner : MonoBehaviour
{
    private readonly int count = 100;
    public static EnemySpawner Instance { get; private set; }
    //public List<GameObject> PrefabObject;

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

    public Dictionary<string, GameObject> allMonsterList;
    private EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

    //Array
    [HideInInspector]
    public TransformAccessArray TransformAccessArray;
    public NativeArray<float> healthArray;
    public NativeArray<int>   moneyArray;
    public NativeArray<float> damageArray;
    public NativeArray<float> radiusArray;
    public NativeArray<float> speedArray;
    public NativeArray<float> slowArray;
    public NativeArray<float> petrifyArray;
    public NativeArray<float> buffArray;
    public NativeArray<float> lifetimeArray;

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
    }
    void OnDisable()
    {
        if (Entities.IsCreated)
            Entities.Dispose();

        if (TransformAccessArray.isCreated)
            TransformAccessArray.Dispose();

        //Disposing Array
        healthArray.Dispose();
        moneyArray.Dispose();
        damageArray.Dispose();
        radiusArray.Dispose();
        speedArray.Dispose();
        slowArray.Dispose();
        petrifyArray.Dispose();
        buffArray.Dispose();
        lifetimeArray.Dispose();
    }
    void Start()
    {
        //Prepare input
        GameObjects = new GameObject[count];
        transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
            typeof(EnemyTag), typeof(Health), typeof(Damage),
            typeof(Speed), typeof(Radius), typeof(PetrifyAmt),
            typeof(Lifetime), typeof(SlowRate), typeof(BuffTime),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Hybrid>());
        EntityManager.CreateEntity(archetype, Entities);

        TransformAccessArray = new TransformAccessArray(transforms);
        healthArray = new NativeArray<float>(count, Allocator.Persistent);
        moneyArray = new NativeArray<int>(count, Allocator.Persistent);
        damageArray = new NativeArray<float>(count, Allocator.Persistent);
        radiusArray = new NativeArray<float>(count, Allocator.Persistent);
        speedArray = new NativeArray<float>(count, Allocator.Persistent);
        slowArray = new NativeArray<float>(count, Allocator.Persistent);
        petrifyArray = new NativeArray<float>(count, Allocator.Persistent);
        buffArray = new NativeArray<float>(count, Allocator.Persistent);
        lifetimeArray = new NativeArray<float>(count, Allocator.Persistent);
    }

    public int[] Spawn(string Name, float3 Position, float3 Rotation, float health, int money,
        float damage, float radius,float speed, float lifetime, int num = 1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];
        for (int i = 0; i < count && spawnCnt < num; i++)
        {
            if (GameObjects[i] != null) continue;
            float slow = new float();
            slow = 0;
            float petrifyAmt = new float();
            petrifyAmt = 0;
            float buff = new float();
            buff = 0;

            GameObjects[i] = Instantiate(allMonsterList[Name], transform);
            GameObjects[i].transform.position = Position;
            GameObjects[i].transform.rotation = Quaternion.Euler(Rotation);
            transforms[i] = GameObjects[i].transform;
            healthArray[i] = health;
            moneyArray[i] = money;
            damageArray[i] = damage;
            radiusArray[i] = radius;
            speedArray[i] = speed;
            slowArray[i] = slow;
            petrifyArray[i] = petrifyAmt;
            buffArray[i] = buff;
            lifetimeArray[i] = lifetime;

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
                Value = petrifyAmt,
            });
            EntityManager.SetComponentData(Entities[i], new SlowRate
            {
                Value = slow,
            });
            EntityManager.SetComponentData(Entities[i], new BuffTime
            {
                Value = buff,
            });
            EntityManager.SetComponentData(Entities[i], new Money
            {
                Value = money,
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
