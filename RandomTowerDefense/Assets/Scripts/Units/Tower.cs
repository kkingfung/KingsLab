﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;
using Unity.Entities;

public class Tower : MonoBehaviour
{
    private readonly int[] MaxLevel = { 10, 30, 50, 70 };
    //private readonly int[] MaxLevel = { 1, 1, 1, 1 };
    private readonly float TowerDestroyTime = 2;
    private readonly int ActionSetNum = 3;
    private readonly int ExpPerAttack = 5;

    public TowerAttr attr;
    public int level;
    public int rank;
    public int exp;

    public TowerInfo.TowerInfoID type;

    private float atkCounter;
    private Vector3 atkEntityPos;

    //private List<GameObject> AtkVFX;
    private GameObject auraVFX;
    private GameObject lvupVFX;

    private GameObject auraVFXPrefab;
    private GameObject lvupVFXPrefab;

    private AudioManager audioManager;
    public GameObject pillar;

    private AudioSource audioSource;

    //Testing
    //GameObject defaultTarget;

    private Animator animator;
    private VisualEffect lvupVFXComponent;

    private int entityID;
    private TowerSpawner towerSpawner;
    private AttackSpawner attackSpawner;

    private EntityManager entityManager;
    private FilledMapGenerator filledMapGenerator;
    private void Awake()
    {
        atkCounter = 0;
        entityID = -1;
        //AtkVFX = new List<GameObject>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //defaultTarget = GameObject.FindGameObjectWithTag("DefaultTag");
    }

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        if (towerSpawner == null) towerSpawner = FindObjectOfType<TowerSpawner>();
        attackSpawner = FindObjectOfType<AttackSpawner>();
        filledMapGenerator = FindObjectOfType<FilledMapGenerator>();
    }

    void Update()
    {
        if (atkCounter > 0) atkCounter -= Time.deltaTime;
        if (atkCounter <= 0 && towerSpawner.hastargetArray[entityID])
        {
            if (entityManager.HasComponent<Target>(towerSpawner.Entities[entityID])) {
                Target target = entityManager.GetComponentData<Target>(towerSpawner.Entities[entityID]);
                Vector3 targetPos = target.targetPos;
                targetPos.y = transform.position.y;
                transform.forward = (targetPos - transform.position).normalized;
                atkEntityPos = targetPos;
                Attack();
            }
        }
    }
    public void Attack()
    {
        float posAdj = 0;

        switch (type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_Lighting"));
                }
                posAdj = 0.2f;
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_Snail"));
                }
                posAdj = -0.2f;
                atkEntityPos = transform.position;
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_Shot"));
                }

                posAdj = 0.0f;
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_MagicFire"));
                }
                posAdj = 1f;
                atkEntityPos = transform.position;
                break;
        }
        int[] entityID=attackSpawner.Spawn((int)type, this.transform.position
          + this.transform.forward * posAdj, atkEntityPos, this.transform.localRotation,
          attr.attackSpd*transform.forward, attr.damage, attr.attackRadius,
          attr.attackWaittime, attr.attackLifetime);
        //this.AtkVFX.Add(attackSpawner.GameObjects[entityID[0]]);
        if (type == TowerInfo.TowerInfoID.Enum_TowerNightmare || type == TowerInfo.TowerInfoID.Enum_TowerTerrorBringer)
            attackSpawner.GameObjects[entityID[0]].GetComponent<VisualEffect>().SetVector3("TargetPos", towerSpawner.targetArray[this.entityID]);

        //StartCoroutine(WaitToKillVFX(this.AtkVFX[AtkVFX.Count - 1], 8, 0));

        atkCounter = attr.waitTime;
        GainExp(ExpPerAttack);
        animator.SetTrigger("Detected");
        animator.SetInteger("ActionID", UnityEngine.Random.Range(0, ActionSetNum-1));
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
        if (auraVFX)
            Destroy(auraVFX);
        if (lvupVFX)
            Destroy(lvupVFX);
        Destroy(this.gameObject, TowerDestroyTime);
    }

    public void newTower(int entityID,TowerSpawner towerSpawner,GameObject pillar, GameObject LevelUpVFX, GameObject AuraVFX, TowerInfo.TowerInfoID type, int lv = 1, int rank = 1)
    {
        this.towerSpawner = towerSpawner;
        this.type = type;
        this.level = lv;
        this.rank = rank;
        this.pillar = pillar;
        this.entityID = entityID;
        auraVFXPrefab = AuraVFX;
        lvupVFXPrefab = LevelUpVFX;
        this.auraVFX = GameObject.Instantiate(auraVFXPrefab, this.transform.position, Quaternion.Euler(90f, 0, 0));
        this.lvupVFX = GameObject.Instantiate(lvupVFXPrefab, this.transform.position, Quaternion.identity);
        lvupVFXComponent = this.lvupVFX.GetComponent<VisualEffect>();
        switch (type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                lvupVFXComponent.SetVector4("MainColor", new Vector4(1, 1, 0, 1));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                lvupVFXComponent.SetVector4("MainColor", new Vector4(0, 1, 0, 1));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                lvupVFXComponent.SetVector4("MainColor", new Vector4(0, 0, 1, 1));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                lvupVFXComponent.SetVector4("MainColor", new Vector4(1, 0, 0, 1));
                break;
        }

        exp = 0;
        UpdateAttr();
    }

    public void GainExp(int exp)
    {
        this.exp += exp;
        //Level Lv Formula
        if (this.exp > 25 * level * (1 + level))
        {
            LevelUp();
            this.exp = 0;
        }
    }

    public void LevelUp(int chg = 1)
    {
        SetLevel(level + chg);
    }

    public void SetLevel(int lv)
    {
        level = lv;
        lvupVFXComponent.SetFloat("SpawnRate", level);
        UpdateAttr();
    }

    private void UpdateAttr()
    {
        attr = TowerInfo.GetTowerInfo(type);

        //Update by rank/level with factors
        attr = new TowerAttr(attr.radius * (0.2f * rank + 0.005f * level),
            attr.damage * (1f * rank + 0.05f * level),
            attr.waitTime * (1f - (0.1f * rank)), attr.attackLifetime, attr.attackWaittime, attr.attackRadius,attr.attackSpd);

        switch (type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                attr.radius = attr.radius
                    * (1 + (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army1)));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                attr.radius = attr.radius
                      * (1 + (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army2)));

                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                attr.damage = attr.damage
                    * (1 + (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army3)));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                attr.waitTime = attr.waitTime
                    * (1 - (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army4)));
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

    }

    public bool CheckLevel()
    {
        return true;
        //return Level == MaxLevel[rank - 1];
    }

    private IEnumerator WaitToKillVFX(GameObject targetVFX, int waittime, int killtime)
    {
        float timer = waittime;
        while (timer-- > 0)
        {
            timer -= Time.deltaTime;
            yield return new WaitForSeconds(0f);
        }
        targetVFX.GetComponent<VisualEffect>().Stop();
        Destroy(targetVFX, killtime);
    }
}