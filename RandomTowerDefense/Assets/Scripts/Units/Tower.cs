using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;
using Unity.Entities;

public class Tower : MonoBehaviour
{
    //private readonly int[] MaxLevel = { 10, 30, 50, 70 };
    private readonly int[] MaxLevel = { 1, 1, 1, 1 };

    public TowerAttr attr;
    public int Level;
    public int rank;
    public TowerInfo.TowerInfoID type;

    private float AttackCounter;

    private List<GameObject> AtkVFX;
    private GameObject AuraVFX;
    private GameObject LevelUpVFX;

    private GameObject AuraVFXPrefab;
    private GameObject LevelUpVFXPrefab;

    private AudioManager audioManager;
    public GameObject pillar;

    private AudioSource audio;

    //Testing
    //GameObject defaultTarget;

    private Animator animator;

    private int entityID;
    private TowerSpawner towerSpawner;
    private AttackSpawner attackSpawner;
    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        towerSpawner = FindObjectOfType<TowerSpawner>();
        AttackCounter = 0;
        entityID = -1;
        AtkVFX = new List<GameObject>();
        animator = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();

        //defaultTarget = GameObject.FindGameObjectWithTag("DefaultTag");
    }

    void Update()
    {
        if (AttackCounter > 0) AttackCounter -= Time.deltaTime;
        if (AttackCounter <= 0 && towerSpawner.hastargetArray[entityID]) 
            Attack();
    }

    public void Attack()
    {
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
        int[] entityID=attackSpawner.Spawn((int)type, this.transform.position
          + this.transform.forward * posAdj, this.transform.rotation, attr.damage, attr.radius,
          attr.waitTime, 1.0f, 0.2f);

        this.AtkVFX.Add(attackSpawner.GameObjects[entityID[0]]);
        if (type == TowerInfo.TowerInfoID.Enum_TowerNightmare || type == TowerInfo.TowerInfoID.Enum_TowerTerrorBringer)
            this.AtkVFX[AtkVFX.Count - 1].GetComponent<VisualEffect>().SetVector3("TargetPos", towerSpawner.targetArray[this.entityID]);

        this.transform.localEulerAngles = new Vector3(0,
            (90f + Mathf.Rad2Deg * Mathf.Atan2(this.transform.position.z - towerSpawner.targetArray[this.entityID].z,
            towerSpawner.targetArray[this.entityID].x - this.transform.position.x)), 0);

        StartCoroutine(WaitToKillVFX(this.AtkVFX[AtkVFX.Count - 1], 8, 0));

        AttackCounter = attr.waitTime;
        animator.SetTrigger("Detected");
        animator.SetInteger("ActionID", UnityEngine.Random.Range(0, 2));
    }

    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public void Destroy()
    {
        //AlsoDestroy Entity
        GameObject.FindObjectOfType<FilledMapGenerator>().UpdatePillarStatus(pillar, 0);

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.SetComponentData(towerSpawner.Entities[this.entityID], new Lifetime
        {
            Value = -1
        });

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

    public void newTower(int entityID,GameObject pillar, GameObject LevelUpVFX, GameObject AuraVFX, TowerInfo.TowerInfoID type, int lv = 1, int rank = 1)
    {
        this.type = type;
        this.Level = lv;
        this.rank = rank;
        this.pillar = pillar;
        this.entityID = entityID;
        AuraVFXPrefab = AuraVFX;
        LevelUpVFXPrefab = LevelUpVFX;
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

        attr = new TowerAttr(attr.radius * (0.2f * rank + 0.005f * Level),
            attr.damage * (1f * rank + 0.05f * Level),
            attr.waitTime * (1f - (0.1f * rank)));

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

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

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

    public bool CheckMaxLevel()
    {
        return Level == MaxLevel[rank - 1];
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