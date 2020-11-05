using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Rendering;

public class Skill : MonoBehaviour
{
    private Upgrades.StoreItems actionID;
    private SkillAttr attr;

    private bool ActionEnded;
    private int entityID;

    private PlayerManager playerManager;//For Raycasting target Position

    private GameObject targetEnm;

    private AudioManager audioManager;
    private AudioSource audioSource;

    private EnemySpawner enemySpawner;

    private float temp;//for any purpose

    private SkillSpawner skillSpawner;

    private GameObject defaultTarget;
    // Start is called before the first frame update
    private void Start()
    {
        if (skillSpawner == null)
            MyStart();
    }

    private void MyStart() {
        ActionEnded = false;

        playerManager = FindObjectOfType<PlayerManager>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        audioManager = FindObjectOfType<AudioManager>();
        audioSource = GetComponent<AudioSource>();
        skillSpawner = FindObjectOfType<SkillSpawner>();

        switch (actionID)
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
                audioSource.PlayOneShot(audioManager.GetAudio("se_MagicPetrification"));
                break;
            case Upgrades.StoreItems.MagicMinions:
                findEnm();
                this.GetComponent<VisualEffect>().SetVector3("TargetLocation",
                    targetEnm.transform.position - this.transform.position);
                audioSource.PlayOneShot(audioManager.GetAudio("se_MagicSummon"));

                break;
        }
    }
    // Update is called once per frame
    private void Update()
    {
        switch (actionID)
        {
            case Upgrades.StoreItems.MagicMeteor:
            case Upgrades.StoreItems.MagicPetrification:
            case Upgrades.StoreItems.MagicMinions:
                attr.waitTime -= Time.deltaTime;
                if (ActionEnded == false && attr.waitTime < 0)
                {
                    if (!ActionEnded)
                    {
                        Destroy(this.gameObject);
                        ActionEnded = true;
                    }
                }
                break;
            case Upgrades.StoreItems.MagicBlizzard:
                if (ActionEnded == false)
                {
                    this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                    EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    entityManager.SetComponentData(skillSpawner.Entities[entityID], new Translation
                    {
                        Value = this.transform.position
                    });

                    attr.lifeTime -= Time.deltaTime;
                    if (!ActionEnded && attr.lifeTime < 0)
                    {
                        Destroy(this.gameObject);
                        ActionEnded = true;
                    }
                }
                break;
        }
    }

    public void Init(Upgrades.StoreItems actionID, SkillAttr attr, int entityID)
    {
        this.actionID = actionID;
        this.attr = new SkillAttr(attr);
        this.entityID = entityID;
        if (skillSpawner == null)
            MyStart();

        switch (actionID)
        {
            case Upgrades.StoreItems.MagicMinions:
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                entityManager.SetComponentData(skillSpawner.Entities[entityID], new Translation
                {
                    Value = targetEnm.transform.position
                });
                break;
        }
    }

    public void SetConstantForVFX(float val)
    {
        switch (actionID)
        {
            case Upgrades.StoreItems.MagicPetrification:
                this.GetComponent<VisualEffect>().SetFloat("Radius", val * 1.5f);
                this.GetComponent<VisualEffect>().SetFloat("Rotation", val / attr.lifeTime * 30.0f);
                break;
        }
    }

    private void findEnm()
    {
        if (targetEnm != null) return;
        if (defaultTarget == null)
            defaultTarget = GameObject.FindGameObjectWithTag("DebugTag");

        this.transform.position = Camera.main.transform.position;
        Vector3 targetPos = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
        List<GameObject> allAliveMonsters = enemySpawner.AllAliveMonstersList();
        if (allAliveMonsters.Count > 0)
        {
            GameObject nearestMonster = null;
            float dist = float.MaxValue;
            foreach (GameObject i in allAliveMonsters)
            {
                float tempDist = (i.transform.position - targetPos).sqrMagnitude;
                if (tempDist < dist)
                {
                    dist = tempDist;
                    nearestMonster = i;
                }
            }
            targetEnm = nearestMonster;
        }
        else targetEnm = defaultTarget;
    }
}
