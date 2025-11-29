using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Rendering;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.Units;
using RandomTowerDefense.Info;
using RandomTowerDefense.Managers.System;

/// <summary>
/// スキルクラス - スキルの動作を管理
/// </summary>
public class Skill : MonoBehaviour
{
    /// <summary>
    /// エンティティID
    /// </summary>
    public int entityID;

    /// <summary>
    /// プレイヤーマネージャー
    /// </summary>
    private PlayerManager playerManager;

    /// <summary>
    /// オーディオマネージャー
    /// </summary>
    private AudioManager audioManager;

    /// <summary>
    /// スキルスポーナー
    /// </summary>
    private SkillSpawner skillSpawner;

    /// <summary>
    /// スキルID
    /// </summary>
    private UpgradesManager.StoreItems actionID;

    /// <summary>
    /// スキル属性
    /// </summary>
    private SkillAttr attr;

    /// <summary>
    /// アクション終了フラグ
    /// </summary>
    private bool ActionEnded;

    /// <summary>
    /// ターゲットエネミー座標
    /// </summary>
    private Vector3 targetEnemy;

    /// <summary>
    /// オーディオソース
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// デフォルトターゲットオブジェクト
    /// </summary>
    private GameObject defaultTarget;

    /// <summary>
    /// ビジュアルエフェクトコンポーネント
    /// </summary>
    private VisualEffect VFX;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Start()
    {
        ActionEnded = false;

        playerManager = FindObjectOfType<PlayerManager>();
        audioManager = FindObjectOfType<AudioManager>();
        audioSource = GetComponent<AudioSource>();

        if (skillSpawner == null)
        {
            skillSpawner = FindObjectOfType<SkillSpawner>();
        }

        if (VFX == null)
        {
            VFX = this.GetComponent<VisualEffect>();
        }

        switch (actionID)
        {
            case UpgradesManager.StoreItems.MagicMeteor:
                //this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_MagicFire"));
                }
                //skillSpawner.UpdateEntityPos(entityID,transform.position);
                break;
            case UpgradesManager.StoreItems.MagicBlizzard:
                //this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                if (audioManager.enabledSE)
                {
                    audioSource.clip = audioManager.GetAudio("se_MagicBlizzard");
                    audioSource.loop = true;
                    audioSource.Play();
                }
                //skillSpawner.UpdateEntityPos(entityID, transform.position);
                break;
            case UpgradesManager.StoreItems.MagicPetrification:
                //this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                if (audioManager.enabledSE)
                {
                    audioSource.PlayOneShot(audioManager.GetAudio("se_MagicPetrification"));
                }
                // skillSpawner.UpdateEntityPos(entityID, transform.position);
                break;
            case UpgradesManager.StoreItems.MagicMinions:
                break;
        }
    }

    private void OnEnable()
    {
        targetEnemy = new Vector3();
    }
    // Update is called once per frame
    private void Update()
    {
        switch (actionID)
        {
            case UpgradesManager.StoreItems.MagicMeteor:
            case UpgradesManager.StoreItems.MagicPetrification:
                break;
            case UpgradesManager.StoreItems.MagicMinions:
                //Debug.Log(entityID + ":" + skillSpawner.hastargetArray[entityID]);
                if (targetEnemy.y == 0 && findEnemy())
                {
                    VFX.SetVector3("TargetLocation",
                        targetEnemy - this.transform.position);
                    if (audioManager.enabledSE)
                    {
                        audioSource.PlayOneShot(audioManager.GetAudio("se_MagicSummon"));
                    }
                    skillSpawner.UpdateEntityPos(entityID, targetEnemy);
                }
                break;
            case UpgradesManager.StoreItems.MagicBlizzard:
                if (ActionEnded == false)
                {
                    Vector3 result = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                    if (result.sqrMagnitude != 0)
                    {
                        this.transform.position = playerManager.RaycastTest(LayerMask.GetMask("Arena"));
                        skillSpawner.UpdateEntityPos(entityID, this.transform.position);
                    }

                    attr.LifeTime -= Time.deltaTime;
                    if (!ActionEnded && attr.LifeTime < 0)
                    {
                        this.gameObject.SetActive(false);
                        ActionEnded = true;
                    }
                }
                break;
        }
    }

    public void Init(SkillSpawner skillSpawner, UpgradesManager.StoreItems actionID, SkillAttr attr, int entityID)
    {
        this.actionID = actionID;
        this.attr = new SkillAttr(attr);
        this.entityID = entityID;
        this.skillSpawner = skillSpawner;
        switch (actionID)
        {
            case UpgradesManager.StoreItems.MagicMinions:
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
            case UpgradesManager.StoreItems.MagicPetrification:
                if (VFX == null) VFX = this.GetComponent<VisualEffect>();
                VFX.SetFloat("Radius", val * 1.5f);
                VFX.SetFloat("Rotation", val / attr.LifeTime * 30.0f);
                break;
        }
    }

    private bool findEnemy()
    {
        //if (defaultTarget == null)
        //    defaultTarget = GameObject.FindGameObjectWithTag("DebugTag");

        if (skillSpawner.hastargetArray[entityID])
        {
            targetEnemy = skillSpawner.targetArray[entityID];
            return true;
        }
        return false;
    }
}
