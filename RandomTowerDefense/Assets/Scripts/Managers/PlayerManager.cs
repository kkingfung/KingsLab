using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private readonly float CancelDist = 20f;

    [HideInInspector]
    public bool isSkillActive;

    [HideInInspector]
    public bool isSelling;

    [Header("Camera Settings")]
    public Camera refCamL;
    public Camera refCamP;

    [Header("Skill Settings")]
    public float StockOperatorDepth;
    public GameObject FireSkillPrefab;
    public GameObject IceSkillPrefab;
    public GameObject MindSkillPrefab;
    public GameObject SummonSkillPrefab;

    public GameObject FireSkillAura;
    public GameObject IceSkillAura;

    [Header("Stock Settings")]
    public GameObject StockOperatorPrefab;
    private Vector2 StockPos;

    [HideInInspector]
    public bool isChecking;

    GameObject CurrentSkill;
    GameObject StockOperator;

    EnemyManager enemyManager;
    StageManager stageManager;

    TowerManager towerManager;
    FilledMapGenerator mapGenerator;

    private void Start()
    {
        enemyManager = FindObjectOfType<EnemyManager>();
        stageManager = FindObjectOfType<StageManager>();
        towerManager = FindObjectOfType<TowerManager>();
        mapGenerator = FindObjectOfType<FilledMapGenerator>();
    }

    private void Update()
    {
    }

    public Vector2 GetDragPos() { return StockPos; }
    public void CheckStock(Vector2 DragPos)
    {
        if (StockOperator != null) return;
        if (isSkillActive) return;

        isChecking = true;

        Camera targetCam = (Screen.width > Screen.height) ? refCamL : refCamP;

        StockOperator = Instantiate(StockOperatorPrefab, targetCam.ScreenToWorldPoint(new Vector3(DragPos.x, DragPos.y, StockOperatorDepth)), targetCam.transform.rotation);
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
            if ((Input.touches[0].position - DragPos).sqrMagnitude < CancelDist * CancelDist)
                return;
        }
        else
        {
            if ((new Vector2(Input.mousePosition.x, Input.mousePosition.y) - DragPos).sqrMagnitude < CancelDist * CancelDist)
                return;
        }

        int StockSelected = -1;
        //Check Action to be taken by DragPos && current mouse/touch
        float AngleCalculation;
        if (Input.touchCount > 0)
            AngleCalculation = (-90f + Mathf.Rad2Deg * Mathf.Atan2(Input.touches[0].position.y - DragPos.y, Input.touches[0].position.x - DragPos.x));
        else
            AngleCalculation = (-90f + Mathf.Rad2Deg * Mathf.Atan2(Input.mousePosition.y - DragPos.y, Input.mousePosition.x - DragPos.x));
        while (AngleCalculation < 0) AngleCalculation += 360f;

        if (AngleCalculation >= 18f && AngleCalculation < 90f)
            StockSelected = 2;
        else if (AngleCalculation >= 90f && AngleCalculation < 162f)
            StockSelected = 1;
        else if (AngleCalculation >= 162f && AngleCalculation < 234f)
            isSelling = true;
        else if (AngleCalculation >= 234f && AngleCalculation < 306f)
            StockSelected = 4;
        else StockSelected = 3;
        //Raycast test ground and camera ray
        Vector3 hitPosition = RaycastTest(LayerMask.GetMask("Arena"));
        if (isSelling) return;

        //For non-sale
        switch (SkillStack.UseStock(StockSelected))
        {
            case (int)Upgrades.StoreItems.BonusBoss1:
                enemyManager.SpawnBonusBoss(0, mapGenerator.CoordToPosition(stageManager.SpawnPoint[Random.Range(1, 3)]));
                break;
            case (int)Upgrades.StoreItems.BonusBoss2:
                enemyManager.SpawnBonusBoss(1, mapGenerator.CoordToPosition(stageManager.SpawnPoint[Random.Range(1, 3)]));
                break;
            case (int)Upgrades.StoreItems.BonusBoss3:
                enemyManager.SpawnBonusBoss(2, mapGenerator.CoordToPosition(stageManager.SpawnPoint[Random.Range(1, 3)]));
                break;
            case (int)Upgrades.StoreItems.MagicMeteor:
                CurrentSkill = GameObject.Instantiate(FireSkillPrefab, hitPosition, Quaternion.identity);
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.MagicBlizzard:
                CurrentSkill = GameObject.Instantiate(IceSkillPrefab, hitPosition, Quaternion.identity);
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.MagicSummon:
                CurrentSkill = GameObject.Instantiate(SummonSkillPrefab, hitPosition, Quaternion.identity);
                isSkillActive = true;
                break;
            case (int)Upgrades.StoreItems.MagicPetrification:
                CurrentSkill = GameObject.Instantiate(MindSkillPrefab, hitPosition, Quaternion.identity);
                isSkillActive = true;
                break;
            default:
                break;
        }
    }

    public Vector3 RaycastTest(int layer)
    {
        Ray ray = new Ray();
        RaycastHit hit = new RaycastHit();

        //Get Ray according to orientation
        if (Screen.width > Screen.height)
        {
            ray = refCamL.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }
        else
        {
            ray = refCamP.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }

        Physics.Raycast(ray, out hit, 1000, layer);
        return hit.point;
    }

    public Vector3 RaycastTest( bool isDoubleTap)
    {
        if (isDoubleTap == false && isSelling == false) return new Vector3();
        Ray ray = new Ray();
        RaycastHit hit = new RaycastHit();

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
        if (isSelling && Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Tower")))
        {
            towerManager.SellTower(hit.transform.gameObject);
        }
        else
        {
            if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Tower")))
            {
                towerManager.MergeTower(hit.transform.gameObject, hit.point);
            }
            if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Pillar")))
            {
                towerManager.BuildTower(hit.transform.gameObject);
            }
        }
        return hit.point;
    }

    public bool StockCheckExist()
    {
        return (this.StockOperator != null);
    }
}
