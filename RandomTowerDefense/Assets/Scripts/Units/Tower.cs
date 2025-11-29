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

        //private List<GameObject> AtkVFX;
        private GameObject auraVFX = null;

        private GameObject auraVFXPrefab;
        private GameObject lvupVFXPrefab;
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

        #endregion

        private Animator animator;
        private VisualEffect auraVFXComponent;

        public VisualEffect lvupVFXComponent;

        private int entityID;
        private TowerSpawner towerSpawner;
        private AttackSpawner attackSpawner;

        private EntityManager entityManager;
        private FilledMapGenerator filledMapGenerator;
        private BonusChecker bonusChecker;

        private float3 oriScale;
        private bool newlySpawned;

        private DebugManager debugManager;
        private BoxCollider boxCollider;
        private void Awake()
        {
            atkCounter = 0;
            entityID = -1;
            //AtkVFX = new List<GameObject>();
            animator = GetComponent<Animator>();
            boxCollider = GetComponent<BoxCollider>();
            // audioSource = GetComponent<AudioSource>();

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //defaultTarget = GameObject.FindGameObjectWithTag("DefaultTag");
            newlySpawned = true;
        }

        private void Start()
        {
            //towerSpawner = FindObjectOfType<TowerSpawner>();
            //audioManager = FindObjectOfType<AudioManager>();
            //attackSpawner = FindObjectOfType<AttackSpawner>();
            //filledMapGenerator = FindObjectOfType<FilledMapGenerator>();
            //debugManager = FindObjectOfType<DebugManager>();

            oriScale = transform.localScale;
            transform.localScale = new Vector3();
            StartCoroutine(StartAnim());
        }

        private void OnEnable()
        {
            if (newlySpawned == false)
            {
                transform.localScale = new Vector3();
                StartCoroutine(StartAnim());
            }
        }

        private void OnDisable()
        {
            newlySpawned = false;
        }

        void Update()
        {
            if (atkCounter > 0)
            {
                atkCounter -= Time.deltaTime;
            }
            if (atkCounter <= 0 && towerSpawner.hastargetArray[entityID])
            {
                if (stageManager && stageManager.GetResult() == 0)
                {
                    if (entityManager.HasComponent<Target>(towerSpawner.Entities[entityID]))
                    {
                        Target target = entityManager.GetComponentData<Target>(towerSpawner.Entities[entityID]);
                        float targetHealth = target.targetHealth;
                        if (targetHealth > 0)
                        {
                            Vector3 targetPos = target.targetPos;
                            targetPos.y = transform.position.y;
                            transform.forward = (targetPos - transform.position).normalized;
                            atkEntityPos = targetPos;
                            Attack();
                        }
                    }
                }
            }
        }
        public GameObject debug;
        public void Attack()
        {
            Vector3 posAdj = new Vector3();

            switch (type)
            {
                case TowerInfo.TowerInfoID.Enum_TowerNightmare:
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
                case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                    if (audioManager && audioManager.enabledSE)
                    {
                        audioManager.PlayAudio("se_Snail");
                    }
                    posAdj = SOULEATER_ATTACK_POSITION_OFFSET;
                    atkEntityPos = transform.position;
                    if (IsAtMaxLevel() == false)
                        GainExp(ExpPerAttack * SOULEATER_EXP_MULTIPLIER * (stageManager.GetCurrIsland() + 1));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                    if (audioManager && audioManager.enabledSE)
                    {
                        audioManager.PlayAudio("se_Shot");
                    }
                    posAdj = TERRORBRINGER_ATTACK_POSITION_OFFSET;
                    if (IsAtMaxLevel() == false)
                        GainExp(ExpPerAttack * TERRORBRINGER_EXP_MULTIPLIER * (stageManager.GetCurrIsland() + 1));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerUsurper:
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
            int[] entityID = attackSpawner.Spawn((int)type, this.transform.position
              + this.transform.forward * posAdj.z + this.transform.up * posAdj.y, atkEntityPos, this.transform.localRotation,
              attr.attackSpd * transform.forward, attr.Damage, attr.attackRadius,
              attr.attackWaittime, attr.attackLifetime);

            //this.AtkVFX.Add(attackSpawner.GameObjects[entityID[0]]);
            VisualEffect vfx = attackSpawner.GameObjects[entityID[0]].GetComponent<VisualEffect>();

            switch (type)
            {
                case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                    //if (vfx.HasFloat("AuraSize") == false || vfx.HasVector3("TargetPos") == false || vfx.HasFloat("StarSize") == false)
                    //{
                    //    debug = vfx.gameObject;
                    //    Debug.Log(-1);
                    //}

                    vfx.SetVector3("TargetPos", towerSpawner.targetArray[this.entityID]);
                    vfx.SetFloat("StarSize", rank * NIGHTMARE_STAR_SIZE_MULTIPLIER);
                    vfx.SetFloat("AuraSize", rank * NIGHTMARE_AURA_SIZE_MULTIPLIER);
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                    //if (vfx.HasFloat("Spawn rate") == false)
                    //{
                    //    debug = vfx.gameObject;
                    //    Debug.Log(-1);
                    //}

                    vfx.SetFloat("Spawn rate", rank * SOULEATER_SPAWN_RATE_MULTIPLIER);
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                    //if (vfx.HasFloat("SkullSize") == false || vfx.HasVector3("TargetPos") == false)
                    //{
                    //    debug = vfx.gameObject;
                    //    Debug.Log(-1);
                    //}
                    vfx.SetVector3("TargetPos", towerSpawner.targetArray[this.entityID]);
                    vfx.SetFloat("SkullSize", rank * TERRORBRINGER_SKULL_SIZE_MULTIPLIER + TERRORBRINGER_SKULL_SIZE_BASE);
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                    //if (vfx.HasFloat("SizeMultiplier") == false)
                    //{
                    //    debug = vfx.gameObject;
                    //    Debug.Log(-1);
                    //}
                    vfx.SetFloat("SizeMultiplier", rank * USURPER_SIZE_MULTIPLIER);
                    break;
            }

            atkCounter = attr.WaitTime;
            animator.SetTrigger("Detected");
            animator.SetInteger("ActionID", StageInfoList.prng.Next(0, ActionSetNum - 1));
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        public void Destroy()
        {
            //AlsoDestroy Entity
            filledMapGenerator.UpdatePillarStatus(pillar, 0);
            entityManager.RemoveComponent(towerSpawner.Entities[this.entityID], typeof(PlayerTag));

            //foreach (GameObject i in AtkVFX)
            //{
            //    AtkVFX.Remove(i);
            //    Destroy(i);
            //}

            StartCoroutine(EndAnim());
        }

        public void NewTower(int entityID, TowerSpawner towerSpawner, GameObject pillar,
            GameObject LevelUpVFX, GameObject AuraVFX, TowerInfo.TowerInfoID type,
            int lv = 1, int rank = 1)
        {
            this.towerSpawner = towerSpawner;
            this.type = type;
            this.rank = rank;
            this.pillar = pillar;
            this.entityID = entityID;
            auraVFXPrefab = AuraVFX;
            lvupVFXPrefab = LevelUpVFX;
            //this.auraVFX = GameObject.Instantiate(auraVFXPrefab, this.transform.position, Quaternion.Euler(90f, 0, 0));
            //this.auraVFX.transform.parent = this.transform;
            //this.auraVFX.transform.localScale = Vector3.one * 10f;
            //this.auraVFXComponent = auraVFX.GetComponentInChildren<VisualEffect>();

            //this.lvupVFXComponent = GetComponentInChildren<VisualEffect>();

            if (this.lvupVFXComponent == null)
            {
                GameObject lvupVFX = GameObject.Instantiate(lvupVFXPrefab, this.transform.position, Quaternion.identity);
                lvupVFX.transform.parent = transform;
                lvupVFXComponent = lvupVFX.GetComponentInChildren<VisualEffect>();
            }
            else
            {
                this.lvupVFXComponent.gameObject.transform.position = this.transform.position;
            }

            switch (type)
            {
                case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                    lvupVFXComponent.SetVector4("MainColor", new Vector4(0.6f, 0.46f, 0.3f, 1));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                    lvupVFXComponent.SetVector4("MainColor", new Vector4(0, 0.4f, 0.1f, 1));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                    lvupVFXComponent.SetVector4("MainColor", new Vector4(0.5f, 0.8f, 0.9f, 1));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                    lvupVFXComponent.SetVector4("MainColor", new Vector4(0.8f, 0, 0.1f, 1));
                    break;
            }
            lvupVFXComponent.SetFloat("GlowGroundSize", rank);
            exp = 0;
            SetLevel(lv);
        }

        public void LinkingManagers(StageManager stageManager, TowerSpawner towerSpawner, AudioManager audioManager,
            AttackSpawner attackSpawner, FilledMapGenerator filledMapGenerator,
            ResourceManager resourceManager, BonusChecker bonusChecker = null, DebugManager debugManager = null)
        {
            this.stageManager = stageManager;
            this.towerSpawner = towerSpawner;
            this.audioManager = audioManager;
            this.attackSpawner = attackSpawner;
            this.filledMapGenerator = filledMapGenerator;
            this.resourceManager = resourceManager;
            this.debugManager = debugManager;
            this.bonusChecker = bonusChecker;
        }

        public void GainExp(int exp)
        {
            this.exp += exp;
            int reqExp = GetRequiredExp();
            //Level Lv Formula
            while (this.exp > reqExp)
            {
                this.exp -= reqExp;
                reqExp = GetRequiredExp();
                if (level < MaxLevel[rank - 1])
                {
                    LevelUp();
                    if (bonusChecker)
                        bonusChecker.TowerLevelChg = true;
                }
            }
        }

        /// <summary>
        /// タワーの次レベルアップに必要な経験値を取得
        /// </summary>
        /// <returns>必要経験値</returns>
        public int GetRequiredExp()
        {
            return 25 * level * (1 + level) * rank;
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
        public void SetLevel(int lv)
        {
            level = lv;
            //auraVFXComponent.SetFloat("Spawn Rate", level * 5);
            lvupVFXComponent.SetFloat("SizeMultiplier",
                (float)level / MaxLevel[rank - 1] * (float)level / MaxLevel[rank - 1] * 5.0f);
            UpdateAttr();
            if (level == MaxLevel[rank - 1])
                resourceManager.ChangeMaterial(MaxLevelBonus[rank - 1]);

            if (IsAtMaxLevel())
                exp = 0;
        }

        private void UpdateAttr()
        {
            attr = TowerInfo.GetTowerInfo(type);

            //Update by rank/level with factors
            attr = new TowerAttr(attr.Radius * (1 + 0.02f * rank + 0.005f * level),
                attr.Damage * (2f * rank + 0.5f * level
                //+ ((debugManager != null) ? debugManager.towerrank_Damage * rank +
                //debugManager.towerlvl_Damage * level: 0)
                ), attr.WaitTime * (1f - (0.1f * rank)),
                3f, attr.attackWaittime,
                attr.attackRadius, attr.attackSpd, attr.attackLifetime);

            int upgradeLv = 0;
            switch (type)
            {
                case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                    upgradeLv = UpgradesManager.GetLevel(UpgradesManager.StoreItems.ArmySoulEater);
                    attr.Damage = attr.Damage
                       * (1 + (0.1f * upgradeLv * upgradeLv));
                    attr.WaitTime = attr.WaitTime
                       * (1 - (0.01f * upgradeLv * upgradeLv));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                    upgradeLv = UpgradesManager.GetLevel(UpgradesManager.StoreItems.ArmyNightmare);
                    attr.Damage = attr.Damage
                       * (1 + (0.1f * upgradeLv * upgradeLv));
                    attr.WaitTime = attr.WaitTime
                       * (1 - (0.01f * upgradeLv * upgradeLv));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                    upgradeLv = UpgradesManager.GetLevel(UpgradesManager.StoreItems.ArmyTerrorBringer);
                    attr.Damage = attr.Damage
                        * (1 + (0.2f * upgradeLv * upgradeLv));
                    attr.WaitTime = attr.WaitTime
                       * (1 - (0.005f * upgradeLv * upgradeLv));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                    upgradeLv = UpgradesManager.GetLevel(UpgradesManager.StoreItems.ArmyUsurper);
                    attr.Damage = attr.Damage
                        * (1 + (0.2f * upgradeLv * upgradeLv));
                    attr.WaitTime = attr.WaitTime
                       * (1 - (0.005f * upgradeLv * upgradeLv));
                    break;
            }

            entityManager.SetComponentData(towerSpawner.Entities[this.entityID], new Radius
            {
                Value = attr.Radius
            });
            entityManager.SetComponentData(towerSpawner.Entities[this.entityID], new Damage
            {
                Value = attr.Damage
            });
            entityManager.SetComponentData(towerSpawner.Entities[this.entityID], new WaitingTime
            {
                Value = attr.WaitTime
            });

            this.lvupVFXComponent.transform.localScale = Vector3.one * 10f;
        }

        /// <summary>
        /// タワーがレベルアップ可能かどうかを判定
        /// </summary>
        /// <returns>レベルアップ可能な場合true</returns>
        public bool CanLevelUp()
        {
            return true;
            //return level == MaxLevel[rank - 1];
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

        private IEnumerator EndAnim()
        {
            if (boxCollider) boxCollider.enabled = false;

            float timeCounter = 0;
            float spd = transform.localScale.x / (TowerDestroyTime);
            while (timeCounter < TowerDestroyTime)
            {
                float delta = Time.deltaTime;
                timeCounter += delta;
                transform.localScale = new Vector3(transform.localScale.x - spd * delta,
                    transform.localScale.y - spd * delta, transform.localScale.z - spd * delta);
                yield return new WaitForSeconds(0);
            }

            transform.localScale = new Vector3();

            if (auraVFX)
                Destroy(auraVFX);
            //if(lvupVFXComponent)
            //    lvupVFXComponent.enabled = false;
            this.gameObject.SetActive(false);
            //Destroy(this.gameObject);
            StopCoroutine(EndAnim());
        }

        private IEnumerator StartAnim()
        {
            float timeCounter = 0;
            float spd = oriScale.x / (TowerDestroyTime * 0.1f);
            while (timeCounter < TowerDestroyTime * 0.1f)
            {
                float delta = Time.deltaTime;
                timeCounter += delta;
                transform.localScale = new Vector3(transform.localScale.x + spd * delta,
                    transform.localScale.y + spd * delta, transform.localScale.z + spd * delta);
                yield return new WaitForSeconds(0);
            }

            transform.localScale = oriScale;
            boxCollider.enabled = true;
        }
    }
}