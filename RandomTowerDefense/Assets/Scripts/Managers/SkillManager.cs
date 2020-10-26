using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject MeteorSkPrefab;
    public GameObject BlizzardSkPrefab;
    public GameObject PetrificationSkPrefab;
    public GameObject SummonSkPrefab;

    public GameObject FireFieldSkAura;
    public GameObject BlizzardFieldSkAura;

    private PlayerManager playerManager;

    public Camera MainCamera;

    private GameObject Skill;
    private GameObject SkillAura;

    private Dictionary<Upgrades.StoreItems, int> SkillUpgrader;
    private readonly int[] SkillRequirement= { 5,15,30,50,75,100,130,170,210,250};

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
    }

    public void GainExp(Upgrades.StoreItems itemID,int exp) {
        SkillUpgrader[itemID] += exp;
    }

    void SkillUpgrade(Upgrades.StoreItems itemID) {
        int currExp = SkillUpgrader[itemID];
        if (currExp > SkillRequirement[Upgrades.GetLevel(itemID)]) {
            Upgrades.AddLevel(itemID, 1);
            currExp = 0;
        }
    }

    public GameObject MeteorSkill(Vector3 hitPos)
    {
        SkillAura = Instantiate(FireFieldSkAura, hitPos, Quaternion.identity);

        SkillAttr attr = SkillInfo.GetSkillInfo("SkillMeteor");
       // attr.area = attr.area;
        attr.damage = attr.damage * (1+Upgrades.GetLevel(Upgrades.StoreItems.MagicMeteor) *0.2f);
        //attr.cycleTime = attr.cycleTime;

        StartCoroutine(MeteorSkillCoroutine(attr));
        return SkillAura;
    }
    public GameObject BlizzardSkill(Vector3 hitPos)
    {
        SkillAura = Instantiate(BlizzardFieldSkAura, hitPos, Quaternion.identity);

        SkillAttr attr = SkillInfo.GetSkillInfo("SkillBlizzard");
        attr.area = attr.area * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard) * 0.2f);
        //attr.damage = attr.damage;
        //attr.cycleTime = attr.cycleTime;

        StartCoroutine(BlizzardSkillCoroutine(attr));
        return SkillAura;
    }
    public GameObject Petrification(Vector3 hitPos)
    {
        GameObject SkillPointer = Instantiate(PetrificationSkPrefab, hitPos, Quaternion.identity);

        SkillAttr attr = SkillInfo.GetSkillInfo("SkillPetrification");
        //attr.area = attr.area;
        //attr.damage = attr.damage;
        attr.cycleTime = (int)(attr.cycleTime * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicPetrification) * 0.2f));

        StartCoroutine(PetrificationCoroutine(attr));
        return SkillPointer;
    }
    public GameObject SummonSkill(Vector3 hitPos)
    {
        GameObject SkillPointer = Instantiate(SummonSkPrefab, MainCamera.transform.position, Quaternion.identity);

        SkillAttr attr = SkillInfo.GetSkillInfo("SkillMinions");
        //attr.area = attr.area;
        //attr.damage = attr.damage;
        attr.cycleTime = (int)(attr.cycleTime * (1 + Upgrades.GetLevel(Upgrades.StoreItems.MagicSummon) * 0.2f));

        StartCoroutine(SummonSkillCoroutine(attr));
        return SkillPointer;
    }

    public void SkillEnd() {
        playerManager.isSkillActive = false;
    }

    private IEnumerator MeteorSkillCoroutine(SkillAttr attr)
    {
        int frame = attr.activeTime;

        int frameWait = attr.frameWait;
        while (frameWait-- > 0)
        {
            yield return new WaitForSeconds(0f);
        }

        int frameToNext = 0;
        while (frame-- > 0)
        {
            if (frameToNext++ > attr.cycleTime) {
                GameObject skillObj = Instantiate(MeteorSkPrefab);
                skillObj.GetComponent<Skill>().init(Upgrades.StoreItems.MagicMeteor, attr);
                frameToNext = 0;
            }
            yield return new WaitForSeconds(0f);
        }

        Destroy(SkillAura);
        SkillEnd();
    }

    private IEnumerator BlizzardSkillCoroutine(SkillAttr attr)
    {
        int frame = attr.activeTime;

        int frameWait = attr.frameWait;
        while (frameWait-- > 0)
        {
            yield return new WaitForSeconds(0f);
        }

        GameObject skillObj = Instantiate(BlizzardSkPrefab);
        skillObj.GetComponent<Skill>().init(Upgrades.StoreItems.MagicBlizzard, attr);

        while (frame-- > 0)
        {
            yield return new WaitForSeconds(0f);
        }

        Destroy(SkillAura);
        SkillEnd();
    }

    private IEnumerator PetrificationCoroutine(SkillAttr attr)
    {
        int frame = attr.activeTime;

        int frameToNext = 0;
        while (frame-- > 0)
        {
            if (frameToNext++ > attr.cycleTime)
            {
                GameObject skillObj = Instantiate(PetrificationSkPrefab);
                skillObj.GetComponent<Skill>().init(Upgrades.StoreItems.MagicPetrification, attr);
                frameToNext = 0;
            }
            yield return new WaitForSeconds(0f);
        }
        SkillEnd();
    }

    private IEnumerator SummonSkillCoroutine(SkillAttr attr)
    {
        int frame = attr.activeTime;
        int frameToNext = 0;
        while (frame-- > 0)
        {
            if (frameToNext++ > attr.cycleTime)
            {
                GameObject skillObj = Instantiate(SummonSkPrefab);
                skillObj.GetComponent<Skill>().init(Upgrades.StoreItems.MagicSummon, attr);
                frameToNext = 0;
            }
            yield return new WaitForSeconds(0f);
        }
        SkillEnd();
    }
}
