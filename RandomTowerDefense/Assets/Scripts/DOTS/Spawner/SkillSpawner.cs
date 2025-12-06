using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.VFX;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;

namespace RandomTowerDefense.DOTS.Spawner
{
    /// <summary>
    /// スキルエンティティスポーナーシステム - 魔法スキルの動的生成と管理
    ///
    /// 主な機能:
    /// - 4種類魔法スキル（Meteor、Blizzard、Petrification、Minions）対応
    /// - ハイブリッドMonoBehaviour-ECS統合スキル管理システム
    /// - スキルエリアエフェクト処理とリアルタイム効果適用
    /// - VFXグラフ統合とスキル持続時間管理
    /// - スキル種別リストとゲームオブジェクトプール最適化
    /// </summary>
    public class SkillSpawner : MonoBehaviour
    {
        private readonly int _count = 10;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static SkillSpawner Instance { get; private set; }

        /// <summary>
        /// スキルプレハブオブジェクトリスト
        /// </summary>
        public List<GameObject> PrefabObject;

        /// <summary>
        /// Meteorスキルプールリスト
        /// </summary>
        public List<GameObject> MeteorList;

        /// <summary>
        /// Blizzardスキルプールリスト
        /// </summary>
        public List<GameObject> BlizzardList;

        /// <summary>
        /// Petrificationスキルプールリスト
        /// </summary>
        public List<GameObject> PetrificationList;

        /// <summary>
        /// Minionsスキルプールリスト
        /// </summary>
        public List<GameObject> MinionsList;

        private EntityManager _entityManager;

        /// <summary>
        /// スキル生存時間配列
        /// </summary>
        [HideInInspector]
        public NativeArray<float> lifetimeArray;

        /// <summary>
        /// スキルターゲット位置配列
        /// </summary>
        public NativeArray<float3> targetArray;

        /// <summary>
        /// スキルターゲット存在フラグ配列
        /// </summary>
        public NativeArray<bool> hastargetArray;

        /// <summary>
        /// アクティブスキルゲームオブジェクト配列
        /// </summary>
        [HideInInspector]
        public GameObject[] GameObjects;

        /// <summary>
        /// スキルエンティティ配列
        /// </summary>
        public NativeArray<Entity> Entities;

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

            //if (TransformAccessArray.isCreated)
            //TransformAccessArray.Dispose();

            // 配列の破棄
            if (lifetimeArray.IsCreated)
                lifetimeArray.Dispose();
            if (targetArray.IsCreated)
                targetArray.Dispose();
            if (hastargetArray.IsCreated)
                hastargetArray.Dispose();
        }

        /// <summary>
        /// 開始時処理 - プールとエンティティの初期化
        /// </summary>
        void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // 入力データの準備
            GameObjects = new GameObject[_count];
            //_transforms = new Transform[_count];
            //MeteorList = new List<GameObject>();
            //BlizzardList = new List<GameObject>();
            //PetrificationList = new List<GameObject>();
            //MinionsList = new List<GameObject>();

            Entities = new NativeArray<Entity>(_count, Allocator.Persistent);
            var archetype = _entityManager.CreateArchetype(
                typeof(Damage), typeof(Radius),
                typeof(WaitingTime), typeof(ActionTime),
                typeof(Lifetime), typeof(SlowRate), typeof(BuffTime),
                ComponentType.ReadOnly<Translation>()
                //ComponentType.ReadOnly<Hybrid>()
                );
            _entityManager.CreateEntity(archetype, Entities);

            //TransformAccessArray = new TransformAccessArray(_transforms);
            lifetimeArray = new NativeArray<float>(_count, Allocator.Persistent);
            targetArray = new NativeArray<float3>(_count, Allocator.Persistent);
            hastargetArray = new NativeArray<bool>(_count, Allocator.Persistent);
        }
        /// <summary>
        /// 毎フレーム更新 - スキル配列の同期
        /// </summary>
        private void Update()
        {
            UpdateArrays();
        }

        /// <summary>
        /// スキル配列をECSエンティティと同期更新
        /// </summary>
        public void UpdateArrays()
        {
            for (int i = 0; i < GameObjects.Length; ++i)
            {
                if (GameObjects[i] == null) continue;
                if (GameObjects[i].activeSelf == false) continue;
                lifetimeArray[i] = _entityManager.GetComponentData<Lifetime>(Entities[i]).Value;
                if (_entityManager.HasComponent<Target>(Entities[i]))
                {
                    Target target = _entityManager.GetComponentData<Target>(Entities[i]);
                    targetArray[i] = target.targetPos;
                    hastargetArray[i] = _entityManager.HasComponent<EnemyTag>(target.targetEntity);
                    Debug.DrawLine(target.targetPos, GameObjects[i].transform.position, Color.cyan);
                }
                else
                {
                    hastargetArray[i] = false;
                }
            }
        }

        /// <summary>
        /// 魔法スキルをスポーン
        /// </summary>
        /// <param name="prefabID">プレハブID（0: Meteor, 1: Blizzard, 2: Petrification, 3: Minions）</param>
        /// <param name="Position">スポーン位置（ワールド座標）</param>
        /// <param name="EntityPos">エンティティ位置</param>
        /// <param name="Rotation">回転</param>
        /// <param name="damage">ダメージ量</param>
        /// <param name="radius">効果範囲</param>
        /// <param name="wait">待機時間</param>
        /// <param name="lifetime">生存時間</param>
        /// <param name="action">アクション時間</param>
        /// <param name="slow">スロー効果率（デフォルト: 0）</param>
        /// <param name="buff">バフ継続時間（デフォルト: 0）</param>
        /// <param name="num">スポーン数（デフォルト: 1）</param>
        /// <returns>スポーンされたインデックス配列</returns>
        public int[] Spawn(int prefabID, float3 Position, float3 EntityPos, float3 Rotation, float damage, float radius,
            float wait, float lifetime,
            float action, float slow = 0, float buff = 0,
            int num = 1)
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
                        foreach (GameObject j in MeteorList)
                        {
                            if (j == null) continue;
                            if (j.activeSelf) continue;
                            GameObjects[i] = j;
                            reuse = true;
                            break;
                        }
                        break;
                    case 1:
                        foreach (GameObject j in BlizzardList)
                        {
                            if (j == null) continue;
                            if (j.activeSelf) continue;
                            GameObjects[i] = j;
                            reuse = true;
                            break;
                        }
                        break;
                    case 2:
                        foreach (GameObject j in PetrificationList)
                        {
                            if (j == null) continue;
                            if (j.activeSelf) continue;
                            GameObjects[i] = j;
                            reuse = true;
                            break;
                        }
                        break;
                    case 3:
                        foreach (GameObject j in MinionsList)
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
                            MeteorList.Add(GameObjects[i]);
                            break;
                        case 1:
                            BlizzardList.Add(GameObjects[i]);
                            break;
                        case 2:
                            PetrificationList.Add(GameObjects[i]);
                            break;
                        case 3:
                            MinionsList.Add(GameObjects[i]);
                            break;
                    }
                }
                else
                {
                    GameObjects[i].SetActive(true);
                    GameObjects[i].GetComponent<VisualEffect>().Play();
                }

                //GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
                GameObjects[i].transform.position = Position;
                GameObjects[i].transform.localRotation = Quaternion.Euler(Rotation);
                //_transforms[i] = GameObjects[i].transform;
                lifetimeArray[i] = lifetime;

                // エンティティへ追加
                _entityManager.SetComponentData(Entities[i], new Damage
                {
                    Value = damage,
                });
                _entityManager.SetComponentData(Entities[i], new Radius
                {
                    Value = radius,
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
                _entityManager.SetComponentData(Entities[i], new SlowRate
                {
                    Value = slow,
                });
                _entityManager.SetComponentData(Entities[i], new BuffTime
                {
                    Value = buff,
                });

                if (_entityManager.HasComponent<QuadrantEntity>(Entities[i]))
                {
                    _entityManager.RemoveComponent(Entities[i], typeof(QuadrantEntity));
                }

                if (_entityManager.HasComponent<MeteorTag>(Entities[i]))
                {
                    _entityManager.RemoveComponent(Entities[i], typeof(MeteorTag));
                }

                if (_entityManager.HasComponent<BlizzardTag>(Entities[i]))
                {
                    _entityManager.RemoveComponent(Entities[i], typeof(BlizzardTag));
                }

                if (_entityManager.HasComponent<PetrificationTag>(Entities[i]))
                {
                    _entityManager.RemoveComponent(Entities[i], typeof(PetrificationTag));
                }

                if (_entityManager.HasComponent<MinionsTag>(Entities[i]))
                {
                    _entityManager.RemoveComponent(Entities[i], typeof(MinionsTag));
                }

                if (_entityManager.HasComponent<Target>(Entities[i]))
                {
                    _entityManager.RemoveComponent(Entities[i], typeof(Target));
                }

                switch (prefabID)
                {
                    case 0:
                        _entityManager.AddComponent(Entities[i], typeof(MeteorTag));
                        break;
                    case 1:
                        _entityManager.AddComponent(Entities[i], typeof(BlizzardTag));
                        break;
                    case 2:
                        _entityManager.AddComponent(Entities[i], typeof(PetrificationTag));
                        break;
                    case 3:
                        _entityManager.AddComponent(Entities[i], typeof(MinionsTag));
                        _entityManager.AddComponent(Entities[i], typeof(QuadrantEntity));
                        _entityManager.SetComponentData(Entities[i], new QuadrantEntity
                        {
                            typeEnum = QuadrantEntity.TypeEnum.MinionsTag
                        });
                        break;
                }

                _entityManager.SetComponentData(Entities[i], new Translation
                {
                    Value = EntityPos
                });

                //_entityManager.SetComponentData(Entities[i], new Hybrid
                //{
                //    Index = i,
                //});

                if (_entityManager.HasComponent<SkillTag>(Entities[i]) == false)
                    _entityManager.AddComponent<SkillTag>(Entities[i]);
                spawnIndexList[spawnCnt++] = i;
            }

            // スポーン時に変更（不要な可能性あり）
            //TransformAccessArray = new TransformAccessArray(_transforms);
            return spawnIndexList;
        }

        /// <summary>
        /// スキルエンティティの位置を更新
        /// </summary>
        /// <param name="entityID">エンティティID</param>
        /// <param name="pos">新しい位置</param>
        public void UpdateEntityPos(int entityID, Vector3 pos)
        {
            _entityManager.SetComponentData(Entities[entityID], new Translation
            {
                Value = pos,
            });
        }

        /// <summary>
        /// 現在アクティブなスキルゲームオブジェクトのリストを取得
        /// </summary>
        /// <returns>アクティブなスキルのリスト</returns>
        public List<GameObject> AllAliveSkillsList()
        {
            List<GameObject> result = new List<GameObject>();
            foreach (GameObject i in GameObjects)
            {
                if (i != null && i.activeSelf)
                    result.Add(i);
            }
            return result;
        }
    }
}