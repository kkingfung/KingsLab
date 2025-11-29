using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.MLAgents;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.MapGenerator;
using RandomTowerDefense.AI;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;
using RandomTowerDefense.Units;

namespace RandomTowerDefense.DOTS.Spawner
{
    /// <summary>
    /// 敵エンティティスポーナーシステム - 多種類敵キャラクターの動的生成と管理
    ///
    /// 主な機能:
    /// - 20種類以上の敵タイプ対応プールシステム
    /// - ステージ別敵配置システムとML-Agents統合
    /// - リアルタイム敵ステータス配列管理（HP、スロー、石化効果）
    /// - ハイブリッドMonoBehaviour-ECS敵エンティティ生成
    /// - 敵種別グループ管理とボーナス敵特別処理
    /// - ゲームオブジェクトプール最適化とメモリ効率化
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        private readonly int count = 50;
        public static EnemySpawner Instance { get; private set; }
        public List<GameObject> PrefabObject;
        [Header("MonsterVFX")]
        public GameObject DieEffect;
        public GameObject DropEffect;

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

        public List<GameObject> ListMetalonGreen;
        public List<GameObject> ListMetalonPurple;
        public List<GameObject> ListMetalonRed;

        public List<GameObject> ListAttackBot;
        public List<GameObject> ListRobotSphere;

        public List<GameObject> ListDragon;
        public List<GameObject> ListBull;
        public List<GameObject> ListStoneMonster;

        public List<GameObject> ListFreeLichS;
        public List<GameObject> ListFreeLich;
        public List<GameObject> ListGolemS;
        public List<GameObject> ListGolem;
        public List<GameObject> ListSkeletonArmed;
        public List<GameObject> ListSpiderGhost;

        public List<GameObject> ListSkeleton;
        public List<GameObject> ListGruntS;
        public List<GameObject> ListFootmanS;
        public List<GameObject> ListGrunt;
        public List<GameObject> ListFootman;

        public List<GameObject> ListTurtleShell;
        public List<GameObject> ListMushroom;
        public List<GameObject> ListSlime;

        public List<GameObject> ListPigChef;
        public List<GameObject> ListPhoenixChick;
        public List<GameObject> ListRockCritter;

        public Dictionary<string, GameObject> allMonsterPrefabList;
        public Dictionary<string, List<GameObject>> allMonsterList;
        private EntityManager EntityManager;

        //Array
        [HideInInspector]
        public GameObject[] GameObjects;
        public TransformAccessArray TransformAccessArray;
        public NativeArray<float> healthArray;
        public NativeArray<float> petrifyArray;
        public NativeArray<float> slowArray;

        //Bridge
        public NativeArray<Entity> Entities;

        //For input
        private Transform[] transforms;

        public FilledMapGenerator mapGenerator;

        public EffectSpawner effectManager;
        private CastleSpawner castleSpawner;
        public AgentScript agent;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            allMonsterPrefabList = new Dictionary<string, GameObject>();

            //Bonus
            allMonsterPrefabList.Add("MetalonGreen", MetalonGreen);
            allMonsterPrefabList.Add("MetalonPurple", MetalonPurple);
            allMonsterPrefabList.Add("MetalonRed", MetalonRed);

            //Stage 4
            allMonsterPrefabList.Add("AttackBot", AttackBot);
            allMonsterPrefabList.Add("RobotSphere", RobotSphere);

            //Bosses
            allMonsterPrefabList.Add("Dragon", Dragon);
            allMonsterPrefabList.Add("Bull", Bull);
            allMonsterPrefabList.Add("StoneMonster", StoneMonster);

            //Stage 3
            allMonsterPrefabList.Add("FreeLichS", FreeLichS);
            allMonsterPrefabList.Add("FreeLich", FreeLich);
            allMonsterPrefabList.Add("GolemS", GolemS);
            allMonsterPrefabList.Add("Golem", Golem);
            allMonsterPrefabList.Add("SkeletonArmed", SkeletonArmed);
            allMonsterPrefabList.Add("SpiderGhost", SpiderGhost);

            //Stage 2
            allMonsterPrefabList.Add("Skeleton", Skeleton);
            allMonsterPrefabList.Add("GruntS", GruntS);
            allMonsterPrefabList.Add("FootmanS", FootmanS);
            allMonsterPrefabList.Add("Grunt", Grunt);
            allMonsterPrefabList.Add("Footman", Footman);

            //Stage 1
            allMonsterPrefabList.Add("TurtleShell", TurtleShell);
            allMonsterPrefabList.Add("Mushroom", Mushroom);
            allMonsterPrefabList.Add("Slime", Slime);

            //Addition
            allMonsterPrefabList.Add("PigChef", PigChef);
            allMonsterPrefabList.Add("PhoenixChick", PhoenixChick);
            allMonsterPrefabList.Add("RockCritter", RockCritter);
        }
        private void OnDisable()
        {
            if (Entities.IsCreated)
                Entities.Dispose();

            if (TransformAccessArray.isCreated)
                TransformAccessArray.Dispose();

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
            ListMetalonGreen = new List<GameObject>();
            ListMetalonPurple = new List<GameObject>();
            ListMetalonRed = new List<GameObject>();

            ListAttackBot = new List<GameObject>();
            ListRobotSphere = new List<GameObject>();

            ListDragon = new List<GameObject>();
            ListBull = new List<GameObject>();
            ListStoneMonster = new List<GameObject>();

            ListFreeLichS = new List<GameObject>();
            ListFreeLich = new List<GameObject>();
            ListGolemS = new List<GameObject>();
            ListGolem = new List<GameObject>();
            ListSkeletonArmed = new List<GameObject>();
            ListSpiderGhost = new List<GameObject>();

            ListSkeleton = new List<GameObject>();
            ListGruntS = new List<GameObject>();
            ListFootmanS = new List<GameObject>();
            ListGrunt = new List<GameObject>();
            ListFootman = new List<GameObject>();

            ListTurtleShell = new List<GameObject>();
            ListMushroom = new List<GameObject>();
            ListSlime = new List<GameObject>();

            ListPigChef = new List<GameObject>();
            ListPhoenixChick = new List<GameObject>();
            ListRockCritter = new List<GameObject>();

            allMonsterList = new Dictionary<string, List<GameObject>>();

            //Bonus
            allMonsterList.Add("MetalonGreen", ListMetalonGreen);
            allMonsterList.Add("MetalonPurple", ListMetalonPurple);
            allMonsterList.Add("MetalonRed", ListMetalonRed);

            //Stage 4
            allMonsterList.Add("AttackBot", ListAttackBot);
            allMonsterList.Add("RobotSphere", ListRobotSphere);

            //Bosses
            allMonsterList.Add("Dragon", ListDragon);
            allMonsterList.Add("Bull", ListBull);
            allMonsterList.Add("StoneMonster", ListStoneMonster);

            //Stage 3
            allMonsterList.Add("FreeLichS", ListFreeLichS);
            allMonsterList.Add("FreeLich", ListFreeLich);
            allMonsterList.Add("GolemS", ListGolemS);
            allMonsterList.Add("Golem", ListGolem);
            allMonsterList.Add("SkeletonArmed", ListSkeletonArmed);
            allMonsterList.Add("SpiderGhost", ListSpiderGhost);

            //Stage 2
            allMonsterList.Add("Skeleton", ListSkeleton);
            allMonsterList.Add("GruntS", ListGruntS);
            allMonsterList.Add("FootmanS", ListFootmanS);
            allMonsterList.Add("Grunt", ListGrunt);
            allMonsterList.Add("Footman", ListFootman);

            //Stage 1
            allMonsterList.Add("TurtleShell", ListTurtleShell);
            allMonsterList.Add("Mushroom", ListMushroom);
            allMonsterList.Add("Slime", ListSlime);

            //Addition
            allMonsterList.Add("PigChef", ListPigChef);
            allMonsterList.Add("PhoenixChick", ListPhoenixChick);
            allMonsterList.Add("RockCritter", ListRockCritter);

            EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            castleSpawner = FindObjectOfType<CastleSpawner>();
            mapGenerator = FindObjectOfType<FilledMapGenerator>();

            //Prepare input
            GameObjects = new GameObject[count];
            transforms = new Transform[count];

            Entities = new NativeArray<Entity>(count, Allocator.Persistent);
            var archetype = EntityManager.CreateArchetype(
                 typeof(Health), typeof(Damage), typeof(Speed),
               typeof(Radius), typeof(PetrifyAmt), typeof(Lifetime), typeof(SlowRate),
                typeof(BuffTime), typeof(PathFollow), typeof(LocalToWorld)
                );
            EntityManager.CreateEntity(archetype, Entities);

            for (int i = 0; i < count; ++i)
            {
                EntityManager.AddBuffer<PathPosition>(Entities[i]);
            }

            TransformAccessArray = new TransformAccessArray(transforms);
            healthArray = new NativeArray<float>(count, Allocator.Persistent);
            slowArray = new NativeArray<float>(count, Allocator.Persistent);
            petrifyArray = new NativeArray<float>(count, Allocator.Persistent);
        }

        private void Update()
        {
            for (int i = 0; i < GameObjects.Length; ++i)
            {
                if (GameObjects[i] == null) continue;
                if (GameObjects[i].activeSelf == false) continue;
                GameObjects[i].transform.position = EntityManager.GetComponentData<Translation>(Entities[i]).Value;
            }
            UpdateArrays();
        }

        public void UpdateArrays()
        {
            for (int i = 0; i < GameObjects.Length; ++i)
            {
                if (GameObjects[i] == null) continue;
                if (GameObjects[i].activeSelf == false) continue;
                healthArray[i] = EntityManager.GetComponentData<Health>(Entities[i]).Value;
                slowArray[i] = EntityManager.GetComponentData<SlowRate>(Entities[i]).Value;
                petrifyArray[i] = EntityManager.GetComponentData<PetrifyAmt>(Entities[i]).Value;
            }
        }

        public int[] Spawn(string Name, float3 Position, float3 Rotation, float health, int money,
            float damage, float radius, float speed, float lifetime, int num = 1)
        {
            int spawnCnt = 0;
            int[] spawnIndexList = new int[num];
            for (int i = 0; i < count && spawnCnt < num; ++i)
            {
                if (GameObjects[i] != null && GameObjects[i].activeSelf) continue;
                GameObjects[i] = SpawnFromList(Name);
                if (GameObjects[i] == null)
                {
                    GameObjects[i] = Instantiate(allMonsterPrefabList[Name], transform);
                    AddToList(GameObjects[i], Name);
                }
                else
                {
                    GameObjects[i].SetActive(true);
                }
                GameObjects[i].transform.position = Position;
                GameObjects[i].transform.localScale = allMonsterPrefabList[Name].transform.localScale;
                GameObjects[i].transform.localRotation = Quaternion.identity;
                GameObjects[i].GetComponent<Enemy>().Init(i, money, agent);
                transforms[i] = GameObjects[i].transform;
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

                if (EntityManager.HasComponent<EnemyTag>(Entities[i]) == false)
                    EntityManager.AddComponent<EnemyTag>(Entities[i]);

                if (EntityManager.HasComponent<PathFollow>(Entities[i]) == false)
                    EntityManager.AddComponent<PathFollow>(Entities[i]);

                if (EntityManager.HasComponent<PathfindingParams>(Entities[i]) == false)
                {
                    EntityManager.AddComponentData(Entities[i], new PathfindingParams
                    {
                        startPosition = mapGenerator.GetTileIDFromPosition(Position),
                        endPosition = new int2(0, 0),
                    });
                }


                spawnIndexList[spawnCnt++] = i;
            }

            //Change Whenever Spawned (Not Needed?)
            TransformAccessArray = new TransformAccessArray(transforms);

            return spawnIndexList;
        }

        public List<GameObject> AllAliveMonstersList()
        {
            List<GameObject> result = new List<GameObject>();
            foreach (GameObject i in GameObjects)
            {
                if (i != null && i.activeSelf)
                    result.Add(i);
            }
            return result;
        }

        private GameObject SpawnFromList(string name)
        {
            switch (name)
            {
                case "MetalonGreen":
                    foreach (GameObject j in ListMetalonGreen)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "MetalonPurple":
                    foreach (GameObject j in ListMetalonPurple)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "MetalonRed":
                    foreach (GameObject j in ListMetalonRed)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "AttackBot":
                    foreach (GameObject j in ListAttackBot)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "RobotSphere":
                    foreach (GameObject j in ListRobotSphere)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "Dragon":
                    foreach (GameObject j in ListDragon)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Bull":
                    foreach (GameObject j in ListBull)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "StoneMonster":
                    foreach (GameObject j in ListStoneMonster)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "FreeLichS":
                    foreach (GameObject j in ListFreeLichS)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "FreeLich":
                    foreach (GameObject j in ListFreeLich)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "GolemS":
                    foreach (GameObject j in ListGolemS)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Golem":
                    foreach (GameObject j in ListGolem)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "SkeletonArmed":
                    foreach (GameObject j in ListSkeletonArmed)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "SpiderGhost":
                    foreach (GameObject j in ListSpiderGhost)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "Skeleton":
                    foreach (GameObject j in ListSkeleton)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "GruntS":
                    foreach (GameObject j in ListGruntS)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "FootmanS":
                    foreach (GameObject j in ListFootmanS)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Grunt":
                    foreach (GameObject j in ListGrunt)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Footman":
                    foreach (GameObject j in ListFootman)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "TurtleShell":
                    foreach (GameObject j in ListTurtleShell)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Mushroom":
                    foreach (GameObject j in ListMushroom)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Slime":
                    foreach (GameObject j in ListSlime)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "PigChef":
                    foreach (GameObject j in ListPigChef)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "PhoenixChick":
                    foreach (GameObject j in ListPhoenixChick)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "RockCritter":
                    foreach (GameObject j in ListRockCritter)
                    {
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
            }
            return null;
        }

        private void AddToList(GameObject obj, string name)
        {
            switch (name)
            {
                case "MetalonGreen": ListMetalonGreen.Add(obj); break;
                case "MetalonPurple": ListMetalonPurple.Add(obj); break;
                case "MetalonRed": ListMetalonRed.Add(obj); break;

                case "AttackBot": ListAttackBot.Add(obj); break;
                case "RobotSphere": ListRobotSphere.Add(obj); break;

                case "Dragon": ListDragon.Add(obj); break;
                case "Bull": ListBull.Add(obj); break;
                case "StoneMonster": ListStoneMonster.Add(obj); break;

                case "FreeLichS": ListFreeLichS.Add(obj); break;
                case "FreeLich": ListFreeLich.Add(obj); break;
                case "GolemS": ListGolemS.Add(obj); break;
                case "Golem": ListGolem.Add(obj); break;
                case "SkeletonArmed": ListSkeletonArmed.Add(obj); break;
                case "SpiderGhost": ListSpiderGhost.Add(obj); break;

                case "Skeleton": ListSkeleton.Add(obj); break;
                case "GruntS": ListGruntS.Add(obj); break;
                case "FootmanS": ListFootmanS.Add(obj); break;
                case "Grunt": ListGrunt.Add(obj); break;
                case "Footman": ListFootman.Add(obj); break;

                case "TurtleShell": ListTurtleShell.Add(obj); break;
                case "Mushroom": ListMushroom.Add(obj); break;
                case "Slime": ListSlime.Add(obj); break;

                case "PigChef": ListPigChef.Add(obj); break;
                case "PhoenixChick": ListPhoenixChick.Add(obj); break;
                case "RockCritter": ListRockCritter.Add(obj); break;
            }
        }
    }
}
