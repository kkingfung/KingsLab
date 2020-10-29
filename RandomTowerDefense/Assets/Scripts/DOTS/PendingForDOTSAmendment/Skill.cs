using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Skill : MonoBehaviour
{
    private Upgrades.StoreItems ActionID;
    private SkillAttr attr;

    private bool Actioned;
    private Vector3 ActivePos;

    private PlayerManager playerManager;//For Raycasting target Position

    private EnemyManager enemyManager;//For Homing position;
    private GameObject targetEnm;

    private AudioManager audioManager;
    private AudioSource audioSource;

    private float temp;//for any purpose

    //Testing
    GameObject testobj;

    // Start is called before the first frame update
    void Start()
    {
        Actioned = false;
        ActivePos = transform.position;

        playerManager = FindObjectOfType<PlayerManager>();
        enemyManager = FindObjectOfType<EnemyManager>();
        audioManager = FindObjectOfType<AudioManager>();
        audioSource = GetComponent<AudioSource>();

        testobj = GameObject.FindGameObjectWithTag("DebugTag");

        switch (ActionID)
        {
            case Upgrades.StoreItems.MagicMeteor:
                this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                audioSource.PlayOneShot(audioManager.GetAudio("se_MagicFire"));
                break;
            case Upgrades.StoreItems.MagicBlizzard:
                this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                audioSource.clip = audioManager.GetAudio("se_MagicBlizzard");
                audioSource.loop = true;
                audioSource.Play();
                break;
            case Upgrades.StoreItems.MagicPetrification:
                this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                foreach (GameObject i in enemyManager.allAliveMonsters)
                {
                    i.GetComponent<EnemyAI>().Petrified(attr.frameWait);
                }
                audioSource.PlayOneShot(audioManager.GetAudio("se_MagicPetrification"));
                break;
            case Upgrades.StoreItems.MagicMinions:
                this.transform.position = Camera.main.transform.position;
                Vector3 targetPos = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                if (enemyManager.allAliveMonsters.Count > 0)
                {
                    GameObject nearestMonster = null;
                    float dist = float.MaxValue;
                    foreach (GameObject i in enemyManager.allAliveMonsters)
                    {
                        float tempDist = (i.transform.position - targetPos).sqrMagnitude;
                        if (tempDist < dist)
                        {
                            dist = tempDist;
                            nearestMonster = i;
                        }
                    }
                }
                else targetEnm = null;

               if (testobj) targetEnm = testobj;
                
                this.GetComponent<VisualEffect>().SetVector3("TargetLocation", targetEnm.transform.position-this.transform.position);
                audioSource.PlayOneShot(audioManager.GetAudio("se_MagicSummon"));

                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (ActionID)
        {
            case Upgrades.StoreItems.MagicMeteor:
                attr.frameWait -= Time.deltaTime;
                if (Actioned == false && attr.frameWait < 0)
                {
                    Vector2 skillPos = new Vector2(this.transform.position.x, this.transform.position.z);
                    foreach (GameObject i in enemyManager.allAliveMonsters)
                    {
                        Vector2 enmPos = new Vector2(i.transform.position.x, i.transform.position.z);
                        if ((enmPos - skillPos).sqrMagnitude <= attr.area * attr.area)
                        {
                            i.GetComponent<EnemyAI>().Damaged(attr.damage);
                        }
                    }

                    if (!Actioned)
                    {
                        Destroy(this.gameObject);
                        Actioned = true;
                    }
                }
                break;
            case Upgrades.StoreItems.MagicBlizzard:
                if (Actioned == false)
                {
                    this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                    attr.frameWait -= Time.deltaTime;
                        if (attr.frameWait < 0)
                    {
                        Vector2 skillPos = new Vector2(this.transform.position.x, this.transform.position.z);
                        foreach (GameObject i in enemyManager.allAliveMonsters)
                        {
                            Vector2 enmPos = new Vector2(i.transform.position.x, i.transform.position.z);
                            if ((enmPos - skillPos).sqrMagnitude <= attr.area * attr.area)
                            {
                                i.GetComponent<EnemyAI>().Damaged(attr.damage);
                                i.GetComponent<EnemyAI>().Slowed(attr.cycleTime);
                            }
                        }
                        attr.frameWait = temp;
                    }
                    attr.activeTime -= Time.deltaTime;
                    if (!Actioned && attr.activeTime < 0)
                    {
                        Destroy(this.gameObject);
                        Actioned = true;
                    }
                }
                break;
            case Upgrades.StoreItems.MagicPetrification:
                attr.frameWait -= Time.deltaTime;
                if (Actioned == false && attr.frameWait < 0)
                {
                    if (!Actioned)
                    {
                        Destroy(this.gameObject);
                        Actioned = true;
                    }
                }
                break;
            case Upgrades.StoreItems.MagicMinions:
                attr.frameWait -= Time.deltaTime;
                if (Actioned == false && attr.frameWait < 0)
                {  
                    if (targetEnm != null)
                    {
                        if(targetEnm.GetComponent<EnemyAI>())
                        targetEnm.GetComponent<EnemyAI>().Damaged(attr.damage);
                    }
                    if (!Actioned)
                    {
                        Destroy(this.gameObject);
                        Actioned = true;
                    }
                }
                break;
        }
    }

    public void init(Upgrades.StoreItems ActionID, SkillAttr attr)
    {
        this.ActionID = ActionID;
        this.attr = new SkillAttr(attr);
    }

    public void SetTemp(float val)
    {
        temp = val;
        switch (ActionID)
        {
            case Upgrades.StoreItems.MagicPetrification:
                this.GetComponent<VisualEffect>().SetFloat("Radius", temp*1.5f);
                this.GetComponent<VisualEffect>().SetFloat("Rotation",temp/attr.activeTime*30.0f);
                break;
        }
    }
}
