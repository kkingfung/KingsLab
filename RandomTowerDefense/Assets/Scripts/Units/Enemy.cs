using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.UI;
using Unity.Entities;

using Unity.MLAgents;

public class Enemy : MonoBehaviour
{
    private readonly int DmgCntIncrement = 2;
    private readonly int ResizeFactor = 5;
    private readonly float ResizeScale = 0.0f;
    private readonly float EnemyDestroyTime = 0.2f;

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
    private EffectSpawner effectManager;

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
        if (effectManager == null) effectManager = FindObjectOfType<EffectSpawner>();
        resourceManager = FindObjectOfType<ResourceManager>();

        if (animator == null) 
            animator = GetComponent<Animator>();

        if (animator == null) {
            animator = GetComponentInChildren<Animator>();
        }

        if (HpBar == null)
            HpBar = GetComponentInChildren<Slider>();
        if (HpBar != null)
            HpBarRot = HpBar.gameObject.GetComponent<RectTransform>();

        meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
   
    }

    private void Start() 
    {

    }

    private void MyStart()
    {
        HealthRecord = 1;
        prevPos = transform.position;
        oriPos = prevPos;
        isDead = false;
        isReady = false;

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
            if ((DamagedCount / ResizeFactor) % 3 == 0 && DamagedCount!=0)
                transform.localScale = oriScale * ResizeScale; 
            else
                transform.localScale = oriScale;
        }

        if (transform.position != prevPos)
        {
            Vector3 direction = transform.position - prevPos;
            transform.forward = direction;
            prevPos = transform.position;
        }
    }

    public void Init(EnemySpawner enemySpawner, EffectSpawner effectManager, int entityID, int money, AgentScript agent = null)
    {
        MyStart();

        this.enemySpawner = enemySpawner;
        this.effectManager = effectManager;
        this.entityID = entityID;
        this.money = money;
        this.agent = agent;
        HpBar.maxValue = 1;
        HpBar.value = 1;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if (animator)
            animator.SetTrigger("Reused");
    }

    public void Damaged(float currHP)
    {
        DamagedCount += DmgCntIncrement;
        if (DamagedCount % 5 == 0)
        {
            effectManager.Spawn(4, this.transform.position);
        }
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
        effectManager.Spawn(1, this.transform.position + Vector3.up * 0.5f);
        yield return new WaitForSeconds(EnemyDestroyTime*0.2f);

        GameObject vfx = effectManager.Spawn(2, this.transform.position);
        vfx.GetComponent<VisualEffect>().SetInt("SpawnCount", Mathf.Max(money /10,1));
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

        this.gameObject.SetActive(false);
        //Destroy(this.gameObject);
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
