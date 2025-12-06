using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;
using RandomTowerDefense.Info;

namespace RandomTowerDefense.DOTS.Spawner
{
    /// <summary>
    /// タワーエンティティスポーナーシステム - 4種類タワーの動的生成と管理
    ///
    /// 主な機能:
    /// - 4種類タワー（Nightmare、SoulEater、TerrorBringer、Usurper）各ランク1-4対応
    /// - ハイブリッドMonoBehaviour-ECS統合管理システム
    /// - リアルタイムタワーターゲッティング配列同期
    /// - タワー合成システムとランクアップ処理
    /// - 座標変換アクセス配列とネイティブ配列最適化
    /// - タワー種別・ランク別リスト管理システム
    /// </summary>
    public class TowerSpawner : MonoBehaviour
    {
        /// <summary>
        /// タワーの最大ランク数
        /// </summary>
        public const int MonsterMaxRank = 4;
        private readonly int _count = 45;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static TowerSpawner Instance { get; private set; }

        /// <summary>
        /// タワープレハブオブジェクトリスト
        /// </summary>
        public List<GameObject> PrefabObject;

        private EntityManager _entityManager;

        /// <summary>
        /// Nightmareタワーランク1プールリスト
        /// </summary>
        public List<GameObject> TowerNightmareRank1;
        /// <summary>
        /// Nightmareタワーランク2プールリスト
        /// </summary>
        public List<GameObject> TowerNightmareRank2;
        /// <summary>
        /// Nightmareタワーランク3プールリスト
        /// </summary>
        public List<GameObject> TowerNightmareRank3;
        /// <summary>
        /// Nightmareタワーランク4プールリスト
        /// </summary>
        public List<GameObject> TowerNightmareRank4;

        /// <summary>
        /// SoulEaterタワーランク1プールリスト
        /// </summary>
        public List<GameObject> TowerSoulEaterRank1;
        /// <summary>
        /// SoulEaterタワーランク2プールリスト
        /// </summary>
        public List<GameObject> TowerSoulEaterRank2;
        /// <summary>
        /// SoulEaterタワーランク3プールリスト
        /// </summary>
        public List<GameObject> TowerSoulEaterRank3;
        /// <summary>
        /// SoulEaterタワーランク4プールリスト
        /// </summary>
        public List<GameObject> TowerSoulEaterRank4;

        /// <summary>
        /// TerrorBringerタワーランク1プールリスト
        /// </summary>
        public List<GameObject> TowerTerrorBringerRank1;
        /// <summary>
        /// TerrorBringerタワーランク2プールリスト
        /// </summary>
        public List<GameObject> TowerTerrorBringerRank2;
        /// <summary>
        /// TerrorBringerタワーランク3プールリスト
        /// </summary>
        public List<GameObject> TowerTerrorBringerRank3;
        /// <summary>
        /// TerrorBringerタワーランク4プールリスト
        /// </summary>
        public List<GameObject> TowerTerrorBringerRank4;

        /// <summary>
        /// Usurperタワーランク1プールリスト
        /// </summary>
        public List<GameObject> TowerUsurperRank1;
        /// <summary>
        /// Usurperタワーランク2プールリスト
        /// </summary>
        public List<GameObject> TowerUsurperRank2;
        /// <summary>
        /// Usurperタワーランク3プールリスト
        /// </summary>
        public List<GameObject> TowerUsurperRank3;
        /// <summary>
        /// Usurperタワーランク4プールリスト
        /// </summary>
        public List<GameObject> TowerUsurperRank4;

        /// <summary>
        /// トランスフォームアクセス配列（Job System用）
        /// </summary>
        [HideInInspector]
        public TransformAccessArray TransformAccessArray;

        /// <summary>
        /// タワーターゲット位置配列
        /// </summary>
        public NativeArray<float3> targetArray;

        /// <summary>
        /// タワーターゲット存在フラグ配列
        /// </summary>
        public NativeArray<bool> hastargetArray;

        /// <summary>
        /// アクティブタワーゲームオブジェクト配列
        /// </summary>
        [HideInInspector]
        public GameObject[] GameObjects;

        /// <summary>
        /// タワーエンティティ配列
        /// </summary>
        public NativeArray<Entity> Entities;

        // 入力用トランスフォーム
        private Transform[] _transforms;

        /// <summary>
        /// 初期化処理 - シングルトンインスタンス設定
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
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
            if (targetArray.IsCreated)
                targetArray.Dispose();
            if (hastargetArray.IsCreated)
                hastargetArray.Dispose();
        }

        /// <summary>
        /// 開始時処理 - プールとエンティティの初期化
        /// </summary>
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

            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 入力データの準備
            GameObjects = new GameObject[_count];
            _transforms = new Transform[_count];

            Entities = new NativeArray<Entity>(_count, Allocator.Persistent);
            var archetype = _entityManager.CreateArchetype(
                 typeof(WaitingTime), typeof(Radius), typeof(CastlePos),
                 typeof(Damage), typeof(LocalToWorld),
                ComponentType.ReadOnly<Translation>()
                );
            _entityManager.CreateEntity(archetype, Entities);


            TransformAccessArray = new TransformAccessArray(_transforms);
            targetArray = new NativeArray<float3>(_count, Allocator.Persistent);
            hastargetArray = new NativeArray<bool>(_count, Allocator.Persistent);
        }

        /// <summary>
        /// 毎フレーム更新 - タワー配列の同期
        /// </summary>
        private void Update()
        {
            UpdateArrays();
        }

        /// <summary>
        /// タワー配列をECSエンティティと同期更新
        /// </summary>
        public void UpdateArrays()
        {
            for (int i = 0; i < GameObjects.Length; ++i)
            {
                if (GameObjects[i] == null) continue;
                if (GameObjects[i].activeSelf == false) continue;
                if (_entityManager.HasComponent<Target>(Entities[i]))
                {
                    Target target = _entityManager.GetComponentData<Target>(Entities[i]);
                    targetArray[i] = target.targetPos;
                    hastargetArray[i] = _entityManager.HasComponent<EnemyTag>(target.targetEntity);
                    //Debug.DrawLine(target.targetPos, GameObjects[i].transform.position, Color.cyan);
                }
                else
                {
                    hastargetArray[i] = false;
                }
            }
        }

        /// <summary>
        /// タワーをスポーン
        /// </summary>
        /// <param name="prefabID">プレハブID（タワー種別 × 4 + ランク）</param>
        /// <param name="Position">スポーン位置（ワールド座標）</param>
        /// <param name="CastlePosition">城の位置</param>
        /// <param name="num">スポーン数（デフォルト: 1）</param>
        /// <returns>スポーンされたインデックス配列</returns>
        public int[] Spawn(int prefabID, float3 Position, float3 CastlePosition, int num = 1)
        {
            int spawnCnt = 0;
            int[] spawnIndexList = new int[num];
            for (int i = 0; i < _count && spawnCnt < num; ++i)
            {
                if (GameObjects[i] != null && GameObjects[i].activeSelf) continue;

                GameObjects[i] = SpawnFromList(prefabID);

                if (GameObjects[i] == null)
                {
                    GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
                    AddToList(GameObjects[i], prefabID);
                }
                else
                {
                    GameObjects[i].SetActive(true);
                }
                GameObjects[i].transform.position = Position;
                GameObjects[i].transform.localRotation = Quaternion.identity;
                _transforms[i] = GameObjects[i].transform;
                hastargetArray[i] = false;
                targetArray[i] = Position;

                // エンティティへ追加
                _entityManager.SetComponentData(Entities[i], new WaitingTime
                {
                    Value = float.MaxValue,
                });

                _entityManager.SetComponentData(Entities[i], new Radius
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
                    typeEnum = QuadrantEntity.TypeEnum.PlayerTag
                });

                _entityManager.SetComponentData(Entities[i], new CastlePos
                {
                    Value = CastlePosition,
                });

                if (_entityManager.HasComponent<PlayerTag>(Entities[i]) == false)
                    _entityManager.AddComponent<PlayerTag>(Entities[i]);


                spawnIndexList[spawnCnt++] = i;
            }

            // スポーン時に変更（不要な可能性あり）
            if (TransformAccessArray.isCreated)
                TransformAccessArray.Dispose();
            TransformAccessArray = new TransformAccessArray(_transforms);
            return spawnIndexList;
        }

        /// <summary>
        /// 現在アクティブなタワーゲームオブジェクトのリストを取得
        /// </summary>
        /// <returns>アクティブなタワーのリスト</returns>
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
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerNightmare + 0:
                    foreach (GameObject j in TowerNightmareRank1)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerNightmare + 1:
                    foreach (GameObject j in TowerNightmareRank2)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerNightmare + 2:
                    foreach (GameObject j in TowerNightmareRank3)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerNightmare + 3:
                    foreach (GameObject j in TowerNightmareRank4)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerSoulEater + 0:
                    foreach (GameObject j in TowerSoulEaterRank1)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerSoulEater + 1:
                    foreach (GameObject j in TowerSoulEaterRank2)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerSoulEater + 2:
                    foreach (GameObject j in TowerSoulEaterRank3)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerSoulEater + 3:
                    foreach (GameObject j in TowerSoulEaterRank4)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerTerrorBringer + 0:
                    foreach (GameObject j in TowerTerrorBringerRank1)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerTerrorBringer + 1:
                    foreach (GameObject j in TowerTerrorBringerRank2)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerTerrorBringer + 2:
                    foreach (GameObject j in TowerTerrorBringerRank3)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerTerrorBringer + 3:
                    foreach (GameObject j in TowerTerrorBringerRank4)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerUsurper + 0:
                    foreach (GameObject j in TowerUsurperRank1)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerUsurper + 1:
                    foreach (GameObject j in TowerUsurperRank2)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerUsurper + 2:
                    foreach (GameObject j in TowerUsurperRank3)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerUsurper + 3:
                    foreach (GameObject j in TowerUsurperRank4)
                    {
                        if (j == null) continue;
                        if (j.activeSelf) continue;
                        return j;
                    }
                    break;
            }
            return null;
        }

        private void AddToList(GameObject obj, int prefabID)
        {
            switch (prefabID)
            {
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerNightmare + 0:
                    TowerNightmareRank1.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerNightmare + 1:
                    TowerNightmareRank2.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerNightmare + 2:
                    TowerNightmareRank3.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerNightmare + 3:
                    TowerNightmareRank4.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerSoulEater + 0:
                    TowerSoulEaterRank1.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerSoulEater + 1:
                    TowerSoulEaterRank2.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerSoulEater + 2:
                    TowerSoulEaterRank3.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerSoulEater + 3:
                    TowerSoulEaterRank4.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerTerrorBringer + 0:
                    TowerTerrorBringerRank1.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerTerrorBringer + 1:
                    TowerTerrorBringerRank2.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerTerrorBringer + 2:
                    TowerTerrorBringerRank3.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerTerrorBringer + 3:
                    TowerTerrorBringerRank4.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerUsurper + 0:
                    TowerUsurperRank1.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerUsurper + 1:
                    TowerUsurperRank2.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerUsurper + 2:
                    TowerUsurperRank3.Add(obj);
                    break;
                case MonsterMaxRank * (int)TowerInfo.TowerInfoID.EnumTowerUsurper + 3:
                    TowerUsurperRank4.Add(obj);
                    break;
            }
        }
    }
}
