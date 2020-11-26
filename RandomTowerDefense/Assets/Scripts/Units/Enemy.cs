﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI;
using Unity.Entities;

using Unity.MLAgents;

public class Enemy : MonoBehaviour
{
    private readonly int DamageCounterMax = 5;
    private readonly int ResizeFactor = 3;
    private readonly float ResizeScale = 0.0f;
    private readonly float EnemyDestroyTime = 0.2f;

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

    private AgentScript agent;
    private Vector3 oriPos;

    private SkinnedMeshRenderer[] meshes;
    public Animator animator;
    public Slider HpBar;
    private RectTransform HpBarRot;

    private bool isDead;
    private bool isReady;

    // Start is called before the first frame update
    private void Awake()
    {
        if (enemySpawner==null) enemySpawner = FindObjectOfType<EnemySpawner>();
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
        oriPos = prevPos;
        isDead = false;

        meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        isReady = false;
    }

    private void Start() 
    {

        oriScale = transform.localScale;
        transform.localScale = new Vector3();
        StartCoroutine(StartAnim());
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDead) return;
        if (entityID >= 0 && HealthRecord>0) {
            float currHP = enemySpawner.healthArray[entityID];
            if (currHP > HpBar.maxValue)
                HpBar.maxValue = currHP;
            if (currHP != HealthRecord && HealthRecord >= 0) {
                Damaged(currHP);
                HealthRecord = currHP;
                HpBar.value = Mathf.Max(currHP, 0);
            }
            HpBarRot.eulerAngles = new Vector3(0, 0, 0);
            //CheckingStatus
            Petrified();
            Slowed();
        }

        if (DamagedCount > 0 && isReady)
        {
            DamagedCount--;
            if ((DamagedCount / ResizeFactor) % 2 == 0)
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

    public void Init(EnemySpawner enemySpawner,GameObject DieEffect, GameObject DropEffect,int entityID,int money, AgentScript agent=null)
    {
        this.enemySpawner = enemySpawner;
        this.DieEffect = DieEffect;
        this.DropEffect = DropEffect;
        this.entityID = entityID;
        this.money = money;
        this.agent = agent;
        HpBar.maxValue = 1;
        HpBar.value = 1;
    }

    public void Damaged(float currHP)
    {
        DamagedCount += DamageCounterMax;
        if (currHP <= 0) {
            isDead = true;
            StartCoroutine(DieAnimation());
        }
    }

    public void Die()
    {
        transform.localScale = oriScale;
        isReady = false;
        if (agent) agent.EnemyDisappear(oriPos, this.transform.position);
        StartCoroutine(EndAnim());
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

    private IEnumerator DieAnimation()
    {
        if (animator)
            animator.SetTrigger("Dead");
        GameObject.Instantiate(DieEffect, this.transform.position+Vector3.up*0.5f, Quaternion.identity);
        yield return new WaitForSeconds(EnemyDestroyTime*0.2f);

        GameObject vfx = Instantiate(DropEffect, this.transform.position, Quaternion.identity);
        vfx.GetComponent<VisualEffect>().SetInt("SpawnCount", money);
        resourceManager.ChangeMaterial(money);
        Die();
    }

    private IEnumerator EndAnim()
    {
        float timeCounter = 0;
        float spd = transform.localScale.x / (EnemyDestroyTime);
        while (timeCounter < EnemyDestroyTime)
        {
            float delta = Time.deltaTime;
            timeCounter += delta;
            transform.localScale = new Vector3(transform.localScale.x - spd * delta,
                transform.localScale.y - spd * delta, transform.localScale.z - spd * delta);
            yield return new WaitForSeconds(0);
        }

        transform.localScale = new Vector3();
        Destroy(this.gameObject);
    }

    private IEnumerator StartAnim()
    {
        float timeCounter = 0;
        float spd = oriScale.x / (EnemyDestroyTime * 0.1f);
        while (timeCounter < EnemyDestroyTime * 0.1f)
        {
            float delta = Time.deltaTime;
            timeCounter += delta;
            transform.localScale = new Vector3(transform.localScale.x + spd * delta,
                transform.localScale.y + spd * delta, transform.localScale.z + spd * delta);
            yield return new WaitForSeconds(0);
        }

        transform.localScale = oriScale;
        isReady = true;
    }
}
