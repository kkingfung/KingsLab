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

    private Vector3 targetEnm;

    private AudioManager audioManager;
    private AudioSource audioSource;

   // private EnemySpawner enemySpawner;

    private SkillSpawner skillSpawner;

    private GameObject defaultTarget;
    private VisualEffect VFX;
    // Start is called before the first frame update
    private void Start()
    {
        ActionEnded = false;

        playerManager = FindObjectOfType<PlayerManager>();
        //enemySpawner = FindObjectOfType<EnemySpawner>();
        audioManager = FindObjectOfType<AudioManager>();
        if (skillSpawner == null) skillSpawner = FindObjectOfType<SkillSpawner>();
        if (VFX == null) VFX = this.GetComponent<VisualEffect>();

        switch (actionID)
        {
            case Upgrades.StoreItems.MagicMeteor:
                //this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_MagicFire"));
                }
                //skillSpawner.UpdateEntityPos(entityID,transform.position);
                break;
            case Upgrades.StoreItems.MagicBlizzard:
                //this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                if (audioManager.enabledSE)
                {
                    audioSource.clip = audioManager.GetAudio("se_MagicBlizzard");
                    audioSource.loop = true;
                    audioSource.Play();
                }
                //skillSpawner.UpdateEntityPos(entityID, transform.position);
                break;
            case Upgrades.StoreItems.MagicPetrification:
                //this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_MagicPetrification"));
                }
                // skillSpawner.UpdateEntityPos(entityID, transform.position);
                break;
            case Upgrades.StoreItems.MagicMinions:
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
                break;
            case Upgrades.StoreItems.MagicMinions:
                if (targetEnm == new Vector3() && findEnm())
                {
                    VFX.SetVector3("TargetLocation",
                        targetEnm - this.transform.position);
                    if (audioManager.enabledSE)
                    {
                        audioSource.PlayOneShot(audioManager.GetAudio("se_MagicSummon"));
                    }
                    skillSpawner.UpdateEntityPos(entityID, targetEnm);
                }
                break;
            case Upgrades.StoreItems.MagicBlizzard:
                if (ActionEnded == false)
                {
                    Vector3 result= playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                    if (result.sqrMagnitude != 0)
                    {
                        this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                        skillSpawner.UpdateEntityPos(entityID, this.transform.position);
                    }

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

    public void Init(SkillSpawner skillSpawner,Upgrades.StoreItems actionID, SkillAttr attr, int entityID)
    {
        this.actionID = actionID;
        this.attr = new SkillAttr(attr);
        this.entityID = entityID;
        this.skillSpawner = skillSpawner;

        switch (actionID)
        {
            case Upgrades.StoreItems.MagicMinions:
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                entityManager.SetComponentData(skillSpawner.Entities[entityID], new Translation
                {
                    Value = this.transform.position
                });
                break;
        }
    }

    public void SetConstantForVFX(float val)
    {
        switch (actionID)
        {
            case Upgrades.StoreItems.MagicPetrification:
                if (VFX == null) VFX = this.GetComponent<VisualEffect>();
                VFX.SetFloat("Radius", val * 1.5f);
                VFX.SetFloat("Rotation", val / attr.lifeTime * 30.0f);
                break;
        }
    }

    private bool findEnm()
    {
        //if (defaultTarget == null)
        //    defaultTarget = GameObject.FindGameObjectWithTag("DebugTag");
  
        if ( skillSpawner.hastargetArray[entityID])
        {
                targetEnm = skillSpawner.targetArray[entityID];
                return true;
        }
        return false;
    }
}
