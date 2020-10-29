using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Tower1 : MonoBehaviour
{
    //private readonly int[] MaxLevel = { 10, 30, 50, 70 };
    private readonly int[] MaxLevel = { 1, 1, 1, 1 };

    public TowerAttr attr;
    public int Level;
    public int rank;
    public TowerInfo.TowerInfoID type;

    private float AttackCounter;

    private GameObject CurrTarget;
    private List<GameObject> AtkVFX;
    private GameObject AuraVFX;
    private GameObject LevelUpVFX;

    private GameObject AtkVFXPrefab;
    private GameObject AuraVFXPrefab;
    private GameObject LevelUpVFXPrefab;

    private EnemyManager enemyManager;
    private AudioManager audioManager;
    public GameObject pillar;

    private AudioSource audio;
    //Testing
    //GameObject testobj;

    private Animator animator;

    private void Start()
    {
        enemyManager = FindObjectOfType<EnemyManager>();
        audioManager = FindObjectOfType<AudioManager>();
        //testobj = GameObject.FindGameObjectWithTag("DebugTag");

        AttackCounter = 0;
        AtkVFX = new List<GameObject>();
        animator = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (AttackCounter > 0) AttackCounter -= Time.deltaTime;

        if (CurrTarget == null) CurrTarget = detectEnemy();
        if (CurrTarget != null && AttackCounter <= 0) Attack();
    }

    public GameObject detectEnemy()
    {
        GameObject nearestMonster = null;
        float dist = float.MaxValue;
        foreach (GameObject i in enemyManager.allAliveMonsters)
        {
            float tempDist = (i.transform.position - this.transform.position).sqrMagnitude;
            if (tempDist > attr.areaSq) continue;
            if (tempDist < dist)
            {
                dist = tempDist;
                nearestMonster = i;
            }
        }

        //if ((testobj.transform.position - this.transform.position).sqrMagnitude <= attr.areaSq)
        //{
        //    nearestMonster = testobj;
        //}

        return nearestMonster;
    }

    public void Attack()
    {
        if ((CurrTarget.transform.position - this.transform.position).sqrMagnitude > attr.areaSq)
        {
            CurrTarget = null;
            return;
        }

        float posAdj = 0;
        switch (type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                audio.PlayOneShot(audioManager.GetAudio("se_Lighting"));
                posAdj = 0.2f; break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                audio.PlayOneShot(audioManager.GetAudio("se_Snail"));
                posAdj = -0.2f; break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                audio.PlayOneShot(audioManager.GetAudio("se_Shot"));
                posAdj = 0.0f; break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                audio.PlayOneShot(audioManager.GetAudio("se_MagicFire"));
                posAdj = 1f; break;
        }

        this.AtkVFX.Add(GameObject.Instantiate(AtkVFXPrefab, this.transform.position
          + this.transform.forward * posAdj,
           this.transform.rotation));

        if (type == TowerInfo.TowerInfoID.Enum_TowerNightmare || type == TowerInfo.TowerInfoID.Enum_TowerTerrorBringer)
            this.AtkVFX[AtkVFX.Count - 1].GetComponent<VisualEffect>().SetVector3("TargetPos", CurrTarget.transform.position);

        this.transform.localEulerAngles = new Vector3(0,
            (90f + Mathf.Rad2Deg * Mathf.Atan2(this.transform.position.z - CurrTarget.transform.position.z, CurrTarget.transform.position.x - this.transform.position.x)), 0);
        EnemyAI enm = CurrTarget.GetComponent<EnemyAI>();
        if (enm) CurrTarget.GetComponent<EnemyAI>().Damaged(attr.damage);

        StartCoroutine(WaitToKillVFX(this.AtkVFX[AtkVFX.Count - 1], 500, 0));

        AttackCounter = attr.frameWait;
        animator.SetTrigger("Detected");
        animator.SetInteger("ActionID", UnityEngine.Random.Range(0, 2));
    }

    public void Destroy()
    {
        GameObject.FindObjectOfType<FilledMapGenerator>().UpdatePillarStatus(pillar, 0);
        foreach (GameObject i in AtkVFX)
        {
            AtkVFX.Remove(i);
            Destroy(i);
        }
        if (AuraVFX)
            Destroy(AuraVFX);
        if (LevelUpVFX)
            Destroy(LevelUpVFX);
        Destroy(this.gameObject, 2);
    }

    public void newTower(GameObject pillar, GameObject AtkVFX, GameObject LevelUpVFX, GameObject AuraVFX, TowerInfo.TowerInfoID type, int lv = 1, int rank = 1)
    {
        this.type = type;
        this.Level = lv;
        this.rank = rank;
        this.pillar = pillar;
        AtkVFXPrefab = AtkVFX;
        AuraVFXPrefab = AuraVFX;
        LevelUpVFXPrefab = LevelUpVFX;
        //this.AtkVFX=GameObject.Instantiate(AtkVFX, this.transform.position
        //    + new Vector3(0, (type == TowerInfo.TowerInfoID.Enum_TowerNightmare || type == TowerInfo.TowerInfoID.Enum_TowerUsurper) ? 1.55f : 0
        //    , (type == TowerInfo.TowerInfoID.Enum_TowerNightmare || type == TowerInfo.TowerInfoID.Enum_TowerUsurper)?8:0), Quaternion.identity,this.transform);
        this.AuraVFX = GameObject.Instantiate(AuraVFXPrefab, this.transform.position, Quaternion.Euler(90f, 0, 0));
        this.LevelUpVFX = GameObject.Instantiate(LevelUpVFXPrefab, this.transform.position, Quaternion.identity);

        switch (type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                this.LevelUpVFX.GetComponent<VisualEffect>().SetVector4("MainColor", new Vector4(1, 1, 0, 1));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                this.LevelUpVFX.GetComponent<VisualEffect>().SetVector4("MainColor", new Vector4(0, 1, 0, 1));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                this.LevelUpVFX.GetComponent<VisualEffect>().SetVector4("MainColor", new Vector4(0, 0, 1, 1));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                this.LevelUpVFX.GetComponent<VisualEffect>().SetVector4("MainColor", new Vector4(1, 0, 0, 1));
                break;
        }

        UpdateAttr();
    }

    public void LevelUp(int chg = 1)
    {
        SetLevel(Level + chg);
    }

    public void SetLevel(int lv)
    {
        Level = lv;
        this.LevelUpVFX.GetComponent<VisualEffect>().SetFloat("SpawnRate", Level);
        UpdateAttr();
    }

    private void UpdateAttr()
    {
        attr = TowerInfo.GetTowerInfo(type);

        attr = new TowerAttr(attr.areaSq * (0.2f * rank + 0.005f * Level),
            attr.damage * (1f * rank + 0.05f * Level),
            attr.frameWait * (1f - (0.1f * rank)));

        switch (type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                attr.areaSq = attr.areaSq
                    * (1 + (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army1)));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                attr.areaSq = attr.areaSq
                      * (1 + (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army2)));

                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                attr.damage = attr.damage
                    * (1 + (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army3)));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                attr.frameWait = attr.frameWait
                    * (1 - (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army4)));
                break;
        }
    }

    public bool CheckMaxLevel()
    {
        return Level == MaxLevel[rank - 1];
    }

    private IEnumerator WaitToKillVFX(GameObject targetVFX, int waittime, int killtime)
    {
        int frame = waittime;
        while (frame-- > 0)
        {
            yield return new WaitForSeconds(0f);
        }
        targetVFX.GetComponent<VisualEffect>().Stop();
        Destroy(targetVFX, killtime);
    }
}