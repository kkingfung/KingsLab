using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.Units;
using RandomTowerDefense.Info;

namespace RandomTowerDefense.Managers.System
{
    /// <summary>
    /// スキル（魔法）システムの管理とアクティベーションを処理
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        #region Constants
        // スキル倍率定数
        private const float METEOR_DAMAGE_MULTIPLIER = 1.5f;
        private const float BLIZZARD_RADIUS_MULTIPLIER = 0.5f;
        private const float BLIZZARD_DAMAGE_MULTIPLIER = 0.3f;
        private const float PETRIFICATION_BUFF_MULTIPLIER = 0.2f;
        private const float MINIONS_CYCLE_MULTIPLIER = 0.5f;
        private const float MINIONS_DAMAGE_MULTIPLIER = 1.0f;

        // スキル ID 定数
        private const int METEOR_SKILL_ID = 0;
        private const int BLIZZARD_SKILL_ID = 1;
        private const int PETRIFICATION_SKILL_ID = 2;
        private const int MINIONS_SKILL_ID = 3;

        // 初期化定数
        private const int INITIAL_SKILL_LEVEL = 0;
        private const int SKILL_LEVEL_INCREMENT = 1;
        private const float INITIAL_MAX_ACTIVE_TIME = 1.0f;
        private const float INITIAL_CURR_ACTIVE_TIME = 0.0f;

        // エフェクト・VFX定数
        private const float VFX_CONSTANT_MULTIPLIER = 10.0f;
        private const float COROUTINE_WAIT_TIME = 0.0f;
        #endregion

        [Header("Skill Settings")]
        /// <summary>炎フィールドスキルオーラプレハブ</summary>
        public GameObject FireFieldSkAura;
        /// <summary>ブリザードフィールドスキルオーラプレハブ</summary>
        public GameObject BlizzardFieldSkAura;

        /// <summary>プレイヤー管理クラスの参照</summary>
        public PlayerManager playerManager;
        /// <summary>敵生成管理クラスの参照</summary>
        public EnemySpawner enemySpawner;

        /// <summary>アップグレード管理クラスの参照</summary>
        public UpgradesManager upgradesManager;
        /// <summary>メインカメラの参照</summary>
        public Camera MainCamera;

        //private GameObject Skill;

        private Dictionary<UpgradesManager.StoreItems, int> SkillUpgrader;

        //Total Cast Number * Constant for each Magic 
        private readonly int[] SkillRequirement = { 5, 15, 30, 50, 75, 100, 130, 170, 210, 250 };
        private readonly int[] SkillExp = { 5, 10, 15, 19, 23, 26, 29, 31, 34, 35 };
        private readonly int ExpPerActivation = 5;

        /// <summary>スキル生成管理クラスの参照</summary>
        public SkillSpawner skillSpawner;

        /// <summary>スキル最大アクティブ時間</summary>
        public float maxActiveTime;
        /// <summary>スキル現在アクティブ時間</summary>
        public float currActiveTime;

        /// <summary>スキルアクティブ状態表示スライダーリスト</summary>
        public List<Slider> SkillActivenessSlider;

        private VisualEffect SkillAuraSnow;
        private VisualEffect SkillAuraFire;
        private Skill skillScript;
        void Start()
        {
            //playerManager = FindObjectOfType<PlayerManager>();
            //enemySpawner = FindObjectOfType<EnemySpawner>();
            //skillSpawner = FindObjectOfType<SkillSpawner>();
            SkillUpgrader = new Dictionary<UpgradesManager.StoreItems, int>();
            SkillUpgrader.Add(UpgradesManager.StoreItems.MagicMeteor, INITIAL_SKILL_LEVEL);
            SkillUpgrader.Add(UpgradesManager.StoreItems.MagicBlizzard, INITIAL_SKILL_LEVEL);
            SkillUpgrader.Add(UpgradesManager.StoreItems.MagicMinions, INITIAL_SKILL_LEVEL);
            SkillUpgrader.Add(UpgradesManager.StoreItems.MagicPetrification, INITIAL_SKILL_LEVEL);
            maxActiveTime = INITIAL_MAX_ACTIVE_TIME;
            currActiveTime = INITIAL_CURR_ACTIVE_TIME;

            SkillAuraSnow = null;
            SkillAuraFire = null;
        }

        private void Update()
        {
            foreach (Slider i in SkillActivenessSlider)
            {
                i.gameObject.SetActive(currActiveTime != 0);
            }
            if (currActiveTime != 0)
            {
                foreach (Slider i in SkillActivenessSlider)
                {
                    i.value = currActiveTime / maxActiveTime;
                }
            }
        }

        /// <summary>
        /// スキル経験値獲得処理 - 自動レベルアップシステム連携
        /// </summary>
        /// <param name="itemID">スキルアイテムID</param>
        /// <param name="exp">獲得経験値</param>
        public void GainExp(UpgradesManager.StoreItems itemID, int exp)
        {
            SkillUpgrader[itemID] += exp;
            SkillUpgrade(itemID);
        }

        /// <summary>
        /// スキルアップグレード処理 - レベルアップ条件チェックと実行
        /// </summary>
        /// <param name="itemID">アップグレードするスキルID</param>
        void SkillUpgrade(UpgradesManager.StoreItems itemID)
        {
            int currExp = SkillUpgrader[itemID];
            if (currExp > SkillRequirement[upgradesManager.GetLevel(itemID)])
            {
                if (upgradesManager.UpgradeLevel(itemID, SKILL_LEVEL_INCREMENT))
                    SkillUpgrader[itemID] = INITIAL_SKILL_LEVEL;
            }
        }

        /// <summary>
        /// メテオスキル発動 - 指定位置に連続隕石攻撃
        /// </summary>
        /// <param name="hitPos">攻撃目標位置</param>
        public void MeteorSkill(Vector3 hitPos)
        {
            if (SkillAuraFire == null)
            {
                GameObject SkillAura = Instantiate(FireFieldSkAura, hitPos, Quaternion.identity);
                SkillAura.transform.parent = this.transform;
                SkillAuraFire = SkillAura.GetComponent<VisualEffect>();
            }
            else
            {
                SkillAuraFire.enabled = true;
            }

            SkillAttr attr = SkillInfo.GetSkillInfo("SkillMeteor");
            attr.Damage = attr.Damage * (1 + upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicMeteor) * upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicMeteor) * METEOR_DAMAGE_MULTIPLIER);
            GainExp(UpgradesManager.StoreItems.MagicMeteor, ExpPerActivation);
            StartCoroutine(MeteorSkillCoroutine(attr));
        }
        /// <summary>
        /// ブリザードスキル発動 - 指定位置に範囲スロー効果
        /// </summary>
        /// <param name="hitPos">スキル中心位置</param>
        public void BlizzardSkill(Vector3 hitPos)
        {
            if (SkillAuraSnow == null)
            {
                GameObject SkillAura = Instantiate(BlizzardFieldSkAura, hitPos, Quaternion.identity);
                SkillAura.transform.parent = this.transform;
                SkillAuraSnow = SkillAura.GetComponent<VisualEffect>();
            }
            else
            {
                SkillAuraSnow.enabled = true;
            }

            SkillAttr attr = SkillInfo.GetSkillInfo("SkillBlizzard");
            attr.Radius = attr.Radius * (1 + upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicBlizzard) * upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicBlizzard) * BLIZZARD_RADIUS_MULTIPLIER);
            //attr.Damage = attr.Damage * (1 + upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicBlizzard) * upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicBlizzard) * 0.3f);
            GainExp(UpgradesManager.StoreItems.MagicBlizzard, ExpPerActivation);

            StartCoroutine(BlizzardSkillCoroutine(attr));
        }
        /// <summary>
        /// 石化スキル発動 - 指定位置に範囲石化効果
        /// </summary>
        /// <param name="hitPos">スキル中心位置</param>
        public void PetrificationSkill(Vector3 hitPos)
        {
            SkillAttr attr = SkillInfo.GetSkillInfo("SkillPetrification");
            attr.DebuffTime = attr.DebuffTime * (1 + upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicPetrification) * upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicPetrification) * PETRIFICATION_BUFF_MULTIPLIER);
            GainExp(UpgradesManager.StoreItems.MagicPetrification, ExpPerActivation);

            StartCoroutine(PetrificationCoroutine(attr));
        }
        /// <summary>
        /// ミニオンスキル発動 - カメラ位置から敵に向けてミニオン召喚
        /// </summary>
        /// <param name="hitPos">ターゲット位置</param>
        public void MinionsSkill(Vector3 hitPos)
        {
            SkillAttr attr = SkillInfo.GetSkillInfo("SkillMinions");
            attr.CycleTime = attr.CycleTime * (1 + upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicMinions) * upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicMinions) * MINIONS_CYCLE_MULTIPLIER);
            attr.Damage = attr.Damage * (1 + upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicMinions) * upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicMinions) * MINIONS_DAMAGE_MULTIPLIER);
            GainExp(UpgradesManager.StoreItems.MagicMinions, ExpPerActivation);

            StartCoroutine(MinionsSkillCoroutine(attr));
        }

        /// <summary>
        /// スキル終了処理 - アクティブ状態をリセット
        /// </summary>
        public void SkillEnd()
        {
            playerManager.isSkillActive = false;
        }

        /// <summary>
        /// メテオスキルコルーチン - 連続隕石スポーン処理
        /// </summary>
        /// <param name="attr">スキル属性</param>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator MeteorSkillCoroutine(SkillAttr attr)
        {
            float frame = attr.LifeTime + Time.time;

            float frameToNext = Time.time;

            maxActiveTime = attr.LifeTime;
            currActiveTime = maxActiveTime;

            while (frame - Time.time > 0)
            {
                if (Time.time - frameToNext > attr.CycleTime)
                {
                    float3 pos = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                    int[] entityID = skillSpawner.Spawn(METEOR_SKILL_ID, pos,
                        pos, new float3(),
                        attr.Damage, attr.Radius, attr.WaitTime, attr.LifeTime, attr.WaitTime);
                    skillScript = skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>();
                    skillScript.Init(skillSpawner, UpgradesManager.StoreItems.MagicMeteor, attr, entityID[0]);
                    frameToNext = Time.time;
                }

                currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);

                yield return new WaitForSeconds(COROUTINE_WAIT_TIME);
            }
            skillScript = null;
            currActiveTime = INITIAL_CURR_ACTIVE_TIME;
            GainExp(UpgradesManager.StoreItems.MagicMeteor,
                SkillExp[upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicMeteor)]);
            SkillAuraFire.enabled = false;
            SkillEnd();
        }

        /// <summary>
        /// ブリザードスキルコルーチン - 範囲スロー効果処理
        /// </summary>
        /// <param name="attr">スキル属性</param>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator BlizzardSkillCoroutine(SkillAttr attr)
        {
            float frame = attr.LifeTime + Time.time;
            maxActiveTime = attr.LifeTime;
            currActiveTime = maxActiveTime;
            float3 pos = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
            int[] entityID = skillSpawner.Spawn(BLIZZARD_SKILL_ID, pos, pos, new float3(),
                attr.Damage, attr.Radius, attr.WaitTime, attr.LifeTime, attr.WaitTime,
                attr.SlowRate, attr.DebuffTime);

            skillScript = skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>();
            skillScript.Init(skillSpawner, UpgradesManager.StoreItems.MagicBlizzard, attr, entityID[0]);
            skillScript.SetConstantForVFX(attr.WaitTime);

            while (frame - Time.time > 0)
            {
                currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);
                yield return new WaitForSeconds(COROUTINE_WAIT_TIME);
            }

            skillScript = null;
            currActiveTime = INITIAL_CURR_ACTIVE_TIME;
            GainExp(UpgradesManager.StoreItems.MagicBlizzard,
                SkillExp[upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicBlizzard)]);
            SkillAuraSnow.enabled = false;
            if (skillSpawner != null)
                skillSpawner.GameObjects[entityID[0]].SetActive(false);
            SkillEnd();
        }

        /// <summary>
        /// 石化スキルコルーチン - 範囲石化効果処理
        /// </summary>
        /// <param name="attr">スキル属性</param>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator PetrificationCoroutine(SkillAttr attr)
        {
            float frame = attr.LifeTime + Time.time;
            float record = Time.time;
            float frameToNext = Time.time;
            maxActiveTime = attr.LifeTime;
            currActiveTime = maxActiveTime;
            float3 pos = playerManager.RaycastTest(LayerMask.GetMask("Arena"));

            while (frame - Time.time > 0)
            {
                if (Time.time - frameToNext > attr.CycleTime)
                {
                    int[] entityID = skillSpawner.Spawn(PETRIFICATION_SKILL_ID, pos, pos, new float3(),
                        attr.Damage, attr.Radius, attr.WaitTime, attr.LifeTime, attr.WaitTime,
                        attr.SlowRate, attr.DebuffTime);
                    skillScript = skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>();
                    skillScript.Init(skillSpawner, UpgradesManager.StoreItems.MagicPetrification, attr, entityID[0]);
                    skillScript.SetConstantForVFX((Time.time - record) / attr.LifeTime * VFX_CONSTANT_MULTIPLIER);

                    frameToNext = Time.time;
                }
                currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);
                yield return new WaitForSeconds(COROUTINE_WAIT_TIME);
            }
            skillScript = null;
            currActiveTime = INITIAL_CURR_ACTIVE_TIME;
            GainExp(UpgradesManager.StoreItems.MagicPetrification,
                SkillExp[upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicPetrification)]);
            SkillEnd();
        }

        /// <summary>
        /// ミニオンスキルコルーチン - 連続ミニオン召喚処理
        /// </summary>
        /// <param name="attr">スキル属性</param>
        /// <returns>コルーチンイテレータ</returns>
        private IEnumerator MinionsSkillCoroutine(SkillAttr attr)
        {
            float frame = attr.LifeTime + Time.time;
            float frameToNext = Time.time;
            maxActiveTime = attr.LifeTime;
            currActiveTime = maxActiveTime;

            while (frame - Time.time > 0)
            {
                if (Time.time - frameToNext > attr.CycleTime)
                {
                    if (enemySpawner && enemySpawner.AllAliveMonstersList().Count > 0)
                    {
                        float3 pos = playerManager.RaycastTest(LayerMask.GetMask("Arena"));

                        int[] entityID = skillSpawner.Spawn(MINIONS_SKILL_ID,
                            Camera.main.transform.position, pos, new float3(),
                            attr.Damage, attr.Radius, attr.WaitTime, attr.LifeTime, attr.WaitTime);
                        skillScript = skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>();
                        skillScript.Init(skillSpawner, UpgradesManager.StoreItems.MagicMinions, attr, entityID[0]);
                    }
                    frameToNext = Time.time;
                }
                currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);
                yield return new WaitForSeconds(COROUTINE_WAIT_TIME);
            }
            skillScript = null;
            currActiveTime = INITIAL_CURR_ACTIVE_TIME;
            GainExp(UpgradesManager.StoreItems.MagicMinions,
                SkillExp[upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicMinions)]);
            SkillEnd();
        }
    }
}
