using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.VFX;

public class Tower : MonoBehaviour
{
    private readonly int[] MaxLevel = { 10, 30, 50, 70 };

    public TowerAttr attr;
    public int Level;
    public int rank;
    public TowerInfo.TowerInfoID type;

    bool isBuilding;
    GameObject CurrTarget;
    GameObject AtkVFX;
    GameObject AuraVFX;
    GameObject LevelUpVFX;

    EnemyManager enemyManager;

    private void Start()
    {
        isBuilding = true;
        enemyManager = FindObjectOfType<EnemyManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrTarget == null) CurrTarget = detectEnemy();
        if (CurrTarget != null && isBuilding==false) Attack();
    }

    public GameObject detectEnemy() {
        GameObject nearestMonster=null;
        float dist = -1;
        foreach (GameObject i in enemyManager.allAliveMonsters)
        {
            float tempDist = (i.transform.position - this.transform.position).sqrMagnitude;
            if (tempDist > attr.areaSq) continue;
            if (tempDist < dist) {
                dist = tempDist;
                nearestMonster = i;
            }
        }

        return nearestMonster;
    }

    public void Attack()
    {
        GameObject atk = Instantiate(AtkVFX, this.transform);
        atk.GetComponent<VisualEffect>().SetVector3("TargetEnm", CurrTarget.transform.position);

        CurrTarget.GetComponent<EnemyAI>().Damaged(attr.damage);
    }

    public void newTower(GameObject AtkVFX, GameObject LevelUpVFX, GameObject AuraVFX, TowerInfo.TowerInfoID type,int lv=1,int rank=1) {
        this.type = type;
        this.Level = lv;
        this.rank = rank;
        this.AtkVFX=GameObject.Instantiate(AtkVFX, this.transform);
        this.AuraVFX = GameObject.Instantiate(AuraVFX, this.transform);
        this.LevelUpVFX=GameObject.Instantiate(LevelUpVFX, this.transform);
        isBuilding = true;

        switch (type) {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                this.LevelUpVFX.GetComponent<VisualEffect>().SetVector4("MainColor",new Vector4(1,1,0,1));
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
    }

    public void LevelUp(int chg=1) {
        SetLevel(Level+chg);
    }

    public void SetLevel(int lv) {
        Level = lv;
        this.LevelUpVFX.GetComponent<VisualEffect>().SetFloat("SpawnRate", Level);
    }

    public bool CheckMaxLevel() {
        return Level == MaxLevel[rank - 1];
    }
}


////data coming from the PlaceableData
//private float speed;

//private Animator animator;
//private NavMeshAgent navMeshAgent;

//private void Awake()
//{
//    pType = Placeable.PlaceableType.Unit;

//    //find references to components
//    animator = GetComponent<Animator>();
//    navMeshAgent = GetComponent<NavMeshAgent>(); //will be disabled until Activate is called
//    audioSource = GetComponent<AudioSource>();
//}

////called by GameManager when this Unit is played on the play field
//public void Activate(Faction pFaction, PlaceableData pData)
//{
//    faction = pFaction;
//    hitPoints = pData.hitPoints;
//    targetType = pData.targetType;
//    attackRange = pData.attackRange;
//    attackRatio = pData.attackRatio;
//    speed = pData.speed;
//    damage = pData.damagePerAttack;
//    attackAudioClip = pData.attackClip;
//    dieAudioClip = pData.dieClip;
//    //TODO: add more as necessary

//    navMeshAgent.speed = speed;
//    animator.SetFloat("MoveSpeed", speed); //will act as multiplier to the speed of the run animation clip

//    state = States.Idle;
//    navMeshAgent.enabled = true;
//}

//public override void SetTarget(ThinkingPlaceable t)
//{
//    base.SetTarget(t);
//}

////Unit moves towards the target
//public override void Seek()
//{
//    if (target == null)
//        return;

//    base.Seek();

//    navMeshAgent.SetDestination(target.transform.position);
//    navMeshAgent.isStopped = false;
//    animator.SetBool("IsMoving", true);
//}

////Unit has gotten to its target. This function puts it in "attack mode", but doesn't delive any damage (see DealBlow)
//public override void StartAttack()
//{
//    base.StartAttack();

//    navMeshAgent.isStopped = true;
//    animator.SetBool("IsMoving", false);
//}

////Starts the attack animation, and is repeated according to the Unit's attackRatio
//public override void DealBlow()
//{
//    base.DealBlow();

//    animator.SetTrigger("Attack");
//    transform.forward = (target.transform.position - transform.position).normalized; //turn towards the target
//}

//public override void Stop()
//{
//    base.Stop();

//    navMeshAgent.isStopped = true;
//    animator.SetBool("IsMoving", false);
//}

//protected override void Die()
//{
//    base.Die();

//    navMeshAgent.enabled = false;
//    animator.SetTrigger("IsDead");
//}