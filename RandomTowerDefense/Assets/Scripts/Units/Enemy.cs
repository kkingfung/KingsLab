using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Entities;

public class Enemy : MonoBehaviour
{
    private GameObject DieEffect;
    private GameObject DropEffect;

    private Vector3 oriScale;
    private int DamagedCount = 0;
    private float HealthRecord;
    private Animator animator;

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
     
    }

    public void Init(GameObject DieEffect, GameObject DropEffect,int entityID)
    {
        this.DieEffect = DieEffect;
        this.DropEffect = DropEffect;
        this.entityID = entityID;
    }

    public void Damaged(float currHP)
    {
        transform.localScale = oriScale*0.5f;
        //DamagedCount = 2;
        if (currHP <= 0) {
            isDead = true;
            resourceManager.ChangeMaterial(enemySpawner.moneyArray[entityID]);
            GameObject vfx = Instantiate(DropEffect, this.transform.position, Quaternion.identity);
            vfx.GetComponent<VisualEffect>().SetInt("SpawnCount", enemySpawner.moneyArray[entityID]);

            if (animator)
                animator.SetTrigger("Dead");
            Die();
        }
    }

    public void Die()
    {
        GameObject.Instantiate(DieEffect, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject, animator == null ? 0:2) ;
    }

    public void Petrified() {
        float petrifyAmt = enemySpawner.petrifyArray[entityID];
        MeshRenderer[] meshes = this.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < meshes.Length; ++i)
        {
            List<Material> mats = new List<Material>();
            meshes[i].GetMaterials(mats);
            if (mats.Count >= 3)
            {
                    mats[2].SetFloat("_Progress", petrifyAmt);
            }
        }
    }
    public void Slowed() {
        float slow = enemySpawner.slowArray[entityID];
        MeshRenderer[] meshes = this.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < meshes.Length; ++i)
        {
            List<Material> mats = new List<Material>();
            meshes[i].GetMaterials(mats);
            if (mats.Count >= 3)
            {
                    mats[1].SetFloat("_Progress", slow);
            }
        }

    }
}
