using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;
using Unity.Entities;

public class Tower : MonoBehaviour
{
    private readonly int[] MaxLevel = { 15, 30, 50, 99 };
    //private readonly int[] MaxLevel = { 1, 1, 1, 1 };
    private readonly float TowerDestroyTime = 2;
    private readonly int ActionSetNum = 2;
    private readonly int ExpPerAttack = 1;

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

    private float3 oriScale;
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

        oriScale = transform.localScale;
        transform.localScale = new Vector3();

        StartCoroutine(StartAnim());
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
        Vector3 posAdj = new Vector3();

        switch (type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_Lighting"));
                }
                posAdj.z = 0.2f;
                GainExp(ExpPerAttack * 5);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_Snail"));
                }
                posAdj.z = -0.2f;
                atkEntityPos = transform.position;
                GainExp(ExpPerAttack * 2);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_Shot"));
                }

                posAdj.z = 0.0f;
                GainExp(ExpPerAttack*10);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_MagicFire"));
                }
                posAdj.z = 0.5f;
                posAdj.y = 0.15f;
                atkEntityPos = transform.position;
                GainExp(ExpPerAttack);
                break;
        }
        int[] entityID=attackSpawner.Spawn((int)type, this.transform.position
          + this.transform.forward * posAdj.z + this.transform.up * posAdj.y, atkEntityPos, this.transform.localRotation,
          attr.attackSpd*transform.forward, attr.damage, attr.attackRadius,
          attr.attackWaittime, attr.attackLifetime);
        //this.AtkVFX.Add(attackSpawner.GameObjects[entityID[0]]);
        if (type == TowerInfo.TowerInfoID.Enum_TowerNightmare || type == TowerInfo.TowerInfoID.Enum_TowerTerrorBringer)
            attackSpawner.GameObjects[entityID[0]].GetComponent<VisualEffect>().SetVector3("TargetPos", towerSpawner.targetArray[this.entityID]);

        atkCounter = attr.waitTime;
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

        StartCoroutine(EndAnim());
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
        if (this.exp > RequiredExp())
        {
            LevelUp();
            this.exp = 0;
        }
    }

    public int RequiredExp() {
        return 25 * level * (1 + level);
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
        attr = new TowerAttr(attr.radius * (1 + 0.05f * rank + 0.005f * level),
            attr.damage * (1 + 1f * rank + 0.1f * level),
            attr.waitTime * (1f - (0.1f * rank)), attr.attackLifetime, attr.attackWaittime, attr.attackRadius,attr.attackSpd);

        switch (type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                attr.radius = attr.radius
                    * (1 + (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army1) * Upgrades.GetLevel(Upgrades.StoreItems.Army1)));
                attr.damage = attr.damage
                   * (1 + (0.02f * Upgrades.GetLevel(Upgrades.StoreItems.Army1) * Upgrades.GetLevel(Upgrades.StoreItems.Army1)));
                attr.waitTime = attr.waitTime
                   * (1 - (0.02f * Upgrades.GetLevel(Upgrades.StoreItems.Army1) * Upgrades.GetLevel(Upgrades.StoreItems.Army1)));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                attr.radius = attr.radius
                      * (1 + (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army2) * Upgrades.GetLevel(Upgrades.StoreItems.Army2)));
                attr.damage = attr.damage
                        * (1 + (0.02f * Upgrades.GetLevel(Upgrades.StoreItems.Army2) * Upgrades.GetLevel(Upgrades.StoreItems.Army2)));
                attr.waitTime = attr.waitTime
                   * (1 - (0.02f * Upgrades.GetLevel(Upgrades.StoreItems.Army2) * Upgrades.GetLevel(Upgrades.StoreItems.Army2)));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                attr.damage = attr.damage
                    * (1 + (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army3) * Upgrades.GetLevel(Upgrades.StoreItems.Army3)));
                attr.radius = attr.radius
                     * (1 + (0.02f * Upgrades.GetLevel(Upgrades.StoreItems.Army3) * Upgrades.GetLevel(Upgrades.StoreItems.Army3)));
                attr.waitTime = attr.waitTime
                   * (1 - (0.02f * Upgrades.GetLevel(Upgrades.StoreItems.Army3) * Upgrades.GetLevel(Upgrades.StoreItems.Army3)));
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                attr.waitTime = attr.waitTime
                    * (1 - (0.05f * Upgrades.GetLevel(Upgrades.StoreItems.Army4) * Upgrades.GetLevel(Upgrades.StoreItems.Army4)));
                attr.damage = attr.damage
                     * (1 + (0.02f * Upgrades.GetLevel(Upgrades.StoreItems.Army4) * Upgrades.GetLevel(Upgrades.StoreItems.Army4)));
                attr.radius = attr.radius
                   * (1 + (0.02f * Upgrades.GetLevel(Upgrades.StoreItems.Army4) * Upgrades.GetLevel(Upgrades.StoreItems.Army4)));
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
        if (lvupVFX)
            Destroy(lvupVFX);
        Destroy(this.gameObject);
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
    }
}