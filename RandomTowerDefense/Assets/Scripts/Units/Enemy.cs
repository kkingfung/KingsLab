using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Entities;
using UnityEditor;

public class Enemy : MonoBehaviour
{
    private GameObject DieEffect;
    private GameObject DropEffect;

    private Vector3 oriScale;
    private Vector3 prevPos;
    private int DamagedCount = 0;
    private int money;
    private float HealthRecord;
    private Animator animator;

    private float SlowRecord;
    private float PetrifyRecord;

    private int entityID=-1;
    private EnemySpawner enemySpawner;
    private ResourceManager resourceManager;

    private bool isDead;
    // Start is called before the first frame update
    private void Awake()
    {
        oriScale = transform.localScale;
        enemySpawner = FindObjectOfType<EnemySpawner>();
        resourceManager = FindObjectOfType<ResourceManager>();
        animator = GetComponent<Animator>();

        if (animator == null) {
            animator = GetComponentInChildren<Animator>();
        }

        HealthRecord = 1;
        prevPos = transform.position;
        isDead = false;
        if (animator==null) animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDead) return;

        if (entityID >= 0 && HealthRecord>0) {
            float currHP = enemySpawner.healthArray[entityID];
            if (currHP != HealthRecord) {
                Damaged(currHP);
                currHP = HealthRecord;
            }
      
            //CheckingStatus
            Petrified();
            Slowed();
        }
        if (DamagedCount > 0) { DamagedCount--;
            if(DamagedCount == 0)
                transform.localScale = oriScale;
        }
        if (transform.position != prevPos)
        {
            Vector3 direction = transform.position - prevPos;
            transform.forward = direction;
            prevPos = transform.position;
        }


    }

    public void Init(GameObject DieEffect, GameObject DropEffect,int entityID,int money)
    {
        this.DieEffect = DieEffect;
        this.DropEffect = DropEffect;
        this.entityID = entityID;
        this.money = money;
    }

    public void Damaged(float currHP)
    {
        transform.localScale = oriScale*0.5f;
        //DamagedCount = 2;
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
        Destroy(this.gameObject, 2) ;
    }

    public void Petrified() {
        float petrifyAmt = enemySpawner.petrifyArray[entityID];
        if (PetrifyRecord == petrifyAmt) return;
        else PetrifyRecord = petrifyAmt;

        SkinnedMeshRenderer[] meshes = this.GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < meshes.Length; ++i)
        {
            List<Material> mats = new List<Material>();
            meshes[i].GetMaterials(mats);
            foreach (Material j in mats)
            {
                if (j.name== "Desertification (Instance)")
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

        SkinnedMeshRenderer[] meshes = this.GetComponentsInChildren<SkinnedMeshRenderer>();
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
