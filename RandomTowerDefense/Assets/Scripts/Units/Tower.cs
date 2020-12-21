﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;
using Unity.Entities;

public class Tower : MonoBehaviour
{
    private readonly int[] MaxLevel = { 5, 10, 20, 40 };
    private readonly int[] MaxLevelBonus = { 100, 300, 500, 1000 };
    //private readonly int[] MaxLevel = { 1, 1, 1, 1 };
    private readonly float TowerDestroyTime = 2;
    private readonly int ActionSetNum = 2;
    private readonly int ExpPerAttack = 10;

    public TowerAttr attr;
    public int level;
    public int rank;
    public int exp;

    public TowerInfo.TowerInfoID type;

    private float atkCounter;
    private Vector3 atkEntityPos;

    //private List<GameObject> AtkVFX;
    private GameObject auraVFX = null;

    private GameObject auraVFXPrefab;
    private GameObject lvupVFXPrefab;

    public AudioManager audioManager;
    public ResourceManager resourceManager;
    public StageManager stageManager;
    public GameObject pillar;

    //private AudioSource audioSource;

    //Testing
    //GameObject defaultTarget;

    private Animator animator;
    //private VisualEffect auraVFXComponent;
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

    private void OnEnable() {
        if (newlySpawned == false) {
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
        if (atkCounter > 0) atkCounter -= Time.deltaTime;
        if (atkCounter <= 0 && towerSpawner.hastargetArray[entityID])
        {
            if (stageManager && stageManager.GetResult() == 0)
            {
                if (entityManager.HasComponent<Target>(towerSpawner.Entities[entityID]))
                {
                    Target target = entityManager.GetComponentData<Target>(towerSpawner.Entities[entityID]);
                    Vector3 targetPos = target.targetPos;
                    targetPos.y = transform.position.y;
                    transform.forward = (targetPos - transform.position).normalized;
                    atkEntityPos = targetPos;
                    Attack();
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
                posAdj.z = 0.2f;
                if (CheckMaxLevel() == false)
                GainExp(ExpPerAttack * 3);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                if (audioManager && audioManager.enabledSE)
                {
                    audioManager.PlayAudio("se_Snail");
                }
                posAdj.z = -0.2f;
                atkEntityPos = transform.position;
                if (CheckMaxLevel() == false)
                    GainExp(ExpPerAttack * 2);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
               if (audioManager && audioManager.enabledSE)
               {
                   audioManager.PlayAudio("se_Shot");
               }
                posAdj.z = 0.0f;
                if (CheckMaxLevel() == false)
                    GainExp(ExpPerAttack*5);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
               if (audioManager && audioManager.enabledSE)
               {
                   audioManager.PlayAudio("se_Flame");
               }
                posAdj.z = 0.5f;
                posAdj.y = 0.15f;
                atkEntityPos = transform.position;
                if (CheckMaxLevel() == false)
                    GainExp(ExpPerAttack * 1);
                break;
        }
        int[] entityID=attackSpawner.Spawn((int)type, this.transform.position
          + this.transform.forward * posAdj.z + this.transform.up * posAdj.y, atkEntityPos, this.transform.localRotation,
          attr.attackSpd*transform.forward, attr.damage, attr.attackRadius,
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
                vfx.SetFloat("StarSize", rank * 10f);
                vfx.SetFloat("AuraSize", rank * 0.5f);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                //if (vfx.HasFloat("Spawn rate") == false)
                //{
                //    debug = vfx.gameObject;
                //    Debug.Log(-1);
                //}

                vfx.SetFloat("Spawn rate", rank * 1f);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                //if (vfx.HasFloat("SkullSize") == false || vfx.HasVector3("TargetPos") == false)
                //{
                //    debug = vfx.gameObject;
                //    Debug.Log(-1);
                //}
                vfx.SetVector3("TargetPos", towerSpawner.targetArray[this.entityID]);
                vfx.SetFloat("SkullSize", rank * 10f + 10f);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                //if (vfx.HasFloat("SizeMultiplier") == false)
                //{
                //    debug = vfx.gameObject;
                //    Debug.Log(-1);
                //}
                vfx.SetFloat("SizeMultiplier", rank * 0.1f);
                break;
        }

        atkCounter = attr.waitTime;
        animator.SetTrigger("Detected");
        animator.SetInteger("ActionID", StageInfo.prng.Next(0, ActionSetNum-1));
    }

    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public void Destroy()
    {
        //AlsoDestroy Entity
        filledMapGenerator.UpdatePillarStatus(pillar, 0);
        entityManager.RemoveComponent(towerSpawner.Entities[this.entityID],typeof(PlayerTag));

        //foreach (GameObject i in AtkVFX)
        //{
        //    AtkVFX.Remove(i);
        //    Destroy(i);
        //}

        StartCoroutine(EndAnim());
    }

    public void newTower(int entityID,TowerSpawner towerSpawner,GameObject pillar, 
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

    public void linkingManagers(StageManager stageManager,TowerSpawner towerSpawner, AudioManager audioManager, 
        AttackSpawner attackSpawner, FilledMapGenerator filledMapGenerator,
        ResourceManager resourceManager,BonusChecker bonusChecker=null,DebugManager debugManager=null)
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
        int reqExp = RequiredExp();
        //Level Lv Formula
        while (this.exp > reqExp)
        {
            this.exp -= reqExp;
            reqExp = RequiredExp();
            if (level < MaxLevel[rank - 1])
            {
                LevelUp();
                if (bonusChecker)
                    bonusChecker.TowerLevelChg = true;
            }
        }
    }

    public int RequiredExp() {
        return 25 * level * (1 + level) * rank;
    }

    public void LevelUp(int chg = 1)
    {
        SetLevel(level + chg);
    }

    public void SetLevel(int lv)
    {
        level = lv;
        //auraVFXComponent.SetFloat("Spawn Rate", level * 5);
        lvupVFXComponent.SetFloat("SizeMultiplier", 
            (float)level / MaxLevel[rank - 1] * (float)level/MaxLevel[rank - 1] * 5.0f);
        UpdateAttr();
        if (level == MaxLevel[rank - 1])
            resourceManager.ChangeMaterial(MaxLevelBonus[rank - 1]);

        if (CheckMaxLevel())
            exp = 0;
    }

    private void UpdateAttr()
    {
        attr = TowerInfo.GetTowerInfo(type);

        //Update by rank/level with factors
        attr = new TowerAttr(attr.radius * (1 + 0.02f * rank + 0.005f * level),
            attr.damage * (2f * rank + 0.5f * level
            //+ ((debugManager != null) ? debugManager.towerrank_Damage * rank +
            //debugManager.towerlvl_Damage * level: 0)
            ), attr.waitTime * (1f - (0.1f * rank)), 
            3f, attr.attackWaittime, 
            attr.attackRadius,attr.attackSpd, attr.attackLifetime);

        int upgradeLv = 0;
        switch (type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                upgradeLv = Upgrades.GetLevel(Upgrades.StoreItems.Army1);
                attr.damage = attr.damage
                   * (1 + (0.1f * upgradeLv * upgradeLv));
                attr.waitTime = attr.waitTime
                   * (1 - (0.01f * upgradeLv * upgradeLv));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                upgradeLv = Upgrades.GetLevel(Upgrades.StoreItems.Army2);
                attr.damage = attr.damage
                   * (1 + (0.1f * upgradeLv * upgradeLv));
                attr.waitTime = attr.waitTime
                   * (1 - (0.01f * upgradeLv * upgradeLv));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                upgradeLv = Upgrades.GetLevel(Upgrades.StoreItems.Army3);
                attr.damage = attr.damage
                    * (1 + (0.2f * upgradeLv * upgradeLv));
                attr.waitTime = attr.waitTime
                   * (1 - (0.005f * upgradeLv * upgradeLv));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                upgradeLv = Upgrades.GetLevel(Upgrades.StoreItems.Army4);
                attr.damage = attr.damage
                    * (1 + (0.2f * upgradeLv * upgradeLv));
                attr.waitTime = attr.waitTime
                   * (1 - (0.005f * upgradeLv * upgradeLv));
                break;
        }

        entityManager.SetComponentData(towerSpawner.Entities[this.entityID], new Radius
        {
            Value = attr.radius
        });
        entityManager.SetComponentData(towerSpawner.Entities[this.entityID], new Damage
        {
            Value = attr.damage
        });
        entityManager.SetComponentData(towerSpawner.Entities[this.entityID], new WaitingTime
        {
            Value = attr.waitTime
        });

        this.lvupVFXComponent.transform.localScale = Vector3.one * 10f;
    }

    public bool CheckLevel()
    {
        return true;
        //return level == MaxLevel[rank - 1];
    }

    public bool CheckMaxLevel()
    {
        return level == MaxLevel[rank - 1];
    }

    public bool CheckMaxRank()
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
        while (timeCounter< TowerDestroyTime * 0.1f) 
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