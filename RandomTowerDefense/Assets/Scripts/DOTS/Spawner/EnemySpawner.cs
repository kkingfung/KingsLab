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
using RandomTowerDefense.DOTS.Pathfinding;
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
        private readonly int _count = 50;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static EnemySpawner Instance { get; private set; }

        /// <summary>
        /// 敵プレハブオブジェクトリスト
        /// </summary>
        public List<GameObject> PrefabObject;

        // ボーナス敵プレハブ
        /// <summary>緑Metalon（ボーナス敵）プレハブ</summary>
        [Header("MonsterAsset")]
        public GameObject MetalonGreen;
        /// <summary>紫Metalon（ボーナス敵）プレハブ</summary>
        public GameObject MetalonPurple;
        /// <summary>赤Metalon（ボーナス敵）プレハブ</summary>
        public GameObject MetalonRed;

        // ステージ4敵プレハブ
        /// <summary>AttackBot（ステージ4）プレハブ</summary>
        public GameObject AttackBot;
        /// <summary>RobotSphere（ステージ4）プレハブ</summary>
        public GameObject RobotSphere;

        // ボス敵プレハブ
        /// <summary>Dragon（ボス敵）プレハブ</summary>
        public GameObject Dragon;
        /// <summary>Bull（ボス敵）プレハブ</summary>
        public GameObject Bull;
        /// <summary>StoneMonster（ボス敵）プレハブ</summary>
        public GameObject StoneMonster;

        // ステージ3敵プレハブ
        /// <summary>FreeLichS（ステージ3）プレハブ</summary>
        public GameObject FreeLichS;
        /// <summary>FreeLich（ステージ3）プレハブ</summary>
        public GameObject FreeLich;
        /// <summary>GolemS（ステージ3）プレハブ</summary>
        public GameObject GolemS;
        /// <summary>Golem（ステージ3）プレハブ</summary>
        public GameObject Golem;
        /// <summary>SkeletonArmed（ステージ3）プレハブ</summary>
        public GameObject SkeletonArmed;
        /// <summary>SpiderGhost（ステージ3）プレハブ</summary>
        public GameObject SpiderGhost;

        // ステージ2敵プレハブ
        /// <summary>Skeleton（ステージ2）プレハブ</summary>
        public GameObject Skeleton;
        /// <summary>GruntS（ステージ2）プレハブ</summary>
        public GameObject GruntS;
        /// <summary>FootmanS（ステージ2）プレハブ</summary>
        public GameObject FootmanS;
        /// <summary>Grunt（ステージ2）プレハブ</summary>
        public GameObject Grunt;
        /// <summary>Footman（ステージ2）プレハブ</summary>
        public GameObject Footman;

        // ステージ1敵プレハブ
        /// <summary>TurtleShell（ステージ1）プレハブ</summary>
        public GameObject TurtleShell;
        /// <summary>Mushroom（ステージ1）プレハブ</summary>
        public GameObject Mushroom;
        /// <summary>Slime（ステージ1）プレハブ</summary>
        public GameObject Slime;

        // 追加敵プレハブ
        /// <summary>PigChef（追加敵）プレハブ</summary>
        public GameObject PigChef;
        /// <summary>PhoenixChick（追加敵）プレハブ</summary>
        public GameObject PhoenixChick;
        /// <summary>RockCritter（追加敵）プレハブ</summary>
        public GameObject RockCritter;

        // ボーナス敵プールリスト
        /// <summary>緑Metalonプールリスト</summary>
        public List<GameObject> ListMetalonGreen;
        /// <summary>紫Metalonプールリスト</summary>
        public List<GameObject> ListMetalonPurple;
        /// <summary>赤Metalonプールリスト</summary>
        public List<GameObject> ListMetalonRed;

        // ステージ4敵プールリスト
        /// <summary>AttackBotプールリスト</summary>
        public List<GameObject> ListAttackBot;
        /// <summary>RobotSphereプールリスト</summary>
        public List<GameObject> ListRobotSphere;

        // ボス敵プールリスト
        /// <summary>Dragonプールリスト</summary>
        public List<GameObject> ListDragon;
        /// <summary>Bullプールリスト</summary>
        public List<GameObject> ListBull;
        /// <summary>StoneMonsterプールリスト</summary>
        public List<GameObject> ListStoneMonster;

        // ステージ3敵プールリスト
        /// <summary>FreeLichSプールリスト</summary>
        public List<GameObject> ListFreeLichS;
        /// <summary>FreeLichプールリスト</summary>
        public List<GameObject> ListFreeLich;
        /// <summary>GolemSプールリスト</summary>
        public List<GameObject> ListGolemS;
        /// <summary>Golemプールリスト</summary>
        public List<GameObject> ListGolem;
        /// <summary>SkeletonArmedプールリスト</summary>
        public List<GameObject> ListSkeletonArmed;
        /// <summary>SpiderGhostプールリスト</summary>
        public List<GameObject> ListSpiderGhost;

        // ステージ2敵プールリスト
        /// <summary>Skeletonプールリスト</summary>
        public List<GameObject> ListSkeleton;
        /// <summary>GruntSプールリスト</summary>
        public List<GameObject> ListGruntS;
        /// <summary>FootmanSプールリスト</summary>
        public List<GameObject> ListFootmanS;
        /// <summary>Gruntプールリスト</summary>
        public List<GameObject> ListGrunt;
        /// <summary>Footmanプールリスト</summary>
        public List<GameObject> ListFootman;

        // ステージ1敵プールリスト
        /// <summary>TurtleShellプールリスト</summary>
        public List<GameObject> ListTurtleShell;
        /// <summary>Mushroomプールリスト</summary>
        public List<GameObject> ListMushroom;
        /// <summary>Slimeプールリスト</summary>
        public List<GameObject> ListSlime;

        // 追加敵プールリスト
        /// <summary>PigChefプールリスト</summary>
        public List<GameObject> ListPigChef;
        /// <summary>PhoenixChickプールリスト</summary>
        public List<GameObject> ListPhoenixChick;
        /// <summary>RockCritterプールリスト</summary>
        public List<GameObject> ListRockCritter;

        /// <summary>
        /// 全敵種類のプレハブ辞書（名前 → プレハブ）
        /// </summary>
        public Dictionary<string, GameObject> allMonsterPrefabList;

        /// <summary>
        /// 全敵種類のプールリスト辞書（名前 → プールリスト）
        /// </summary>
        public Dictionary<string, List<GameObject>> allMonsterList;

        private EntityManager _entityManager;

        /// <summary>
        /// アクティブ敵ゲームオブジェクト配列
        /// </summary>
        [HideInInspector]
        public GameObject[] GameObjects;

        /// <summary>
        /// トランスフォームアクセス配列（Job System用）
        /// </summary>
        public TransformAccessArray TransformAccessArray;

        /// <summary>
        /// 敵HP配列
        /// </summary>
        public NativeArray<float> healthArray;

        /// <summary>
        /// 敵石化効果配列
        /// </summary>
        public NativeArray<float> petrifyArray;

        /// <summary>
        /// 敵スロー効果配列
        /// </summary>
        public NativeArray<float> slowArray;

        /// <summary>
        /// 敵エンティティ配列
        /// </summary>
        public NativeArray<Entity> Entities;

        // 入力用トランスフォーム
        private Transform[] _transforms;

        /// <summary>
        /// マップジェネレーター参照
        /// </summary>
        public FilledMapGenerator mapGenerator;

        /// <summary>
        /// エフェクトスポーナー参照
        /// </summary>
        public EffectSpawner effectManager;

        private CastleSpawner castleSpawner;

        /// <summary>
        /// ML-Agentsエージェント参照
        /// </summary>
        public AgentScript agent;

        /// <summary>
        /// 初期化処理 - シングルトンインスタンス設定と敵辞書初期化
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            allMonsterPrefabList = new Dictionary<string, GameObject>();

            // ボーナス敵
            allMonsterPrefabList.Add("MetalonGreen", MetalonGreen);
            allMonsterPrefabList.Add("MetalonPurple", MetalonPurple);
            allMonsterPrefabList.Add("MetalonRed", MetalonRed);

            // ステージ4
            allMonsterPrefabList.Add("AttackBot", AttackBot);
            allMonsterPrefabList.Add("RobotSphere", RobotSphere);

            // ボス敵
            allMonsterPrefabList.Add("Dragon", Dragon);
            allMonsterPrefabList.Add("Bull", Bull);
            allMonsterPrefabList.Add("StoneMonster", StoneMonster);

            // ステージ3
            allMonsterPrefabList.Add("FreeLichS", FreeLichS);
            allMonsterPrefabList.Add("FreeLich", FreeLich);
            allMonsterPrefabList.Add("GolemS", GolemS);
            allMonsterPrefabList.Add("Golem", Golem);
            allMonsterPrefabList.Add("SkeletonArmed", SkeletonArmed);
            allMonsterPrefabList.Add("SpiderGhost", SpiderGhost);

            // ステージ2
            allMonsterPrefabList.Add("Skeleton", Skeleton);
            allMonsterPrefabList.Add("GruntS", GruntS);
            allMonsterPrefabList.Add("FootmanS", FootmanS);
            allMonsterPrefabList.Add("Grunt", Grunt);
            allMonsterPrefabList.Add("Footman", Footman);

            // ステージ1
            allMonsterPrefabList.Add("TurtleShell", TurtleShell);
            allMonsterPrefabList.Add("Mushroom", Mushroom);
            allMonsterPrefabList.Add("Slime", Slime);

            // 追加敵
            allMonsterPrefabList.Add("PigChef", PigChef);
            allMonsterPrefabList.Add("PhoenixChick", PhoenixChick);
            allMonsterPrefabList.Add("RockCritter", RockCritter);
        }

        /// <summary>
        /// 無効化時処理 - ネイティブ配列の解放
        /// </summary>
        private void OnDisable()
        {
            if (Entities.IsCreated)
                Entities.Dispose();

            if (TransformAccessArray.isCreated)
                TransformAccessArray.Dispose();

            // 配列の破棄
            if (healthArray.IsCreated)
                healthArray.Dispose();
            if (slowArray.IsCreated)
                slowArray.Dispose();
            if (petrifyArray.IsCreated)
                petrifyArray.Dispose();
        }

        /// <summary>
        /// 開始時処理 - プール、辞書、エンティティの初期化
        /// </summary>
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

            // ボーナス敵
            allMonsterList.Add("MetalonGreen", ListMetalonGreen);
            allMonsterList.Add("MetalonPurple", ListMetalonPurple);
            allMonsterList.Add("MetalonRed", ListMetalonRed);

            // ステージ4
            allMonsterList.Add("AttackBot", ListAttackBot);
            allMonsterList.Add("RobotSphere", ListRobotSphere);

            // ボス敵
            allMonsterList.Add("Dragon", ListDragon);
            allMonsterList.Add("Bull", ListBull);
            allMonsterList.Add("StoneMonster", ListStoneMonster);

            // ステージ3
            allMonsterList.Add("FreeLichS", ListFreeLichS);
            allMonsterList.Add("FreeLich", ListFreeLich);
            allMonsterList.Add("GolemS", ListGolemS);
            allMonsterList.Add("Golem", ListGolem);
            allMonsterList.Add("SkeletonArmed", ListSkeletonArmed);
            allMonsterList.Add("SpiderGhost", ListSpiderGhost);

            // ステージ2
            allMonsterList.Add("Skeleton", ListSkeleton);
            allMonsterList.Add("GruntS", ListGruntS);
            allMonsterList.Add("FootmanS", ListFootmanS);
            allMonsterList.Add("Grunt", ListGrunt);
            allMonsterList.Add("Footman", ListFootman);

            // ステージ1
            allMonsterList.Add("TurtleShell", ListTurtleShell);
            allMonsterList.Add("Mushroom", ListMushroom);
            allMonsterList.Add("Slime", ListSlime);

            // 追加敵
            allMonsterList.Add("PigChef", ListPigChef);
            allMonsterList.Add("PhoenixChick", ListPhoenixChick);
            allMonsterList.Add("RockCritter", ListRockCritter);

            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            castleSpawner = FindObjectOfType<CastleSpawner>();
            mapGenerator = FindObjectOfType<FilledMapGenerator>();

            // 入力データの準備
            GameObjects = new GameObject[_count];
            _transforms = new Transform[_count];

            Entities = new NativeArray<Entity>(_count, Allocator.Persistent);
            var archetype = _entityManager.CreateArchetype(
                 typeof(Health), typeof(Damage), typeof(Speed),
               typeof(Radius), typeof(PetrifyAmt), typeof(Lifetime), typeof(SlowRate),
                typeof(BuffTime), typeof(PathFollow), typeof(LocalToWorld), typeof(Translation)
                );
            _entityManager.CreateEntity(archetype, Entities);

            for (int i = 0; i < _count; ++i)
            {
                _entityManager.AddBuffer<PathPosition>(Entities[i]);
            }

            TransformAccessArray = new TransformAccessArray(_transforms);
            healthArray = new NativeArray<float>(_count, Allocator.Persistent);
            slowArray = new NativeArray<float>(_count, Allocator.Persistent);
            petrifyArray = new NativeArray<float>(_count, Allocator.Persistent);
        }

        /// <summary>
        /// 毎フレーム更新 - 敵位置とステータス配列の同期
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < GameObjects.Length; ++i)
            {
                if (GameObjects[i] == null) continue;
                if (GameObjects[i].activeSelf == false) continue;
                GameObjects[i].transform.position = _entityManager.GetComponentData<Translation>(Entities[i]).Value;
            }
            UpdateArrays();
        }

        /// <summary>
        /// 敵ステータス配列をECSエンティティと同期更新
        /// </summary>
        public void UpdateArrays()
        {
            for (int i = 0; i < GameObjects.Length; ++i)
            {
                if (GameObjects[i] == null) continue;
                if (GameObjects[i].activeSelf == false) continue;
                healthArray[i] = _entityManager.GetComponentData<Health>(Entities[i]).Value;
                slowArray[i] = _entityManager.GetComponentData<SlowRate>(Entities[i]).Value;
                petrifyArray[i] = _entityManager.GetComponentData<PetrifyAmt>(Entities[i]).Value;
            }
        }

        /// <summary>
        /// 敵をスポーン
        /// </summary>
        /// <param name="Name">敵の名前（辞書キー）</param>
        /// <param name="Position">スポーン位置（ワールド座標）</param>
        /// <param name="Rotation">回転</param>
        /// <param name="health">HP</param>
        /// <param name="money">撃破時の報酬金額</param>
        /// <param name="damage">ダメージ量</param>
        /// <param name="radius">半径</param>
        /// <param name="speed">移動速度</param>
        /// <param name="lifetime">生存時間</param>
        /// <param name="num">スポーン数（デフォルト: 1）</param>
        /// <returns>スポーンされたインデックス配列</returns>
        public int[] Spawn(string Name, float3 Position, float3 Rotation, float health, int money,
            float damage, float radius, float speed, float lifetime, int num = 1)
        {
            int spawnCnt = 0;
            int[] spawnIndexList = new int[num];
            for (int i = 0; i < _count && spawnCnt < num; ++i)
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
                _transforms[i] = GameObjects[i].transform;
                healthArray[i] = health;
                slowArray[i] = 0;
                petrifyArray[i] = 0;
                // エンティティへ追加
                _entityManager.SetComponentData(Entities[i], new Health
                {
                    Value = health,
                });
                _entityManager.SetComponentData(Entities[i], new Damage
                {
                    Value = damage,
                });
                _entityManager.SetComponentData(Entities[i], new Speed
                {
                    Value = speed,
                });
                _entityManager.SetComponentData(Entities[i], new Radius
                {
                    Value = radius,
                });
                _entityManager.SetComponentData(Entities[i], new Lifetime
                {
                    Value = lifetime,
                });
                _entityManager.SetComponentData(Entities[i], new PetrifyAmt
                {
                    Value = 0,
                });
                _entityManager.SetComponentData(Entities[i], new SlowRate
                {
                    Value = 0,
                });
                _entityManager.SetComponentData(Entities[i], new BuffTime
                {
                    Value = 0,
                });

                _entityManager.SetComponentData(Entities[i], new Translation
                {
                    Value = Position
                });

                if (_entityManager.HasComponent<QuadrantEntity>(Entities[i]) == false)
                    _entityManager.AddComponent<QuadrantEntity>(Entities[i]);
                _entityManager.SetComponentData(Entities[i], new QuadrantEntity
                {
                    typeEnum = QuadrantEntity.TypeEnum.EnemyTag
                });

                if (_entityManager.HasComponent<EnemyTag>(Entities[i]) == false)
                    _entityManager.AddComponent<EnemyTag>(Entities[i]);

                if (_entityManager.HasComponent<PathFollow>(Entities[i]) == false)
                    _entityManager.AddComponent<PathFollow>(Entities[i]);

                if (_entityManager.HasComponent<PathfindingParams>(Entities[i]) == false)
                {
                    _entityManager.AddComponentData(Entities[i], new PathfindingParams
                    {
                        startPosition = mapGenerator.GetTileIDFromPosition(Position),
                        endPosition = new int2(0, 0),
                    });
                }


                spawnIndexList[spawnCnt++] = i;
            }

            // スポーン時に変更（不要な可能性あり）
            if (TransformAccessArray.isCreated)
                TransformAccessArray.Dispose();
            TransformAccessArray = new TransformAccessArray(_transforms);

            return spawnIndexList;
        }

        /// <summary>
        /// 現在アクティブな敵ゲームオブジェクトのリストを取得
        /// </summary>
        /// <returns>アクティブな敵のリスト</returns>
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
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "MetalonPurple":
                    foreach (GameObject j in ListMetalonPurple)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "MetalonRed":
                    foreach (GameObject j in ListMetalonRed)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "AttackBot":
                    foreach (GameObject j in ListAttackBot)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "RobotSphere":
                    foreach (GameObject j in ListRobotSphere)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "Dragon":
                    foreach (GameObject j in ListDragon)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Bull":
                    foreach (GameObject j in ListBull)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "StoneMonster":
                    foreach (GameObject j in ListStoneMonster)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "FreeLichS":
                    foreach (GameObject j in ListFreeLichS)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "FreeLich":
                    foreach (GameObject j in ListFreeLich)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "GolemS":
                    foreach (GameObject j in ListGolemS)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Golem":
                    foreach (GameObject j in ListGolem)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "SkeletonArmed":
                    foreach (GameObject j in ListSkeletonArmed)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "SpiderGhost":
                    foreach (GameObject j in ListSpiderGhost)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "Skeleton":
                    foreach (GameObject j in ListSkeleton)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "GruntS":
                    foreach (GameObject j in ListGruntS)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "FootmanS":
                    foreach (GameObject j in ListFootmanS)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Grunt":
                    foreach (GameObject j in ListGrunt)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Footman":
                    foreach (GameObject j in ListFootman)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "TurtleShell":
                    foreach (GameObject j in ListTurtleShell)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Mushroom":
                    foreach (GameObject j in ListMushroom)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "Slime":
                    foreach (GameObject j in ListSlime)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;

                case "PigChef":
                    foreach (GameObject j in ListPigChef)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "PhoenixChick":
                    foreach (GameObject j in ListPhoenixChick)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case "RockCritter":
                    foreach (GameObject j in ListRockCritter)
                    {
                        if (j == null) continue;
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
