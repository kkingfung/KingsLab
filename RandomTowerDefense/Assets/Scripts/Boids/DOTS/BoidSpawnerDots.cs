using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using RandomTowerDefense.Boids.DOTS;

namespace RandomTowerDefense.Boids.DOTS
{
    /// <summary>
    /// DOTSボイドスポナー - Unity ECSアーキテクチャ対応高性能ボイド生成システム
    ///
    /// 主な機能:
    /// - シングルトンパターンによるグローバルアクセス管理
    /// - Unity ECSエンティティとMonoBehaviourのハイブリッド管理
    /// - NativeArrayを使用したメモリ効率的エンティティ管理
    /// - アーキタイプベースエンティティ一括生成システム
    /// - GameObject-Entity間のリアルタイム同期処理
    /// - 動的スポーン数制御とプール管理システム
    /// </summary>
    public class BoidSpawnerDots : MonoBehaviour
    {
        #region Constants

        private const int MAX_COUNT = 50;

        #endregion

        #region Singleton

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static BoidSpawnerDots Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("スポーン設定")]
        [SerializeField] [Tooltip("生成するボイドのプレファブリスト")]
        public List<GameObject> prefab;

        [SerializeField] [Tooltip("ボイド動作設定")]
        public BoidSettings settings;

        [SerializeField] [Tooltip("生成範囲の半径")]
        public float spawnRadius = 10f;

        [SerializeField] [Range(0, 50)] [Tooltip("生成するボイドの数")]
        public int spawnCount = 10;

        [SerializeField] [Tooltip("境界範囲の半径")]
        public float BoundingRadius = 100f;

        [Header("DOTSブリッジ")]
        [HideInInspector] public GameObject[] GameObjects;
        [HideInInspector] public NativeArray<Entity> Entities;

        #endregion

        #region Private Fields

        private EntityManager _entityManager;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 起動時初期化 - シングルトン設定
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 開始処理 - ECSアーキタイプ作成とエンティティ初期化
        /// </summary>
        private void Start()
        {
            InitializeECS();
            Spawn(spawnCount);
        }
        /// <summary>
        /// 毎フレーム更新 - GameObject-Entity間の同期処理
        /// </summary>
        private void Update()
        {
            SynchronizeGameObjectsWithEntities();
        }

        /// <summary>
        /// 無効化時処理 - メモリリソースの解放
        /// </summary>
        private void OnDisable()
        {
            if (Entities.IsCreated)
            {
                Entities.Dispose();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// ボイドスポーン処理 - 指定数のボイドエンティティ生成
        /// </summary>
        /// <param name="num">生成するボイド数</param>
        /// <returns>生成されたボイドのインデックス配列</returns>
        public int[] Spawn(int num = 1)
        {
            int spawnCnt = 0;
            int[] spawnIndexList = new int[num];

            for (int i = 0; i < MAX_COUNT && spawnCnt < num; ++i)
            {
                if (GameObjects[i] != null) continue;

                Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
                int rand = UnityEngine.Random.Range(0, prefab.Count);

                // GameObject生成と初期設定
                GameObjects[i] = Instantiate(prefab[rand]);
                GameObjects[i].transform.position = pos;
                GameObjects[i].transform.forward = UnityEngine.Random.insideUnitSphere.normalized;
                GameObjects[i].transform.parent = this.transform;

                // エンティティコンポーネント設定
                SetupEntityComponents(i, pos);

                spawnIndexList[spawnCnt++] = i;
            }

            return spawnIndexList;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ECSシステム初期化 - アーキタイプ作成とエンティティ配列準備
        /// </summary>
        private void InitializeECS()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            GameObjects = new GameObject[MAX_COUNT];

            Entities = new NativeArray<Entity>(MAX_COUNT, Allocator.Persistent);
            var archetype = _entityManager.CreateArchetype(
                typeof(BoidData), typeof(Velocity), typeof(OriPos),
                typeof(BoidRotation), typeof(BoidSettingDots), typeof(BoidDataAvg)
            );
            _entityManager.CreateEntity(archetype, Entities);
        }

        /// <summary>
        /// GameObject-Entity同期処理 - 位置と回転の双方向同期
        /// </summary>
        private void SynchronizeGameObjectsWithEntities()
        {
            for (int i = 0; i < GameObjects.Length; ++i)
            {
                if (GameObjects[i] == null) continue;

                BoidData data = _entityManager.GetComponentData<BoidData>(Entities[i]);
                GameObjects[i].transform.position = data.position;
                GameObjects[i].transform.forward = data.direction;

                _entityManager.SetComponentData<BoidRotation>(Entities[i], new BoidRotation
                {
                    rotation = GameObjects[i].transform.rotation
                });
            }
        }

        /// <summary>
        /// エンティティコンポーネント設定 - 全必要コンポーネントの初期化
        /// </summary>
        /// <param name="index">設定するインデックス</param>
        /// <param name="position">初期位置</param>
        private void SetupEntityComponents(int index, Vector3 position)
        {
            // BoidData設定
            _entityManager.SetComponentData(Entities[index], new BoidData
            {
                position = GameObjects[index].transform.position,
                direction = GameObjects[index].transform.forward,
                flockHeading = new float3(),
                flockCentre = new float3(),
                avoidanceHeading = new float3(),
                numFlockmates = 0
            });

            // BoidRotation設定
            _entityManager.SetComponentData(Entities[index], new BoidRotation
            {
                rotation = GameObjects[index].transform.rotation
            });

            // BoidSettingDots設定
            _entityManager.SetComponentData(Entities[index], new BoidSettingDots
            {
                minSpeed = settings.minSpeed,
                maxSpeed = settings.maxSpeed,
                perceptionRadius = settings.perceptionRadius,
                avoidanceRadius = settings.avoidanceRadius,
                maxSteerForce = settings.maxSteerForce,
                alignWeight = settings.alignWeight,
                cohesionWeight = settings.cohesionWeight,
                seperateWeight = settings.seperateWeight,
                targetWeight = settings.targetWeight,
                boundsRadius = settings.boundsRadius,
                avoidCollisionWeight = settings.avoidCollisionWeight,
                collisionAvoidDst = settings.collisionAvoidDst,
            });

            // OriPos設定
            _entityManager.SetComponentData(Entities[index], new OriPos
            {
                Value = position,
                BoundingRadius = BoundingRadius
            });

            // Velocity設定
            _entityManager.SetComponentData(Entities[index], new Velocity
            {
                Value = GameObjects[index].transform.forward * ((settings.minSpeed + settings.maxSpeed) / 2f)
            });

            // BoidTag追加
            if (_entityManager.HasComponent<BoidTag>(Entities[index]) == false)
            {
                _entityManager.AddComponent<BoidTag>(Entities[index]);
            }
        }

        #endregion
    }
}