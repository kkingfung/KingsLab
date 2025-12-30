using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.VFX;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.Common;
using RandomTowerDefense.DOTS.Components;

namespace RandomTowerDefense.DOTS.Spawner
{
    /// <summary>
    /// 攻撃エンティティスポーナーシステム - タワー攻撃プロジェクタイルの動的生成と管理
    ///
    /// 主な機能:
    /// - 4種類タワー攻撃タイプ対応プールシステム
    /// - ハイブリッドMonoBehaviour-ECS攻撃エンティティ管理
    /// - プロジェクタイル物理演算と衝突検知システム
    /// - VFXエフェクトと自動破棄タイマー管理
    /// - ゲームオブジェクトリユースとメモリ効率化
    /// </summary>
    public class AttackSpawner : MonoBehaviour
    {
        private readonly int _count = 200;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static AttackSpawner Instance { get; private set; }

        /// <summary>
        /// 攻撃プレハブオブジェクトリスト
        /// </summary>
        public List<GameObject> PrefabObject;

        private EntityManager _entityManager;

        /// <summary>
        /// Nightmareタワー攻撃プールリスト
        /// </summary>
        public List<GameObject> TowerNightmareAttack;

        /// <summary>
        /// SoulEaterタワー攻撃プールリスト
        /// </summary>
        public List<GameObject> TowerSoulEaterAttack;

        /// <summary>
        /// TerrorBringerタワー攻撃プールリスト
        /// </summary>
        public List<GameObject> TowerTerrorBringerAttack;

        /// <summary>
        /// Usurperタワー攻撃プールリスト
        /// </summary>
        public List<GameObject> TowerUsurperAttack;

        /// <summary>
        /// アクティブ攻撃ゲームオブジェクト配列
        /// </summary>
        [HideInInspector]
        public GameObject[] GameObjects;

        /// <summary>
        /// 攻撃エンティティ配列
        /// </summary>
        public NativeArray<Entity> Entities;

        private Transform[] _transforms;

        /// <summary>
        /// トランスフォームアクセス配列（Job System用）
        /// </summary>
        public TransformAccessArray TransformAccessArray;

        private void Update()
        {
        }

        /// <summary>
        /// 初期化処理 - シングルトンインスタンス設定
        /// </summary>
        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// 無効化時処理 - ネイティブ配列の解放
        /// </summary>
        void OnDisable()
        {
            if (Entities.IsCreated)
                Entities.Dispose();

            if (TransformAccessArray.isCreated)
                TransformAccessArray.Dispose();
        }

        /// <summary>
        /// 開始時処理 - プールとエンティティの初期化
        /// </summary>
        void Start()
        {
            TowerNightmareAttack = new List<GameObject>();
            TowerSoulEaterAttack = new List<GameObject>();
            TowerTerrorBringerAttack = new List<GameObject>();
            TowerUsurperAttack = new List<GameObject>();

            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 入力データの準備
            GameObjects = new GameObject[_count];
            _transforms = new Transform[_count];

            Entities = new NativeArray<Entity>(_count, Allocator.Persistent);
            var archetype = _entityManager.CreateArchetype(
                 typeof(Radius), typeof(Damage), typeof(Velocity),
                 typeof(WaitingTime), typeof(Lifetime), typeof(ActionTime),
                ComponentType.ReadOnly<Translation>()
                //ComponentType.ReadOnly<Hybrid>()
                );
            _entityManager.CreateEntity(archetype, Entities);
        }

        /// <summary>
        /// 攻撃プロジェクタイルをスポーン
        /// </summary>
        /// <param name="prefabID">プレハブID（0: Nightmare, 1: SoulEater, 2: TerrorBringer, 3: Usurper）</param>
        /// <param name="position">スポーン位置（ワールド座標）</param>
        /// <param name="entityposition">エンティティ位置</param>
        /// <param name="rotation">回転</param>
        /// <param name="velocity">速度ベクトル</param>
        /// <param name="damage">ダメージ量</param>
        /// <param name="radius">衝突半径</param>
        /// <param name="wait">待機時間</param>
        /// <param name="lifetime">生存時間</param>
        /// <param name="action">アクション時間（デフォルト: 0.2秒）</param>
        /// <param name="num">スポーン数（デフォルト: 1）</param>
        /// <returns>スポーンされたインデックス配列</returns>
        public int[] Spawn(int prefabID, float3 position, float3 entityposition, Quaternion rotation, float3 velocity, float damage,
            float radius, float wait, float lifetime, float action = 0.2f, int num = 1)
        {
            int spawnCnt = 0;
            int[] spawnIndexList = new int[num];

            for (int i = 0; i < _count && spawnCnt < num; ++i)
            {
                if (GameObjects[i] != null && GameObjects[i].activeSelf) continue;
                bool reuse = false;

                switch (prefabID)
                {
                    case 0:
                        foreach (GameObject j in TowerNightmareAttack)
                        {
                            if (j == null) continue;
                            if (j.activeSelf) continue;
                            GameObjects[i] = j;
                            reuse = true;
                            break;
                        }
                        break;
                    case 1:
                        foreach (GameObject j in TowerSoulEaterAttack)
                        {
                            if (j == null) continue;
                            if (j.activeSelf) continue;
                            GameObjects[i] = j;
                            reuse = true;
                            break;
                        }
                        break;
                    case 2:
                        foreach (GameObject j in TowerTerrorBringerAttack)
                        {
                            if (j == null) continue;
                            if (j.activeSelf) continue;
                            GameObjects[i] = j;
                            reuse = true;
                            break;
                        }
                        break;
                    case 3:
                        foreach (GameObject j in TowerUsurperAttack)
                        {
                            if (j == null) continue;
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

                GameObjects[i].transform.position = position;
                AutoDestroyVFX autoDestroy = GameObjects[i].GetComponent<AutoDestroyVFX>();
                if (autoDestroy) autoDestroy.Timer = lifetime;
                GameObjects[i].transform.localRotation = rotation;
                _transforms[i] = GameObjects[i].transform;

                // エンティティへ追加
                _entityManager.SetComponentData(Entities[i], new Damage
                {
                    Value = damage,
                });
                _entityManager.SetComponentData(Entities[i], new Radius
                {
                    Value = radius,
                });
                _entityManager.SetComponentData(Entities[i], new Velocity
                {
                    Value = velocity,
                });
                _entityManager.SetComponentData(Entities[i], new WaitingTime
                {
                    Value = wait,
                });
                _entityManager.SetComponentData(Entities[i], new Lifetime
                {
                    Value = lifetime,
                });
                _entityManager.SetComponentData(Entities[i], new ActionTime
                {
                    Value = action,
                });

                _entityManager.SetComponentData(Entities[i], new Translation
                {
                    Value = entityposition,
                });

                // Note: Hybrid component not available in this version
                // _entityManager.SetComponentData(Entities[i], new Hybrid { Index = i });

                if (_entityManager.HasComponent<AttackTag>(Entities[i]) == false)
                    _entityManager.AddComponent<AttackTag>(Entities[i]);

                spawnIndexList[spawnCnt++] = i;
            }

            // スポーン時に変更（不要な可能性あり）
            if (TransformAccessArray.isCreated)
                TransformAccessArray.Dispose();
            TransformAccessArray = new TransformAccessArray(_transforms);
            return spawnIndexList;
        }
    }
}