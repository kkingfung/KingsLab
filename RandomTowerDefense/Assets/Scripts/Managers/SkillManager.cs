using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SkillManager : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject FireFieldSkAura;
    public GameObject BlizzardFieldSkAura;

    private PlayerManager playerManager;
    private EnemySpawner enemySpawner;

    public Camera MainCamera;

    //private GameObject Skill;
    private GameObject SkillAura;

    private Dictionary<Upgrades.StoreItems, int> SkillUpgrader;

    //Total Cast Number * Constant for each Magic 
    private readonly int[] SkillRequirement = { 5, 15, 30, 50, 75, 100, 130, 170, 210, 250 };
    private readonly int[] SkillExp = { 5, 10, 15, 19, 23, 26, 29, 31, 34, 35 };

    private SkillSpawner skillSpawner;

    public float maxActiveTime;
    public float currActiveTime;

    public List<Slider> SkillActivenessSlider;

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        skillSpawner = FindObjectOfType<SkillSpawner>();
        SkillUpgrader = new Dictionary<Upgrades.StoreItems, int>();
        SkillUpgrader.Add(Upgrades.StoreItems.MagicMeteor, 0);
        SkillUpgrader.Add(Upgrades.StoreItems.MagicBlizzard, 0);
        SkillUpgrader.Add(Upgrades.StoreItems.MagicMinions, 0);
        SkillUpgrader.Add(Upgrades.StoreItems.MagicPetrification, 0);
        maxActiveTime = 1;
        currActiveTime = 0;
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

    public void GainExp(Upgrades.StoreItems itemID,int exp) {
        SkillUpgrader[itemID] += exp;
        SkillUpgrade(itemID);
    }

    void SkillUpgrade(Upgrades.StoreItems itemID) {
        int currExp = SkillUpgrader[itemID];
        if (currExp > SkillRequirement[Upgrades.GetLevel(itemID)]) {
            if (Upgrades.AddSkillLevel(itemID, 1))
                SkillUpgrader[itemID] = 0;
        }
    }

    public GameObject MeteorSkill(Vector3 hitPos)
    {
        SkillAura = Instantiate(FireFieldSkAura, hitPos, Quaternion.identity);

        SkillAttr attr = SkillInfo.GetSkillInfo("SkillMeteor");
       // attr.area = attr.area;
        attr.damage = attr.damage * (1+Upgrades.GetLevel(Upgrades.StoreItems.MagicMeteor) *0.2f);
        //attr.cycleTime = attr.cycleTime;
        GainExp(Upgrades.StoreItems.MagicMeteor, 5);

        StartCoroutine(MeteorSkillCoroutine(attr));
        return SkillAura;
    }
    public GameObject BlizzardSkill(Vector3 hitPos)
    {
        SkillAura = Instantiate(BlizzardFieldSkAura, hitPos, Quaternion.identity);

        SkillAttr attr = SkillInfo.GetSkillInfo("SkillBlizzard");
        attr.radius = attr.radius * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard) * 0.2f);
        //attr.damage = attr.damage;
        //attr.cycleTime = attr.cycleTime;
        GainExp(Upgrades.StoreItems.MagicBlizzard, 5);

        StartCoroutine(BlizzardSkillCoroutine(attr));
        return SkillAura;
    }
    public GameObject PetrificationSkill(Vector3 hitPos)
    { 
        SkillAttr attr = SkillInfo.GetSkillInfo("SkillPetrification");
        //attr.area = attr.area;
        //attr.damage = attr.damage;
        attr.cycleTime = attr.cycleTime * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicPetrification) * 0.2f);
        GainExp(Upgrades.StoreItems.MagicPetrification, 5);

        StartCoroutine(PetrificationCoroutine(attr));
        return null;
    }
    public GameObject MinionsSkill(Vector3 hitPos)
    {
        SkillAttr attr = SkillInfo.GetSkillInfo("SkillMinions");
        //attr.area = attr.area;
        //attr.damage = attr.damage;
        attr.cycleTime = attr.cycleTime * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicMinions) * 0.2f);
        GainExp(Upgrades.StoreItems.MagicMinions, 5);

        StartCoroutine(MinionsSkillCoroutine(attr));
        return null;
    }

    public void SkillEnd() {
        playerManager.isSkillActive = false;
    }

    private IEnumerator MeteorSkillCoroutine(SkillAttr attr)
    {
        float frame = attr.lifeTime + Time.time;

        float frameToNext = Time.time;

        maxActiveTime = attr.lifeTime;
        currActiveTime = maxActiveTime;

        while (frame-Time.time > 0)
        {
            if (Time.time-frameToNext > attr.cycleTime) {
                int[] entityID = skillSpawner.Spawn(0,this.transform.position, new float3(),
                    attr.damage,attr.radius,attr.waitTime,attr.lifeTime,attr.waitTime);
                skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>()
                    .Init(Upgrades.StoreItems.MagicMeteor, attr,entityID[0]);

                frameToNext = Time.time;
            }

            currActiveTime =Mathf.Max(0,currActiveTime-Time.deltaTime);
            yield return new WaitForSeconds(0f);
        }

        currActiveTime = 0;
        GainExp(Upgrades.StoreItems.MagicMeteor,
            SkillExp[Upgrades.GetLevel(Upgrades.StoreItems.MagicMeteor)]);
        Destroy(SkillAura);
        SkillEnd();
    }

    private IEnumerator BlizzardSkillCoroutine(SkillAttr attr)
    {
        float frame = attr.lifeTime + Time.time;
        maxActiveTime = attr.lifeTime;
        currActiveTime = maxActiveTime;

        int[] entityID = skillSpawner.Spawn(1, this.transform.position, new float3(),
            attr.damage, attr.radius, attr.waitTime, attr.lifeTime, attr.waitTime,
            attr.slowRate, attr.buffTime);
        skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>()
            .Init(Upgrades.StoreItems.MagicBlizzard, attr, entityID[0]);
        skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>()
            .SetConstantForVFX(attr.waitTime);

        while (frame - Time.time > 0)
        {
            currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);
            yield return new WaitForSeconds(0f);
        }

        currActiveTime = 0;
        GainExp(Upgrades.StoreItems.MagicBlizzard,
            SkillExp[Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard)]);
        Destroy(SkillAura);
        SkillEnd();
    }

    private IEnumerator PetrificationCoroutine(SkillAttr attr)
    {
        float frame = attr.lifeTime + Time.time;
        float record = Time.time;
        float frameToNext = Time.time;
        maxActiveTime = attr.lifeTime;
        currActiveTime = maxActiveTime;

        while (frame - Time.time > 0)
        {
            if (Time.time - frameToNext > attr.cycleTime)
            {
                int[] entityID = skillSpawner.Spawn(2, this.transform.position, new float3(),
                    attr.damage, attr.radius, attr.waitTime, attr.lifeTime, attr.waitTime,
                    attr.slowRate, attr.buffTime);
                skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>()
                    .Init(Upgrades.StoreItems.MagicPetrification, attr, entityID[0]);
                skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>()
                    .SetConstantForVFX((Time.time - record) / attr.lifeTime * 10);

                frameToNext = Time.time;
            }
            currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);
            yield return new WaitForSeconds(0f);
        }

        currActiveTime = 0;
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
                    int[] entityID = skillSpawner.Spawn(3, this.transform.position, new float3(),
                        attr.damage, attr.radius, attr.waitTime, attr.lifeTime, attr.waitTime);

                    skillSpawner.GameObjects[entityID[0]].GetComponent<Skill>()
                        .Init(Upgrades.StoreItems.MagicMinions, attr, entityID[0]);
                }
                frameToNext = Time.time;
            }
            currActiveTime = Mathf.Max(0, currActiveTime - Time.deltaTime);
            yield return new WaitForSeconds(0f);
        }

        currActiveTime = 0;
        GainExp(Upgrades.StoreItems.MagicMinions,
            SkillExp[Upgrades.GetLevel(Upgrades.StoreItems.MagicMinions)]);
        SkillEnd();
    }
}
