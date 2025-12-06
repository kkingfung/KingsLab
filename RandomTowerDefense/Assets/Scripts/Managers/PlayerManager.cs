using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Scene;
using RandomTowerDefense.MapGenerator;
using RandomTowerDefense.Info;
using RandomTowerDefense.Systems;
using RandomTowerDefense.Units;

/// <summary>
/// プレイヤー入力管理システム - タッチ/マウス入力と全ゲームアクションの制御
///
/// 主な機能:
/// - タッチ/マウス入力処理と画面レイキャスト
/// - タワー建設、マージ、売却操作
/// - スキル発動とストックシステム管理
/// - ランドスケープ/ポートレートカメラ連携
/// - ボーナスボス召喚処理
/// - Unity Physics & ECS DOTS統合
/// - タワー選択とUI更新
/// - ダブルタップ検出と応答
/// </summary>
public class PlayerManager : MonoBehaviour
{
    #region Constants
    /// <summary>ストック操作キャンセル距離の閾値</summary>
    private readonly float CancelDist = 30f;
    #endregion

    #region Serialized Fields
    /// <summary>ランドスケープ（横向き）モード用カメラ</summary>
    [Header("Camera Settings")]
    public Camera refCamL;

    /// <summary>ポートレート（縦向き）モード用カメラ</summary>
    public Camera refCamP;

    /// <summary>ジャイロスコープ制御オブジェクト</summary>
    public GameObject refGyro;

    /// <summary>ストックオペレーター表示深度</summary>
    [Header("Stock Settings")]
    public float StockOperatorDepth;

    /// <summary>ストックオペレータープレハブ</summary>
    public GameObject StockOperatorPrefab;

    [Header("Manager References")]
    /// <summary>シーン管理クラスの参照</summary>
    public InGameOperation sceneManager;

    /// <summary>敵管理クラスの参照</summary>
    public EnemyManager enemyManager;

    /// <summary>ステージ管理クラスの参照</summary>
    public StageManager stageManager;

    /// <summary>入力管理クラスの参照</summary>
    public InputManager inputManager;

    /// <summary>タワー管理クラスの参照</summary>
    public TowerManager towerManager;

    /// <summary>マップ生成クラスの参照</summary>
    public FilledMapGenerator mapGenerator;

    /// <summary>スキル管理クラスの参照</summary>
    public SkillManager skillManager;
    #endregion

    #region Public Fields
    /// <summary>スキルアクティブ状態フラグ</summary>
    [HideInInspector]
    public bool isSkillActive;

    /// <summary>タワー売却モードフラグ</summary>
    [HideInInspector]
    public bool isSelling;

    /// <summary>ストック選択中フラグ</summary>
    [HideInInspector]
    public bool isChecking;

    /// <summary>レイキャストヒットしたタワーGameObject</summary>
    public GameObject TowerRaycastResult;

    /// <summary>ストアレイキャストヒットTransform</summary>
    public Transform hitStore;
    #endregion

    #region Private Fields
    /// <summary>ストック選択開始位置</summary>
    private Vector2 StockPos;

    /// <summary>ストックオペレーターインスタンス</summary>
    private GameObject StockOperator;

    /// <summary>柱レイキャストヒット情報</summary>
    private UnityEngine.RaycastHit hitPillar;

    /// <summary>アリーナレイキャストヒット位置</summary>
    private Vector3 hitArena;
    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// 初期化処理 - 管理クラスの参照取得（現在はコメントアウト）
    /// </summary>
    private void Start()
    {
        //sceneManager = FindObjectOfType<InGameOperation>();
        //enemyManager = FindObjectOfType<EnemyManager>();
        //stageManager = FindObjectOfType<StageManager>();
        //inputManager = FindObjectOfType<InputManager>();
        //towerManager = FindObjectOfType<TowerManager>();
        //skillManager = FindObjectOfType<SkillManager>();
        //mapGenerator = FindObjectOfType<FilledMapGenerator>();
    }

    /// <summary>
    /// 毎フレーム更新 - レイキャストテストとUI更新
    /// </summary>
    private void Update()
    {
        isSkillActive = false;
        Physics.Simulate(Time.fixedDeltaTime);
        TowerRaycastResult = null;
        hitPillar = new UnityEngine.RaycastHit();
        hitArena.y = float.MinValue;

        if (sceneManager)
        {
            if (sceneManager.GetOptionStatus() == false)
            {
                if (sceneManager.currScreenShown == 0)
                {
                    TowerRaycastResult = CheckTowerRaycast();
                    towerManager.UpdateInfo(TowerRaycastResult);
                }
                else
                {
                    RaycastTest(LayerMask.GetMask("StoreLayer"));
                }
            }
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// タワーレイキャストチェック - 画面中央からタワーを検出
    /// </summary>
    /// <returns>ヒットしたタワーのGameObject（ヒットしない場合はnull）</returns>
    private GameObject CheckTowerRaycast()
    {

        UnityEngine.Ray ray;
        UnityEngine.RaycastHit hit = new UnityEngine.RaycastHit();

        //Get Ray according to orientation
        if (Screen.width > Screen.height)
        {
            if (refCamL == null) return null;
            ray = refCamL.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }
        else
        {
            if (refCamP == null) return null;
            ray = refCamP.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }
        Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Tower"));
        if (hit.transform)
            return hit.transform.gameObject;
        return null;
    }

    #endregion

    #region Public API

    /// <summary>
    /// ストック選択開始位置を取得
    /// </summary>
    /// <returns>ストック選択開始位置</returns>
    public Vector2 GetDragPos() { return StockPos; }

    /// <summary>
    /// ストック選択UI表示処理 - ドラッグ開始時に呼び出される
    /// </summary>
    /// <param name="DragPos">ドラッグ開始位置</param>
    public void CheckStock(Vector2 DragPos)
    {
        if (StockOperator != null) return;
        if (isSkillActive) return;
        isSelling = false;
        //if (sceneManager.currScreenShown != (int)InGameOperation.ScreenShownID.SSIDArena || sceneManager.nextScreenShown != (int)InGameOperation.ScreenShownID.SSIDArena) return;
        isChecking = true;

        Camera targetCam = (Screen.width > Screen.height) ? refCamL : refCamP;
        StockOperator = Instantiate(StockOperatorPrefab, targetCam.transform);
        StockOperator.transform.position = targetCam.ScreenToWorldPoint(new Vector3(DragPos.x, DragPos.y, StockOperatorDepth));
        StockPos = DragPos;
    }

    /// <summary>
    /// ストック使用処理 - ドラッグ終了時にストック選択方向を判定してアクション実行
    /// </summary>
    /// <param name="DragPos">ドラッグ終了位置</param>
    public void UseStock(Vector2 DragPos)
    {
        if (StockOperator == null) return;
        if (isSkillActive) return;

        isChecking = false;
        Destroy(StockOperator);
        StockOperator = null;

        if (Input.touchCount > 0)
        {
            if ((Input.touches[0].position - StockPos).sqrMagnitude < CancelDist)
                return;
        }
        else
        {
            if ((new Vector2(Input.mousePosition.x, Input.mousePosition.y) - StockPos).sqrMagnitude < CancelDist)
                return;
        }

        int StockSelected = -1;
        //Check Action to be taken by DragPos && current mouse/touch
        float AngleCalculation;
        if (Input.touchCount > 0)
            AngleCalculation = (-90f + Mathf.Rad2Deg * Mathf.Atan2(Input.touches[0].position.y - StockPos.y, Input.touches[0].position.x - StockPos.x));
        else
            AngleCalculation = (-90f + Mathf.Rad2Deg * Mathf.Atan2(Input.mousePosition.y - StockPos.y, Input.mousePosition.x - StockPos.x));
        while (AngleCalculation < 0) AngleCalculation += 360f;

        if (AngleCalculation >= 72f && AngleCalculation < 144f)
            StockSelected = 0;
        else if (AngleCalculation >= 0f && AngleCalculation < 72f)
            StockSelected = 1;
        else if (AngleCalculation >= 288f && AngleCalculation < 360f)
            StockSelected = 2;
        else if (AngleCalculation >= 216f && AngleCalculation < 288f)
            StockSelected = 3;
        else isSelling = true;
        //Raycast test ground and camera ray
        Vector3 hitPosition = RaycastTest(LayerMask.GetMask("Arena"));
        if (isSelling) return;

        //For non-sale
        switch (SkillStack.UseStock(StockSelected))
        {
            case (int)UpgradesManager.StoreItems.BonusBossGreen:
                enemyManager.SpawnBonusBoss(0, stageManager.GetPortalPosition(DefaultStageInfos.Prng.Next(0, 2)));
                break;
            case (int)UpgradesManager.StoreItems.BonusBossPurple:
                enemyManager.SpawnBonusBoss(1, stageManager.GetPortalPosition(DefaultStageInfos.Prng.Next(0, 2)));
                break;
            case (int)UpgradesManager.StoreItems.BonusBossRed:
                enemyManager.SpawnBonusBoss(2, stageManager.GetPortalPosition(DefaultStageInfos.Prng.Next(0, 2)));
                break;
            case (int)UpgradesManager.StoreItems.MagicMeteor:
                //CurrentSkill = 
                skillManager.MeteorSkill(hitPosition);
                isSkillActive = true;
                break;
            case (int)UpgradesManager.StoreItems.MagicBlizzard:
                //CurrentSkill = 
                skillManager.BlizzardSkill(hitPosition);
                isSkillActive = true;
                break;
            case (int)UpgradesManager.StoreItems.MagicMinions:
                //CurrentSkill = 
                skillManager.MinionsSkill(hitPosition);
                isSkillActive = true;
                break;
            case (int)UpgradesManager.StoreItems.MagicPetrification:
                //CurrentSkill = 
                skillManager.PetrificationSkill(hitPosition);
                isSkillActive = true;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// ECS DOTS統合レイキャストテスト - Unity Physicsシステムを使用
    /// </summary>
    /// <param name="fromPosition">レイキャスト開始位置</param>
    /// <param name="toPosition">レイキャスト終了位置</param>
    /// <param name="layer">レイヤーマスク</param>
    /// <returns>ヒット位置</returns>
    public Vector3 RaycastTestDOTS(float3 fromPosition, float3 toPosition, int layer)
    {
        if (layer == LayerMask.GetMask("Arena") && hitArena.z != float.MinValue)
        {
            return hitArena;
        }

        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        RaycastInput raycastInput = new RaycastInput
        {
            Start = fromPosition,
            End = toPosition,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = (uint)layer,
                GroupIndex = 0,
            }
        };

        Unity.Physics.RaycastHit raycastHit = new Unity.Physics.RaycastHit();

        if (collisionWorld.CastRay(raycastInput, out raycastHit))
        {
            if (layer == LayerMask.GetMask("Arena"))
            {
                hitArena = raycastHit.Position;
                return hitArena;
            }
        }

        return raycastHit.Position;
    }

    /// <summary>
    /// レイキャストテスト - 画面中央から指定レイヤーへレイキャスト
    /// </summary>
    /// <param name="layer">レイキャスト対象レイヤー</param>
    /// <returns>ヒット位置</returns>
    public Vector3 RaycastTest(int layer)
    {
        if (layer == LayerMask.GetMask("Arena") && hitArena.y != float.MinValue)
            return hitArena;

        UnityEngine.Ray ray = new UnityEngine.Ray();
        UnityEngine.RaycastHit hit = new UnityEngine.RaycastHit();

        //Get Ray according to orientation
        if (Screen.width > Screen.height)
        {
            ray = refCamL.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }
        else
        {
            ray = refCamP.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }

        Physics.Raycast(ray, out hit, 200, layer);
        if (layer == LayerMask.GetMask("Arena"))
            hitArena = hit.point;
        if (layer == LayerMask.GetMask("StoreLayer"))
            hitStore = hit.transform;
        return hit.point;
    }

    /// <summary>
    /// ダブルタップレイキャストテスト - タワー操作アクションの実行
    /// </summary>
    /// <param name="isDoubleTap">ダブルタップ検出フラグ</param>
    /// <returns>ヒット位置</returns>
    public Vector3 RaycastTest(bool isDoubleTap)
    {
        if (sceneManager.GetOptionStatus())
            return new Vector3();

        if (isDoubleTap)
        {
            inputManager.TapTimeRecord = 0;
        }
        else
        {
            return new Vector3();
        }

        UnityEngine.Ray ray;

        //Get Ray according to orientation
        if (Screen.width > Screen.height)
        {
            ray = refCamL.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }
        else
        {
            ray = refCamP.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }

        //TakeAction
        if (isSelling)
        {
            isSelling = false;
            if (TowerRaycastResult)
            {
                towerManager.SellTower(TowerRaycastResult.GetComponent<Tower>());
            }
        }
        else
        {
            if (TowerRaycastResult != null)
            {
                towerManager.MergeTower(TowerRaycastResult);
            }
            else
            {
                if (hitPillar.transform == null)
                    Physics.Raycast(ray, out hitPillar, 100, LayerMask.GetMask("Pillar"));
                if (hitPillar.transform != null && TowerInfo.InfoUpdated)
                    towerManager.BuildTower(hitPillar.transform.gameObject);
            }
        }
        return hitPillar.point;
    }

    /// <summary>
    /// ストックオペレーターの存在確認
    /// </summary>
    /// <returns>ストックオペレーターが存在する場合true</returns>
    public bool StockCheckExist()
    {
        return (this.StockOperator != null);
    }

    /// <summary>
    /// ECS DOTS統合エンティティレイキャスト - Unity Physicsを使用してエンティティを取得
    /// </summary>
    /// <param name="fromPosition">レイキャスト開始位置</param>
    /// <param name="toPosition">レイキャスト終了位置</param>
    /// <returns>ヒットしたECSエンティティ（ヒットしない場合はEntity.Null）</returns>
    private Entity Raycast(float3 fromPosition, float3 toPosition)
    {
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        RaycastInput raycastInput = new RaycastInput
        {
            Start = fromPosition,
            End = toPosition,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0,
            }
        };

        Unity.Physics.RaycastHit raycastHit = new Unity.Physics.RaycastHit();

        if (collisionWorld.CastRay(raycastInput, out raycastHit))
        {
            // Hit something
            return buildPhysicsWorld.PhysicsWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
        }
        else
        {
            return Entity.Null;
        }
    }

    #endregion
}
