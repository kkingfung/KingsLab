using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI;
using Unity.Entities;

public class Enemy : MonoBehaviour
{
    private readonly int DamageCounterMax = 5;
    private readonly int ResizeFactor = 2;
    private readonly float ResizeScale = 0.5f;
    private readonly float EnemyDestroyTime = 1;

    private GameObject DieEffect;
    private GameObject DropEffect;

    private Vector3 oriScale;
    private Vector3 prevPos;
    private int DamagedCount = 0;
    private int money;
    private float HealthRecord;
   
    private float SlowRecord;
    private float PetrifyRecord;

    private int entityID=-1;
    private EnemySpawner enemySpawner;
    private ResourceManager resourceManager;

    private SkinnedMeshRenderer[] meshes;
    public Animator animator;
    public Slider HpBar;
    private RectTransform HpBarRot;

    private bool isDead;
    // Start is called before the first frame update
    private void Awake()
    {
        transform.localScale = transform.localScale * ResizeScale;
        oriScale = transform.localScale;
        if(enemySpawner==null) enemySpawner = FindObjectOfType<EnemySpawner>();
        resourceManager = FindObjectOfType<ResourceManager>();

        if (animator == null) 
            animator = GetComponent<Animator>();

        if (animator == null) {
            animator = GetComponentInChildren<Animator>();
        }

        HealthRecord = 1;
        if (HpBar == null)
            HpBar = GetComponentInChildren<Slider>();
        if (HpBar != null)
            HpBarRot = HpBar.gameObject.GetComponent<RectTransform>();
        prevPos = transform.position;
        isDead = false;

        meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDead) return;

        if (entityID >= 0 && HealthRecord>0) {
            float currHP = enemySpawner.healthArray[entityID];
            if (currHP != HealthRecord) {
                Damaged(currHP);
                HealthRecord = currHP;
                HpBar.value = currHP;
                HpBarRot.eulerAngles = new Vector3(0, 0, 0);
            }
      
            //CheckingStatus
            Petrified();
            Slowed();
        }

        if (DamagedCount > 0) 
        { 
            DamagedCount--;
            if(DamagedCount % ResizeFactor ==0)
                transform.localScale = oriScale;
            else
                transform.localScale = oriScale * ResizeScale;
        }

        if (transform.position != prevPos)
        {
            Vector3 direction = transform.position - prevPos;
            transform.forward = direction;
            prevPos = transform.position;
        }

    }

    public void Init(EnemySpawner enemySpawner,GameObject DieEffect, GameObject DropEffect,int entityID,int money)
    {
        this.enemySpawner = enemySpawner;
        this.DieEffect = DieEffect;
        this.DropEffect = DropEffect;
        this.entityID = entityID;
        this.money = money;
        HpBar.maxValue = enemySpawner.healthArray[entityID];
        HpBar.value = HpBar.maxValue;
    }

    public void Damaged(float currHP)
    {
        DamagedCount += DamageCounterMax;
        if (currHP <= 0) {
            isDead = true;
            resourceManager.ChangeMaterial(money);
            GameObject vfx = Instantiate(DropEffect, this.transform.position, Quaternion.identity);
            vfx.GetComponent<VisualEffect>().SetInt("SpawnCount", money);

            if (animator)
                animator.SetTrigger("Dead");
            Die();
        }
    }

    public void Die()
    {
        GameObject.Instantiate(DieEffect, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject, EnemyDestroyTime) ;
    }

    public void Petrified() {
        float petrifyAmt = enemySpawner.petrifyArray[entityID];
        if (PetrifyRecord == petrifyAmt) return;
        else PetrifyRecord = petrifyAmt;

            for (int i = 0; i < meshes.Length; ++i)
            {
                List<Material> mats = new List<Material>();
                meshes[i].GetMaterials(mats);
                foreach (Material j in mats)
                {
                    if (j.name == "Desertification (Instance)")
                    {
                        j.SetFloat("_Progress", petrifyAmt);
                    }
                }
            }
    }
    public void Slowed() {
        float slow = enemySpawner.slowArray[entityID];
        if (SlowRecord == slow) return;
        else SlowRecord = slow;

        for (int i = 0; i < meshes.Length; ++i)
        {
            List<Material> mats = new List<Material>();
            meshes[i].GetMaterials(mats);
            foreach (Material j in mats)
            {
                if (j.name == "FreezingMat (Instance)")
                {
                    //Debug.Log(slow);
                    j.SetFloat("_Progress", slow);
                }
            }
        }

    }
}
