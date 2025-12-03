using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;
using Unity.Entities;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.MapGenerator;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Info;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;

namespace RandomTowerDefense.Units
{
    /// <summary>
    /// タワーユニットシステム - 攻撃タワーの行動と成長管理
    ///
    /// 主な機能:
    /// - ランクベース成長システム（ランク1-4）
    /// - 経験値とレベルアップシステム
    /// - 敵ターゲティングと攻撃処理
    /// - ビジュアルエフェクトとアニメーション
    /// - マージシステム統合
    /// - ECSアーキテクチャ連携
    /// - パフォーマンス最適化
    /// </summary>
    public class Tower : MonoBehaviour
    {
        #region Constants

        // レベルシステム定数
        private readonly int[] MaxLevel = { 5, 10, 20, 40 };
        private readonly int[] MaxLevelBonus = { 100, 300, 500, 1000 };
        private readonly int ExpPerAttack = 2;

        // 経験値倍率 (タワータイプ別)
        private const int NIGHTMARE_EXP_MULTIPLIER = 3;
        private const int SOULEATER_EXP_MULTIPLIER = 2;
        private const int TERRORBRINGER_EXP_MULTIPLIER = 5;
        private const int USURPER_EXP_MULTIPLIER = 1;

        // 攻撃位置調整値
        private static readonly Vector3 NIGHTMARE_ATTACK_POSITION_OFFSET = new Vector3(0f, 0f, 0.2f);
        private static readonly Vector3 SOULEATER_ATTACK_POSITION_OFFSET = new Vector3(0f, 0f, -0.2f);
        private static readonly Vector3 TERRORBRINGER_ATTACK_POSITION_OFFSET = new Vector3(0f, 0.1f, 0f);
        private static readonly Vector3 USURPER_ATTACK_POSITION_OFFSET = new Vector3(0f, 0.15f, 0.5f);

        // VFX パラメータ定数
        private const float NIGHTMARE_STAR_SIZE_MULTIPLIER = 10f;
        private const float NIGHTMARE_AURA_SIZE_MULTIPLIER = 0.5f;
        private const float SOULEATER_SPAWN_RATE_MULTIPLIER = 1f;
        private const float TERRORBRINGER_SKULL_SIZE_MULTIPLIER = 10f;
        private const float TERRORBRINGER_SKULL_SIZE_BASE = 10f;
        private const float USURPER_SIZE_MULTIPLIER = 0.1f;

        // その他の定数
        private readonly float TowerDestroyTime = 2;
        private readonly int ActionSetNum = 2;

        // 経験値システム定数
        private const int EXP_BASE_MULTIPLIER = 10;

        // VFXスケール定数
        private const float VFX_SPAWN_RATE_LEVEL_MULTIPLIER = 0.5f;
        private const float AURA_VFX_LEVEL_MULTIPLIER = 10.0f;
        private const float AURA_VFX_DEFAULT_SCALE = 1.0f;

        // アップグレード半径定数
        private const float RANK_UPGRADE_RADIUS_BASE = 0.1f;
        private const float LEVEL_UPGRADE_RADIUS_BASE = 0.05f;

        // ゲームステート定数
        private const int GAME_SUCCESS_RESULT = 0;

        #endregion

        #region Public Properties

        /// <summary>
        /// タワーの戦闘属性（ダメージ、範囲、打撃間隔など）
        /// </summary>
        public TowerAttr attr;

        /// <summary>
        /// タワーの現在レベル
        /// </summary>
        public int level;

        /// <summary>
        /// タワーの現在ランク（1-4）
        /// </summary>
        public int rank;

        /// <summary>
        /// タワーの現在経験値
        /// </summary>
        public int exp;

        /// <summary>
        /// タワーのタイプ（Nightmare、SoulEater、TerrorBringer、Usurper）
        /// </summary>
        public TowerInfo.TowerInfoID type;

        #endregion

        #region Private Fields

        private float atkCounter;
        private Vector3 atkEntityPos;

        private List<GameObject> AtkVFX;
        private GameObject auraVFX = null;

        private GameObject auraVFXPrefab;
        private GameObject lvupVFXPrefab;

        private AudioSource audioSource;
        private GameObject defaultTarget;

        #endregion

        #region Manager References

        /// <summary>
        /// オーディオ管理クラスの参照
        /// </summary>
        public AudioManager audioManager;

        /// <summary>
        /// リソース管理クラスの参照
        /// </summary>
        public ResourceManager resourceManager;

        /// <summary>
        /// ステージ管理クラスの参照
        /// </summary>
        public StageManager stageManager;

        /// <summary>
        /// タワーが建設された柱のGameObject
        /// </summary>
        public GameObject pillar;

        /// <summary>
        /// アップグレード管理クラスの参照
        /// </summary>
        public UpgradesManager upgradesManager;

        #endregion

        #region Private Fields (Components)

        private Animator _animator;
        private VisualEffect _auraVFXComponent;
        private VisualEffect _lvupVFXComponent;
        private BoxCollider _boxCollider;

        #endregion

        #region Private Fields (ECS Integration)

        private int _entityID;
        private TowerSpawner _towerSpawner;
        private AttackSpawner _attackSpawner;
        private EntityManager _entityManager;

        #endregion

        #region Private Fields (System Integration)

        private FilledMapGenerator _filledMapGenerator;
        private BonusChecker _bonusChecker;
        private DebugManager _debugManager;

        #endregion

        #region Private Fields (State Management)

        private float3 _oriScale;
        private bool _newlySpawned;

        #endregion

        #region Public Properties (VFX)

        /// <summary>
        /// レベルアップ時のビジュアルエフェクトコンポーネント
        /// </summary>
        public VisualEffect lvupVFXComponent
        {
            get => _lvupVFXComponent;
            set => _lvupVFXComponent = value;
        }

        #endregion
        #region Unity Lifecycle

        /// <summary>
        /// タワーコンポーネントの初期化処理
        /// </summary>
        private void Awake()
        {
            InitializeFields();
            InitializeComponents();
            InitializeECSIntegration();
        }

        /// <summary>
        /// タワーシステムの依存関係初期化とアニメーション開始
        /// </summary>
        private void Start()
        {
            InitializeManagerReferences();
            InitializeAnimationState();
        }

        /// <summary>
        /// タワーの再有効化時のアニメーション処理
        /// </summary>
        private void OnEnable()
        {
            if (_newlySpawned == false)
            {
                transform.localScale = new Vector3();
                StartCoroutine(StartAnim());
            }
        }

        /// <summary>
        /// タワーの無効化時の状態管理
        /// </summary>
        private void OnDisable()
        {
            _newlySpawned = false;
        }

        /// <summary>
        /// タワーの攻撃タイミングと敵ターゲティング処理（ECS最適化版）
        /// </summary>
        void Update()
        {
            // フレームレートに依存しない攻撃タイマー更新
            float deltaTime = Time.deltaTime;
            if (atkCounter > 0)
            {
                atkCounter -= deltaTime;
            }

            // ECSシステムとの連携で最小限のチェックと処理
            if (atkCounter <= 0 && CanAttackOptimized())
            {
                ProcessTargetAndAttack();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// タワーの攻撃実行処理 - タイプ別VFX生成と効果音再生
        /// </summary>
        public void Attack()
        {
            Vector3 posAdj = new Vector3();

            switch (type)
            {
                case TowerInfo.TowerInfoID.EnumTowerNightmare:
                    if (audioManager && audioManager.enabledSE)
                    {
                        audioManager.PlayAudio("se_Lighting");
                    }
                    posAdj = NIGHTMARE_ATTACK_POSITION_OFFSET;
                    if (IsAtMaxLevel() == false)
                    {
                        GainExp(ExpPerAttack * NIGHTMARE_EXP_MULTIPLIER * (stageManager.GetCurrIsland() + 1));
                    }
                    break;
                case TowerInfo.TowerInfoID.EnumTowerSoulEater:
                    if (audioManager && audioManager.enabledSE)
                    {
                        audioManager.PlayAudio("se_Snail");
                    }
                    posAdj = SOULEATER_ATTACK_POSITION_OFFSET;
                    atkEntityPos = transform.position;
                    if (IsAtMaxLevel() == false)
                        GainExp(ExpPerAttack * SOULEATER_EXP_MULTIPLIER * (stageManager.GetCurrIsland() + 1));
                    break;
                case TowerInfo.TowerInfoID.EnumTowerTerrorBringer:
                    if (audioManager && audioManager.enabledSE)
                    {
                        audioManager.PlayAudio("se_Shot");
                    }
                    posAdj = TERRORBRINGER_ATTACK_POSITION_OFFSET;
                    if (IsAtMaxLevel() == false)
                        GainExp(ExpPerAttack * TERRORBRINGER_EXP_MULTIPLIER * (stageManager.GetCurrIsland() + 1));
                    break;
                case TowerInfo.TowerInfoID.EnumTowerUsurper:
                    if (audioManager && audioManager.enabledSE)
                    {
                        audioManager.PlayAudio("se_Flame");
                    }
                    posAdj = USURPER_ATTACK_POSITION_OFFSET;
                    atkEntityPos = transform.position;
                    if (IsAtMaxLevel() == false)
                        GainExp(ExpPerAttack * USURPER_EXP_MULTIPLIER * (stageManager.GetCurrIsland() + 1));
                    break;
            }
            SpawnAttackProjectile(posAdj);
            atkCounter = attr.WaitTime;
            TriggerAttackAnimation();
        }

        /// <summary>
        /// タワーの破壊処理 - マップ更新、ECS統合、VFX清理
        /// </summary>
        public void Destroy()
        {
            UpdateMapOnDestroy();
            CleanupECSComponents();
            CleanupVFXComponents();
            StartCoroutine(EndAnim());
        }

        /// <summary>
        /// 破壊時のマップ状態更新
        /// </summary>
        private void UpdateMapOnDestroy()
        {
            _filledMapGenerator.UpdatePillarStatus(pillar, 0);
        }

        /// <summary>
        /// ECSコンポーネントの清理
        /// </summary>
        private void CleanupECSComponents()
        {
            _entityManager.RemoveComponent(_towerSpawner.Entities[this._entityID], typeof(PlayerTag));
        }

        /// <summary>
        /// VFXコンポーネントの清理
        /// </summary>
        private void CleanupVFXComponents()
        {
            foreach (GameObject i in AtkVFX)
            {
                AtkVFX.Remove(i);
                Destroy(i);
            }
        }

        /// <summary>
        /// 新規タワーの初期化処理 - ECSエンティティと連携してタワー設定を構築
        /// </summary>
        /// <param name="entityID">ECSエンティティID</param>
        /// <param name="towerSpawner">タワー生成管理クラス</param>
        /// <param name="pillar">建設された柱のGameObject</param>
        /// <param name="LevelUpVFX">レベルアップVFXプレハブ</param>
        /// <param name="AuraVFX">オーラVFXプレハブ</param>
        /// <param name="type">タワータイプ</param>
        /// <param name="lv">初期レベル</param>
        /// <param name="rank">初期ランク</param>
        public void NewTower(int entityID, TowerSpawner towerSpawner, GameObject pillar,
            GameObject LevelUpVFX, GameObject AuraVFX, TowerInfo.TowerInfoID type,
            int lv = 1, int rank = 1)
        {
            InitializeTowerProperties(entityID, towerSpawner, pillar, type, rank);
            SetupVFXComponents(LevelUpVFX, AuraVFX);
            ConfigureTowerTypeVFX(type, rank);
            InitializeLevelAndExp(lv);
        }

        /// <summary>
        /// 管理クラス群の依存関係注入 - システム間連携の確立
        /// </summary>
        /// <param name="stageManager">ステージ管理</param>
        /// <param name="towerSpawner">タワー生成管理</param>
        /// <param name="audioManager">オーディオ管理</param>
        /// <param name="attackSpawner">攻撃生成管理</param>
        /// <param name="filledMapGenerator">マップ生成管理</param>
        /// <param name="resourceManager">リソース管理</param>
        /// <param name="upgradesManager">アップグレード管理</param>
        /// <param name="bonusChecker">ボーナス検証</param>
        /// <param name="debugManager">デバッグ管理</param>
        public void LinkingManagers(StageManager stageManager, TowerSpawner towerSpawner, AudioManager audioManager,
            AttackSpawner attackSpawner, FilledMapGenerator filledMapGenerator,
            ResourceManager resourceManager, UpgradesManager upgradesManager, BonusChecker bonusChecker = null, DebugManager debugManager = null)
        {
            this.stageManager = stageManager;
            this._towerSpawner = towerSpawner;
            this.audioManager = audioManager;
            this._attackSpawner = attackSpawner;
            this._filledMapGenerator = filledMapGenerator;
            this.resourceManager = resourceManager;
            this.upgradesManager = upgradesManager;
            this._debugManager = debugManager;
            this._bonusChecker = bonusChecker;
        }

        /// <summary>
        /// 経験値獲得処理 - 自動レベルアップとボーナスシステム連携
        /// </summary>
        /// <param name="exp">獲得経験値</param>
        public void GainExp(int exp)
        {
            this.exp += exp;
            ProcessAutoLevelUp();
        }

        /// <summary>
        /// タワーの次レベルアップに必要な経験値を取得
        /// </summary>
        /// <returns>必要経験値</returns>
        public int GetRequiredExp()
        {
            return EXP_BASE_MULTIPLIER * level * (1 + level) * rank;
        }

        /// <summary>
        /// タワーのレベルを指定した値だけ上昇させる
        /// </summary>
        /// <param name="chg">上昇させるレベル数（デフォルト: 1）</param>
        public void LevelUp(int chg = 1)
        {
            SetLevel(level + chg);
        }

        /// <summary>
        /// タワーのレベルを指定した値に設定し、属性を更新
        /// </summary>
        /// <param name="lv">設定するレベル</param>
        /// <summary>
        /// タワーのレベルを指定した値に設定し、属性を更新
        /// </summary>
        /// <param name="lv">設定するレベル</param>
        public void SetLevel(int lv)
        {
            level = lv;
            UpdateVFXForLevel();
            UpdateAttr();
            CheckMaxLevelRewards();
        }

        /// <summary>
        /// レベル変更時のVFX更新
        /// </summary>
        private void UpdateVFXForLevel()
        {
            if (_auraVFXComponent != null)
            {
                _auraVFXComponent.SetFloat("Spawn Rate", level * VFX_SPAWN_RATE_LEVEL_MULTIPLIER);
            }

            if (_lvupVFXComponent != null)
            {
                _lvupVFXComponent.SetFloat("SizeMultiplier",
                    (float)level / MaxLevel[rank - 1] * (float)level / MaxLevel[rank - 1]  * AURA_VFX_LEVEL_MULTIPLIER);
            }
        }

        /// <summary>
        /// 最大レベル到達時の報酬チェック
        /// </summary>
        private void CheckMaxLevelRewards()
        {
            if (level == MaxLevel[rank - 1])
                resourceManager.ChangeMaterial(MaxLevelBonus[rank - 1]);

            if (IsAtMaxLevel())
                exp = 0;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// フィールド初期化処理
        /// </summary>
        private void InitializeFields()
        {
            atkCounter = 0;
            _entityID = -1;
            AtkVFX = new List<GameObject>();
            _newlySpawned = true;
        }

        /// <summary>
        /// Unityコンポーネント初期化処理
        /// </summary>
        private void InitializeComponents()
        {
            _animator = GetComponent<Animator>();
            _boxCollider = GetComponent<BoxCollider>();
            audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// ECS統合初期化処理
        /// </summary>
        private void InitializeECSIntegration()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            defaultTarget = GameObject.FindGameObjectWithTag("DefaultTag");
        }

        /// <summary>
        /// 管理クラス参照の初期化
        /// </summary>
        private void InitializeManagerReferences()
        {
            _towerSpawner = FindObjectOfType<TowerSpawner>();
            audioManager = FindObjectOfType<AudioManager>();
            _attackSpawner = FindObjectOfType<AttackSpawner>();
            _filledMapGenerator = FindObjectOfType<FilledMapGenerator>();
            _debugManager = FindObjectOfType<DebugManager>();
        }

        /// <summary>
        /// アニメーション状態の初期化
        /// </summary>
        private void InitializeAnimationState()
        {
            _oriScale = transform.localScale;
            transform.localScale = new Vector3();
            StartCoroutine(StartAnim());
        }

        /// <summary>
        /// 攻撃可能かどうかの判定（ECS最適化版）
        /// </summary>
        /// <returns>攻撃可能な場合true</returns>
        private bool CanAttackOptimized()
        {
            // ECSエンティティシステムとの統合チェック
            return _towerSpawner != null &&
                   _entityID >= 0 &&
                   _entityID < _towerSpawner.hastargetArray.Length &&
                   _towerSpawner.hastargetArray[_entityID];
        }

        /// <summary>
        /// ターゲット処理と攻撃実行（ECS最適化版）
        /// </summary>
        private void ProcessTargetAndAttack()
        {
            // ゲーム状態とECSエンティティの統一チェック
            if (!IsGameActiveAndValidTarget()) return;

            var entity = _towerSpawner.Entities[_entityID];
            if (!_entityManager.HasComponent<Target>(entity)) return;

            Target target = _entityManager.GetComponentData<Target>(entity);
            if (target.targetHealth <= 0) return;

            // ターゲット方向計算と攻撃実行
            SetTargetDirectionAndAttack(target.targetPos);
        }

        /// <summary>
        /// ゲームアクティブ状態と有効なターゲットの確認
        /// </summary>
        /// <returns>有効な状態の場合true</returns>
        private bool IsGameActiveAndValidTarget()
        {
            return stageManager != null &&
                   stageManager.GetResult() == GAME_SUCCESS_RESULT &&
                   _entityManager.Exists(_towerSpawner.Entities[_entityID]);
        }

        /// <summary>
        /// ターゲット方向設定と攻撃実行
        /// </summary>
        /// <param name="targetPos">ターゲット位置</param>
        private void SetTargetDirectionAndAttack(Vector3 targetPos)
        {
            targetPos.y = transform.position.y;
            transform.forward = (targetPos - transform.position).normalized;
            atkEntityPos = targetPos;
            Attack();
        }

        /// <summary>
        /// 攻撃プロジェクタイル生成処理
        /// </summary>
        /// <param name="posAdj">位置調整値</param>
        private void SpawnAttackProjectile(Vector3 posAdj)
        {
            int[] entityID = _attackSpawner.Spawn((int)type, this.transform.position
                + this.transform.forward * posAdj.z + this.transform.up * posAdj.y, atkEntityPos, this.transform.localRotation,
                attr.AttackSpeed * transform.forward, attr.Damage, attr.AttackRadius,
                attr.AttackWaittime, attr.AttackLifetime);

            this.AtkVFX.Add(_attackSpawner.GameObjects[entityID[0]]);
            VisualEffect vfx = _attackSpawner.GameObjects[entityID[0]].GetComponent<VisualEffect>();
            ConfigureAttackVFX(vfx);
        }

        /// <summary>
        /// 攻撃VFX設定処理
        /// </summary>
        /// <param name="vfx">設定するVisualEffectコンポーネント</param>
        private void ConfigureAttackVFX(VisualEffect vfx)
        {
            switch (type)
            {
                case TowerInfo.TowerInfoID.EnumTowerNightmare:
                    vfx.SetVector3("TargetPos", _towerSpawner.targetArray[this._entityID]);
                    vfx.SetFloat("StarSize", rank * NIGHTMARE_STAR_SIZE_MULTIPLIER);
                    vfx.SetFloat("AuraSize", rank * NIGHTMARE_AURA_SIZE_MULTIPLIER);
                    break;
                case TowerInfo.TowerInfoID.EnumTowerSoulEater:
                    vfx.SetFloat("Spawn rate", rank * SOULEATER_SPAWN_RATE_MULTIPLIER);
                    break;
                case TowerInfo.TowerInfoID.EnumTowerTerrorBringer:
                    vfx.SetVector3("TargetPos", _towerSpawner.targetArray[this._entityID]);
                    vfx.SetFloat("SkullSize", rank * TERRORBRINGER_SKULL_SIZE_MULTIPLIER + TERRORBRINGER_SKULL_SIZE_BASE);
                    break;
                case TowerInfo.TowerInfoID.EnumTowerUsurper:
                    vfx.SetFloat("SizeMultiplier", rank * USURPER_SIZE_MULTIPLIER);
                    break;
            }
        }

        /// <summary>
        /// 攻撃アニメーション実行処理
        /// </summary>
        private void TriggerAttackAnimation()
        {
            _animator.SetTrigger("Detected");
            _animator.SetInteger("ActionID", DefaultStageInfos.Prng.Next(0, ActionSetNum - 1));
        }

        /// <summary>
        /// タワー属性のプロパティ初期化
        /// </summary>
        /// <param name="entityID">ECSエンティティID</param>
        /// <param name="towerSpawner">タワー生成管理</param>
        /// <param name="pillar">建設された柱</param>
        /// <param name="type">タワータイプ</param>
        /// <param name="rank">初期ランク</param>
        private void InitializeTowerProperties(int entityID, TowerSpawner towerSpawner, GameObject pillar, TowerInfo.TowerInfoID type, int rank)
        {
            this._towerSpawner = towerSpawner;
            this.type = type;
            this.rank = rank;
            this.pillar = pillar;
            this._entityID = entityID;
        }

        /// <summary>
        /// VFXコンポーネント設定処理
        /// </summary>
        /// <param name="LevelUpVFX">レベルアップVFXプレハブ</param>
        /// <param name="AuraVFX">オーラVFXプレハブ</param>
        private void SetupVFXComponents(GameObject LevelUpVFX, GameObject AuraVFX)
        {
            auraVFXPrefab = AuraVFX;
            lvupVFXPrefab = LevelUpVFX;
            this.auraVFX = GameObject.Instantiate(auraVFXPrefab, this.transform.position, Quaternion.Euler(90f, 0, 0));
            this.auraVFX.transform.parent = this.transform;
            this.auraVFX.transform.localScale = Vector3.one * AURA_VFX_DEFAULT_SCALE;
            this._auraVFXComponent = auraVFX.GetComponentInChildren<VisualEffect>();

            this._lvupVFXComponent = GetComponentInChildren<VisualEffect>();

            if (this._lvupVFXComponent == null)
            {
                GameObject lvupVFX = GameObject.Instantiate(lvupVFXPrefab, this.transform.position, Quaternion.identity);
                lvupVFX.transform.parent = transform;
                _lvupVFXComponent = lvupVFX.GetComponentInChildren<VisualEffect>();
            }
            else
            {
                this._lvupVFXComponent.gameObject.transform.position = this.transform.position;
            }
        }

        /// <summary>
        /// タワータイプ別VFX設定処理
        /// </summary>
        /// <param name="type">タワータイプ</param>
        /// <param name="rank">タワーランク</param>
        private void ConfigureTowerTypeVFX(TowerInfo.TowerInfoID type, int rank)
        {
            switch (type)
            {
                case TowerInfo.TowerInfoID.EnumTowerNightmare:
                    _lvupVFXComponent.SetVector4("MainColor", new Vector4(0.6f, 0.46f, 0.3f, 1));
                    break;
                case TowerInfo.TowerInfoID.EnumTowerSoulEater:
                    _lvupVFXComponent.SetVector4("MainColor", new Vector4(0, 0.4f, 0.1f, 1));
                    break;
                case TowerInfo.TowerInfoID.EnumTowerTerrorBringer:
                    _lvupVFXComponent.SetVector4("MainColor", new Vector4(0.5f, 0.8f, 0.9f, 1));
                    break;
                case TowerInfo.TowerInfoID.EnumTowerUsurper:
                    _lvupVFXComponent.SetVector4("MainColor", new Vector4(0.8f, 0, 0.1f, 1));
                    break;
            }
            _lvupVFXComponent.SetFloat("GlowGroundSize", rank);
        }

        /// <summary>
        /// レベルと経験値の初期化処理
        /// </summary>
        /// <param name="lv">初期レベル</param>
        private void InitializeLevelAndExp(int lv)
        {
            exp = 0;
            SetLevel(lv);
        }

        /// <summary>
        /// 自動レベルアップ処理
        /// </summary>
        private void ProcessAutoLevelUp()
        {
            int reqExp = GetRequiredExp();
            while (this.exp > reqExp)
            {
                this.exp -= reqExp;
                reqExp = GetRequiredExp();
                if (level < MaxLevel[rank - 1])
                {
                    LevelUp();
                    if (_bonusChecker)
                        _bonusChecker.TowerLevelChg = true;
                }
            }
        }

        /// <summary>
        /// タワー属性更新処理 - ランク/レベル/アップグレードによる性能向上適用
        /// </summary>
        private void UpdateAttr()
        {
            attr = TowerInfo.GetTowerInfo(type);

            ApplyRankAndLevelScaling();
            ApplyUpgradeBonus();
            UpdateECSComponents();
            UpdateVFXScale();
        }

        /// <summary>
        /// ランクとレベルによる性能スケーリング適用
        /// </summary>
        private void ApplyRankAndLevelScaling()
        {
            attr = new TowerAttr(attr.Radius * (1 + RANK_UPGRADE_RADIUS_BASE * rank + LEVEL_UPGRADE_RADIUS_BASE * level),
                attr.Damage * (2f * rank + 0.5f * level),
                attr.WaitTime * (1f - (0.1f * rank)),
                3f, attr.AttackWaittime,
                attr.AttackRadius, attr.AttackSpeed, attr.AttackLifetime);
        }

        /// <summary>
        /// アップグレードボーナス適用処理
        /// </summary>
        private void ApplyUpgradeBonus()
        {
            int upgradeLv = 0;
            float damageMultiplier = 1.0f;
            float waitTimeMultiplier = 1.0f;

            switch (type)
            {
                case TowerInfo.TowerInfoID.EnumTowerNightmare:
                    upgradeLv = upgradesManager.GetLevel(UpgradesManager.StoreItems.ArmySoulEater);
                    damageMultiplier = 1 + (0.1f * upgradeLv * upgradeLv);
                    waitTimeMultiplier = 1 - (0.01f * upgradeLv * upgradeLv);
                    break;
                case TowerInfo.TowerInfoID.EnumTowerSoulEater:
                    upgradeLv = upgradesManager.GetLevel(UpgradesManager.StoreItems.ArmyNightmare);
                    damageMultiplier = 1 + (0.1f * upgradeLv * upgradeLv);
                    waitTimeMultiplier = 1 - (0.01f * upgradeLv * upgradeLv);
                    break;
                case TowerInfo.TowerInfoID.EnumTowerTerrorBringer:
                    upgradeLv = upgradesManager.GetLevel(UpgradesManager.StoreItems.ArmyTerrorBringer);
                    damageMultiplier = 1 + (0.2f * upgradeLv * upgradeLv);
                    waitTimeMultiplier = 1 - (0.005f * upgradeLv * upgradeLv);
                    break;
                case TowerInfo.TowerInfoID.EnumTowerUsurper:
                    upgradeLv = upgradesManager.GetLevel(UpgradesManager.StoreItems.ArmyUsurper);
                    damageMultiplier = 1 + (0.2f * upgradeLv * upgradeLv);
                    waitTimeMultiplier = 1 - (0.005f * upgradeLv * upgradeLv);
                    break;
            }

            // Create new TowerAttr with updated values
            attr = new TowerAttr(attr.Radius,
                attr.Damage * damageMultiplier,
                attr.WaitTime * waitTimeMultiplier,
                attr.Lifetime, attr.AttackWaittime,
                attr.AttackRadius, attr.AttackSpeed, attr.AttackLifetime);
        }

        /// <summary>
        /// ECSコンポーネント更新処理（バッチ更新最適化版）
        /// </summary>
        private void UpdateECSComponents()
        {
            // ECSエンティティの存在確認とバッチ更新でパフォーマンス最適化
            if (_entityManager.Exists(_towerSpawner.Entities[this._entityID]))
            {
                var entity = _towerSpawner.Entities[this._entityID];

                // バッチ処理で統一更新
                _entityManager.SetComponentData(entity, new Radius { Value = attr.Radius });
                _entityManager.SetComponentData(entity, new Damage { Value = attr.Damage });
                _entityManager.SetComponentData(entity, new WaitingTime { Value = attr.WaitTime });
            }
        }

        /// <summary>
        /// VFXスケール更新処理
        /// </summary>
        private void UpdateVFXScale()
        {
            if (_lvupVFXComponent != null)
            {
                this._lvupVFXComponent.transform.localScale = Vector3.one * AURA_VFX_DEFAULT_SCALE;
            }
        }

        /// <summary>
        /// タワーがレベルアップ可能かどうかを判定
        /// </summary>
        /// <returns>レベルアップ可能な場合true</returns>
        public bool CanLevelUp()
        {
            return level == MaxLevel[rank - 1];
        }

        /// <summary>
        /// タワーが現在のランクでの最大レベルに達しているかどうかを判定
        /// </summary>
        /// <returns>最大レベルに達している場合true</returns>
        public bool IsAtMaxLevel()
        {
            return level == MaxLevel[rank - 1];
        }

        /// <summary>
        /// タワーが最大ランクに達しているかどうかを判定
        /// </summary>
        /// <returns>最大ランクに達している場合true</returns>
        public bool IsAtMaxRank()
        {
            return rank == MaxLevel.Length;
        }

        /// <summary>
        /// タワー破壊アニメーション処理
        /// </summary>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator EndAnim()
        {
            if (_boxCollider) _boxCollider.enabled = false;

            yield return StartCoroutine(ShrinkTowerAnimation());
            FinalizeDestruction();
        }

        /// <summary>
        /// タワー縮小アニメーション
        /// </summary>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator ShrinkTowerAnimation()
        {
            float timeCounter = 0;
            float spd = transform.localScale.x / TowerDestroyTime;
            while (timeCounter < TowerDestroyTime)
            {
                float delta = Time.deltaTime;
                timeCounter += delta;
                transform.localScale = new Vector3(transform.localScale.x - spd * delta,
                    transform.localScale.y - spd * delta, transform.localScale.z - spd * delta);
                yield return null;
            }
            transform.localScale = Vector3.zero;
        }

        /// <summary>
        /// 破壊処理の最終化
        /// </summary>
        private void FinalizeDestruction()
        {
            if (auraVFX)
                Destroy(auraVFX);
            if (_lvupVFXComponent)
                _lvupVFXComponent.enabled = false;
            this.gameObject.SetActive(false);
            Destroy(this.gameObject);
        }

        /// <summary>
        /// タワー出現アニメーション処理
        /// </summary>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator StartAnim()
        {
            yield return StartCoroutine(GrowTowerAnimation());
            FinalizeTowerSpawn();
        }

        /// <summary>
        /// タワー成長アニメーション
        /// </summary>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator GrowTowerAnimation()
        {
            float timeCounter = 0;
            float animationTime = TowerDestroyTime * 0.1f;
            float spd = _oriScale.x / animationTime;

            while (timeCounter < animationTime)
            {
                float delta = Time.deltaTime;
                timeCounter += delta;
                transform.localScale = new Vector3(transform.localScale.x + spd * delta,
                    transform.localScale.y + spd * delta, transform.localScale.z + spd * delta);
                yield return null;
            }
        }

        /// <summary>
        /// タワー出現処理の最終化
        /// </summary>
        private void FinalizeTowerSpawn()
        {
            transform.localScale = _oriScale;
            _boxCollider.enabled = true;
        }

        #endregion
    }
}