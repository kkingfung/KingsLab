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
        public GameObject FireFieldSkAura;
        public GameObject BlizzardFieldSkAura;

        public PlayerManager playerManager;
        public EnemySpawner enemySpawner;

        public Camera MainCamera;

        //private GameObject Skill;

        private Dictionary<Upgrades.StoreItems, int> SkillUpgrader;

        //Total Cast Number * Constant for each Magic 
        private readonly int[] SkillRequirement = { 5, 15, 30, 50, 75, 100, 130, 170, 210, 250 };
        private readonly int[] SkillExp = { 5, 10, 15, 19, 23, 26, 29, 31, 34, 35 };
        private readonly int ExpPerActivation = 5;

        public SkillSpawner skillSpawner;

        public float maxActiveTime;
        public float currActiveTime;

        public List<Slider> SkillActivenessSlider;

        private VisualEffect SkillAuraSnow;
        private VisualEffect SkillAuraFire;
        private Skill skillScript;
        void Start()
        {
            //playerManager = FindObjectOfType<PlayerManager>();
            //enemySpawner = FindObjectOfType<EnemySpawner>();
            //skillSpawner = FindObjectOfType<SkillSpawner>();
            SkillUpgrader = new Dictionary<Upgrades.StoreItems, int>();
            SkillUpgrader.Add(Upgrades.StoreItems.MagicMeteor, INITIAL_SKILL_LEVEL);
            SkillUpgrader.Add(Upgrades.StoreItems.MagicBlizzard, INITIAL_SKILL_LEVEL);
            SkillUpgrader.Add(Upgrades.StoreItems.MagicMinions, INITIAL_SKILL_LEVEL);
            SkillUpgrader.Add(Upgrades.StoreItems.MagicPetrification, INITIAL_SKILL_LEVEL);
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

        public void GainExp(Upgrades.StoreItems itemID, int exp)
        {
            SkillUpgrader[itemID] += exp;
            SkillUpgrade(itemID);
        }

        void SkillUpgrade(Upgrades.StoreItems itemID)
        {
            int currExp = SkillUpgrader[itemID];
            if (currExp > SkillRequirement[Upgrades.GetLevel(itemID)])
            {
                if (Upgrades.AddSkillLevel(itemID, SKILL_LEVEL_INCREMENT))
                    SkillUpgrader[itemID] = INITIAL_SKILL_LEVEL;
            }
        }

        public void MeteorSkill(Vector3 hitPos)
        {
            //if (SkillAuraFire == null)
            //{
            //    GameObject SkillAura = Instantiate(FireFieldSkAura, hitPos, Quaternion.identity);
            //    SkillAura.transform.parent = this.transform;
            //    SkillAuraFire = SkillAura.GetComponent<VisualEffect>();
            //}
            //else
            //{
            //    SkillAuraFire.enabled = true;
            //}

            SkillAttr attr = SkillInfo.GetSkillInfo("SkillMeteor");
            attr.damage = attr.damage * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicMeteor) * Upgrades.GetLevel(Upgrades.StoreItems.MagicMeteor) * METEOR_DAMAGE_MULTIPLIER);
            GainExp(Upgrades.StoreItems.MagicMeteor, ExpPerActivation);
            StartCoroutine(MeteorSkillCoroutine(attr));
        }
        public void BlizzardSkill(Vector3 hitPos)
        {
            //if (SkillAuraSnow == null)
            //{
            //    GameObject SkillAura = Instantiate(BlizzardFieldSkAura, hitPos, Quaternion.identity);
            //    SkillAura.transform.parent = this.transform;
            //    SkillAuraSnow = SkillAura.GetComponent<VisualEffect>();
            //}
            //else
            //{
            //    SkillAuraSnow.enabled = true;
            //}

            SkillAttr attr = SkillInfo.GetSkillInfo("SkillBlizzard");
            attr.radius = attr.radius * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard) * Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard) * BLIZZARD_RADIUS_MULTIPLIER);
            //attr.damage = attr.damage * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard) * Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard) * 0.3f);
            GainExp(Upgrades.StoreItems.MagicBlizzard, ExpPerActivation);

            StartCoroutine(BlizzardSkillCoroutine(attr));
        }
        public void PetrificationSkill(Vector3 hitPos)
        {
            SkillAttr attr = SkillInfo.GetSkillInfo("SkillPetrification");
            attr.buffTime = attr.buffTime * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicPetrification) * Upgrades.GetLevel(Upgrades.StoreItems.MagicPetrification) * PETRIFICATION_BUFF_MULTIPLIER);
            GainExp(Upgrades.StoreItems.MagicPetrification, ExpPerActivation);

            StartCoroutine(PetrificationCoroutine(attr));
        }
        public void MinionsSkill(Vector3 hitPos)
        {
            SkillAttr attr = SkillInfo.GetSkillInfo("SkillMinions");
            attr.cycleTime = attr.cycleTime * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicMinions) * Upgrades.GetLevel(Upgrades.StoreItems.MagicMinions) * MINIONS_CYCLE_MULTIPLIER);
            attr.damage = attr.damage * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicMinions) * Upgrades.GetLevel(Upgrades.StoreItems.MagicMinions) * MINIONS_DAMAGE_MULTIPLIER);
            GainExp(Upgrades.StoreItems.MagicMinions, ExpPerActivation);

            StartCoroutine(MinionsSkillCoroutine(attr));
        }

        public void SkillEnd()
        {
            playerManager.isSkillActive = false;
        }

        private IEnumerator MeteorSkillCoroutine(SkillAttr attr)
        {
            float frame = attr.lifeTime + Time.time;

            float frameToNext = Time.time;

            maxActiveTime = attr.lifeTime;
            currActiveTime = maxActiveTime;

            while (frame - Time.time > 0)
            {
                if (Time.time - frameToNext > attr.cycleTime)
                {
                    float3 pos = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                    int[] entityID = skillSpawner.Spawn(METEOR_SKILL_ID, pos,
                        pos, new float3(),
                        attr.damage, attr.radius, attr.waitTime, attr.lifeTime, attr.waitTime);
                    skillScript = skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>();
                    skillScript.Init(skillSpawner, Upgrades.StoreItems.MagicMeteor, attr, entityID[0]);
                    frameToNext = Time.time;
                }

                currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);

                yield return new WaitForSeconds(COROUTINE_WAIT_TIME);
            }
            skillScript = null;
            currActiveTime = INITIAL_CURR_ACTIVE_TIME;
            GainExp(Upgrades.StoreItems.MagicMeteor,
                SkillExp[Upgrades.GetLevel(Upgrades.StoreItems.MagicMeteor)]);
            SkillAuraFire.enabled = false;
            SkillEnd();
        }

        private IEnumerator BlizzardSkillCoroutine(SkillAttr attr)
        {
            float frame = attr.lifeTime + Time.time;
            maxActiveTime = attr.lifeTime;
            currActiveTime = maxActiveTime;
            float3 pos = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
            int[] entityID = skillSpawner.Spawn(BLIZZARD_SKILL_ID, pos, pos, new float3(),
                attr.damage, attr.radius, attr.waitTime, attr.lifeTime, attr.waitTime,
                attr.slowRate, attr.buffTime);

            skillScript = skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>();
            skillScript.Init(skillSpawner, Upgrades.StoreItems.MagicBlizzard, attr, entityID[0]);
            skillScript.SetConstantForVFX(attr.waitTime);

            while (frame - Time.time > 0)
            {
                currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);
                yield return new WaitForSeconds(COROUTINE_WAIT_TIME);
            }

            skillScript = null;
            currActiveTime = INITIAL_CURR_ACTIVE_TIME;
            GainExp(Upgrades.StoreItems.MagicBlizzard,
                SkillExp[Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard)]);
            SkillAuraSnow.enabled = false;
            skillSpawner.GameObjects[entityID[0]].SetActive(false);
            SkillEnd();
        }

        private IEnumerator PetrificationCoroutine(SkillAttr attr)
        {
            float frame = attr.lifeTime + Time.time;
            float record = Time.time;
            float frameToNext = Time.time;
            maxActiveTime = attr.lifeTime;
            currActiveTime = maxActiveTime;
            float3 pos = playerManager.RaycastTest(LayerMask.GetMask("Arena"));

            while (frame - Time.time > 0)
            {
                if (Time.time - frameToNext > attr.cycleTime)
                {
                    int[] entityID = skillSpawner.Spawn(PETRIFICATION_SKILL_ID, pos, pos, new float3(),
                        attr.damage, attr.radius, attr.waitTime, attr.lifeTime, attr.waitTime,
                        attr.slowRate, attr.buffTime);
                    skillScript = skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>();
                    skillScript.Init(skillSpawner, Upgrades.StoreItems.MagicPetrification, attr, entityID[0]);
                    skillScript.SetConstantForVFX((Time.time - record) / attr.lifeTime * VFX_CONSTANT_MULTIPLIER);

                    frameToNext = Time.time;
                }
                currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);
                yield return new WaitForSeconds(COROUTINE_WAIT_TIME);
            }
            skillScript = null;
            currActiveTime = INITIAL_CURR_ACTIVE_TIME;
            GainExp(Upgrades.StoreItems.MagicPetrification,
                SkillExp[Upgrades.GetLevel(Upgrades.StoreItems.MagicPetrification)]);
            SkillEnd();
        }

        private IEnumerator MinionsSkillCoroutine(SkillAttr attr)
        {
            float frame = attr.lifeTime + Time.time;
            float frameToNext = Time.time;
            maxActiveTime = attr.lifeTime;
            currActiveTime = maxActiveTime;

            while (frame - Time.time > 0)
            {
                if (Time.time - frameToNext > attr.cycleTime)
                {
                    if (enemySpawner && enemySpawner.AllAliveMonstersList().Count > 0)
                    {
                        float3 pos = playerManager.RaycastTest(LayerMask.GetMask("Arena"));

                        int[] entityID = skillSpawner.Spawn(MINIONS_SKILL_ID, Camera.main.transform.position, pos, new float3(),
                            attr.damage, attr.radius, attr.waitTime, attr.lifeTime, attr.waitTime);
                        skillScript = skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>();
                        skillScript.Init(skillSpawner, Upgrades.StoreItems.MagicMinions, attr, entityID[0]);
                    }
                    frameToNext = Time.time;
                }
                currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);
                yield return new WaitForSeconds(COROUTINE_WAIT_TIME);
            }
            skillScript = null;
            currActiveTime = INITIAL_CURR_ACTIVE_TIME;
            GainExp(Upgrades.StoreItems.MagicMinions,
                SkillExp[Upgrades.GetLevel(Upgrades.StoreItems.MagicMinions)]);
            SkillEnd();
        }
    }
}
