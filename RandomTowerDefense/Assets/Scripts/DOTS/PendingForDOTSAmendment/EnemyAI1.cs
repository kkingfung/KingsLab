using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Entities;

public class EnemyAI1 : MonoBehaviour
{
    private EnemyAttr attr;
    private GameObject DieEffect;
    private GameObject DropEffect;

    private Vector3 oriScale;
    private int DamagedCount = 0;

    private Animator animator;
    private Collider collider;

    // Start is called before the first frame update
    private void Start()
    {
        oriScale = transform.localScale;
        animator = GetComponent<Animator>();
        if(animator==null) animator = GetComponentInChildren<Animator>();
        collider  = GetComponent<Collider>();
        if (collider == null) collider = GetComponentInChildren<Collider>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (DamagedCount > 0) { DamagedCount--;
            if(DamagedCount == 0)
                transform.localScale = oriScale;
        }
     
    }

    public void init(GameObject DieEffect, GameObject DropEffect)
    {
        this.DieEffect = DieEffect;
        this.DropEffect = DropEffect;
    }

    public void Damaged(float dmg)
    {
        attr.health -= dmg;
        transform.localScale = oriScale*0.5f;
        DamagedCount = 1;

        if (attr.health <= 0) {
            GameObject vfx = Instantiate(DropEffect, this.transform.position, Quaternion.identity);
            vfx.GetComponent<VisualEffect>().SetFloat("SpawnCount", attr.money);
            animator.SetTrigger("Dead");
            Die();
        }
    }

    public void Die()
    {
        Destroy(collider);
        GameObject.Instantiate(DieEffect, this.transform.position, Quaternion.identity);
        FindObjectOfType<EnemyManager>().allAliveMonsters.Remove(this.gameObject);
        Destroy(this, 5);
    }

    public void Petrified(float petrifyTimer) { }
    public void Slowed(float slowTimer) { }
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