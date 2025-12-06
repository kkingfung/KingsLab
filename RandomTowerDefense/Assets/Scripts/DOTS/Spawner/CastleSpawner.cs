using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using RandomTowerDefense.Units;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;

namespace RandomTowerDefense.DOTS.Spawner
{
    /// <summary>
    /// 城エンティティスポーナーシステム - プレイヤー城の生成と管理
    ///
    /// 主な機能:
    /// - 城エンティティの動的生成とECSアーキタイプ設定
    /// - ハイブリッドMonoBehaviour-ECS統合管理
    /// - 城ヘルス値のリアルタイム同期システム
    /// - シングルトンパターンによるグローバルアクセス
    /// - ネイティブ配列メモリ管理と自動リソース解放
    /// </summary>
    public class CastleSpawner : MonoBehaviour
    {
        #region Constants
        private readonly int _count = 1;
        #endregion

        #region Public Properties
        public static CastleSpawner Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("🏰 Castle Prefabs")]
        public List<GameObject> PrefabObject;
        #endregion

        #region Private Fields
        private EntityManager _entityManager;
        #endregion

        #region Public Arrays
        [HideInInspector]
        public NativeArray<int> castleHPArray;
        [HideInInspector]
        public GameObject[] GameObjects;
        public NativeArray<Entity> Entities;
        [HideInInspector]
        public Castle castle;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// シングルトンパターンの初期化
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        /// <summary>
        /// ネイティブ配列リソースの解放
        /// </summary>
        private void OnDisable()
        {
            if (Entities.IsCreated)
                Entities.Dispose();

            //if (TransformAccessArray.isCreated)
            //TransformAccessArray.Dispose();

            //Disposing Array
            if (castleHPArray.IsCreated)
                castleHPArray.Dispose();
        }
        /// <summary>
        /// エンティティマネージャーと配列の初期化
        /// </summary>
        private void Start()
        {

            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //Prepare input
            GameObjects = new GameObject[_count];
            //transforms = new Transform[count];

            Entities = new NativeArray<Entity>(_count, Allocator.Persistent);
            var archetype = _entityManager.CreateArchetype(
                 typeof(Health), typeof(Radius), typeof(Damage),
                ComponentType.ReadOnly<Translation>()
                );
            _entityManager.CreateEntity(archetype, Entities);
            castleHPArray = new NativeArray<int>(_count, Allocator.Persistent);
        }

        /// <summary>
        /// 城配列の更新処理
        /// </summary>
        private void Update()
        {
            UpdateArrays();
        }
        #endregion

        #region Public API
        /// <summary>
        /// 城ヘルス配列をECSエンティティと同期更新
        /// </summary>
        public void UpdateArrays()
        {
            for (int i = 0; i < GameObjects.Length; ++i)
            {
                if (GameObjects[i] == null) continue;
                if (GameObjects[i].activeSelf == false) continue;
                castleHPArray[i] = (int)_entityManager.GetComponentData<Health>(Entities[i]).Value;
            }
        }

        /// <summary>
        /// 城エンティティをスポーンし、ECSコンポーネントを設定
        /// </summary>
        /// <param name="Position">スポーン位置</param>
        /// <param name="Rotation">スポーン回転</param>
        /// <param name="castleHP">城のヘルス値</param>
        /// <param name="radius">城の半径</param>
        /// <param name="prefabID">使用するプレハブのID</param>
        /// <param name="num">スポーンする数</param>
        /// <returns>スポーンされた城のインデックス配列</returns>
        public int[] Spawn(float3 Position, Quaternion Rotation, int castleHP, float radius,
            int prefabID = 0, int num = 1)
        {
            int spawnCnt = 0;
            int[] spawnIndexList = new int[num];
            for (int i = 0; i < _count && spawnCnt < num; ++i)
            {
                if (GameObjects[i] != null && GameObjects[i].activeSelf) continue;

                GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
                GameObjects[i].transform.position = Position;
                GameObjects[i].transform.localRotation = Rotation;
                if (castle == null) castle = GameObjects[i].GetComponent<Castle>();
                // transforms[i] = GameObjects[i].transform;
                castleHPArray[i] = castleHP;

                _entityManager.SetComponentData(Entities[i], new Health
                {
                    Value = castleHP,
                });
                _entityManager.SetComponentData(Entities[i], new Radius
                {
                    Value = radius,
                });
                _entityManager.SetComponentData(Entities[i], new Damage
                {
                    Value = -1,
                });
                _entityManager.SetComponentData(Entities[i], new Translation
                {
                    Value = Position,
                });

                if (!_entityManager.HasComponent<CastleTag>(Entities[i]))
                    _entityManager.AddComponent<CastleTag>(Entities[i]);

                spawnIndexList[spawnCnt++] = i;

            }

            return spawnIndexList;
        }

        /// <summary>
        /// エンティティIDから対応するGameObjectを取得
        /// </summary>
        /// <param name="entityID">エンティティID</param>
        /// <returns>対応するGameObject</returns>
        public GameObject GetObjectByID(int entityID)
        {
            return GameObjects[entityID];
        }
        #endregion
    }
}
