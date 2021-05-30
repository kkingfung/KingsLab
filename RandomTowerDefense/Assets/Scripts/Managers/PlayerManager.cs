using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

public class PlayerManager : MonoBehaviour
{
    private readonly float CancelDist = 30f;

    [HideInInspector]
    public bool isSkillActive;

    [HideInInspector]
    public bool isSelling;

    [Header("Camera Settings")]
    public Camera refCamL;
    public Camera refCamP;
    public GameObject refGyro;

    [Header("Stock Settings")]
    public float StockOperatorDepth;
    public GameObject StockOperatorPrefab;
    private Vector2 StockPos;

    [HideInInspector]
    public bool isChecking;

    //private GameObject CurrentSkill;
    private GameObject StockOperator;

    public InGameOperation sceneManager;
    public EnemyManager enemyManager;
    public StageManager stageManager;
    public InputManager inputManager;
    public TowerManager towerManager;
    public FilledMapGenerator mapGenerator;
    public SkillManager skillManager;

    public GameObject TowerRaycastResult;
    private UnityEngine.RaycastHit hitPillar;
    private Vector3 hitArena;
    public Transform hitStore;

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

    private GameObject CheckTowerRaycast() {

        UnityEngine.Ray ray;
        UnityEngine.RaycastHit hit = new UnityEngine.RaycastHit();

        //Get Ray according to orientation
        if (Screen.width > Screen.height)
        {
            if (refCamL==null) return null;
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
    public Vector2 GetDragPos() { return StockPos; }
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
            case (int)Upgrades.StoreItems.BonusBoss1:
                enemyManager.SpawnBonusBoss(0, stageManager.GetPortalPosition(StageInfo.prng.Next(0, 2)));
                break;
            case (int)Upgrades.StoreItems.BonusBoss2:
                enemyManager.SpawnBonusBoss(1, stageManager.GetPortalPosition(StageInfo.prng.Next(0, 2)));
                break;
            case (int)Upgrades.StoreItems.BonusBoss3:
                enemyManager.SpawnBonusBoss(2, stageManager.GetPortalPosition(StageInfo.prng.Next(0, 2)));
                break;
            case (int)Upgrades.StoreItems.MagicMeteor:
                //CurrentSkill = 
                skillManager.MeteorSkill(hitPosition);
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.MagicBlizzard:
                //CurrentSkill = 
                skillManager.BlizzardSkill(hitPosition);
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.MagicMinions:
                //CurrentSkill = 
                skillManager.MinionsSkill(hitPosition);
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.MagicPetrification:
                //CurrentSkill = 
                    skillManager.PetrificationSkill(hitPosition);
                isSkillActive = true;
                break;
            default:
                break;
        }
    }

    public Vector3 RaycastTestDOTS(float3 fromPosition, float3 toPosition,int layer)
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
             hitStore= hit.transform;
        return hit.point;
    }

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
                if (hitPillar.transform != null && TowerInfo.infoUpdated)
                    towerManager.BuildTower(hitPillar.transform.gameObject);
            }
        }
        return hitPillar.point;
    }

    public bool StockCheckExist()
    {
        return (this.StockOperator != null);
    }

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
}
