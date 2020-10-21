using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private readonly float CancelDist = 5f;

    [HideInInspector]
    public bool isSkillActive;

    [HideInInspector]
    public bool isSelling;

    [Header("Camera Settings")]
    public Camera refCamL;
    public Camera refCamP;

    [Header("Skill Settings")]
    public GameObject FireSkillPrefab;
    public GameObject IceSkillPrefab;
    public GameObject MindSkillPrefab;
    public GameObject SummonSkillPrefab;

    public GameObject FireSkillAura;
    public GameObject IceSkillAura;

    [Header("Stock Settings")]
    public GameObject StockOperatorPrefab;

    bool isChecking;

    GameObject CurrentSkill;

    EnemyManager enemyManager;
    StageManager stageManager;

    TowerManager towerManager;

    private void Start()
    {
        enemyManager = FindObjectOfType<EnemyManager>();
        stageManager = FindObjectOfType<StageManager>();
        towerManager = FindObjectOfType<TowerManager>();
    }

    public void CheckStock(Vector2 DragPos) 
    {
        if (isChecking) return;
        if (isSkillActive) return;

        isChecking = true;
    }

    public void UseStock(Vector2 DragPos)
    {
        if (!isChecking) return;
        if (isSkillActive) return;

        if (Input.touchCount > 0 && (Input.touches[0].position - DragPos).sqrMagnitude < CancelDist * CancelDist) {
            isChecking = false;
            return; 
        }

        int StockSelected = -1;
        //Check Action to be taken by DragPos && current mouse/touch
        float AngleCalculation = Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(Input.touches[0].position - DragPos, Vector2.up));
        if (AngleCalculation > 72f * -2f && AngleCalculation <= 72f * -1f)
            StockSelected = 0;
        else if (AngleCalculation > 72f * -1f && AngleCalculation <= 0f)
            StockSelected = 1;
        else if (AngleCalculation > 0f && AngleCalculation <= 72f)
            StockSelected = 2;
        else if (AngleCalculation > 72f && AngleCalculation <= 72f * 2f)
            StockSelected = 3;
        else isSelling=true;
        //Raycast test ground and camera ray
        Vector3 hitPosition = RaycastTest(LayerMask.NameToLayer("Arena"),false);
        if (isSelling) return;

            //For non-sale
        switch (SkillStack.UseStock(StockSelected)) {
            case (int)Upgrades.StoreItems.BonusBoss1:
                enemyManager.SpawnBonusBoss(0, stageManager.SpawnPoint);
                break;
            case (int)Upgrades.StoreItems.BonusBoss2:
                enemyManager.SpawnBonusBoss(1, stageManager.SpawnPoint);
                break;
            case (int)Upgrades.StoreItems.BonusBoss3:
                enemyManager.SpawnBonusBoss(2, stageManager.SpawnPoint);
                break;
            case (int)Upgrades.StoreItems.MagicMeteor:
                CurrentSkill=GameObject.Instantiate(FireSkillPrefab, hitPosition,Quaternion.identity);
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
        }
    }

    public Vector3 RaycastTest(LayerMask layer,bool isDoubleTap)
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
        if (Physics.Raycast(ray, out hit, 1000, layer))
        {
            if (isSelling && Physics.Raycast(ray, out hit, 1000, LayerMask.NameToLayer("Tower"))) {
                towerManager.SellTower(hit.transform.gameObject);
            }
            else {
                if (Physics.Raycast(ray, out hit, 1000, LayerMask.NameToLayer("Tower")))
                {
                    towerManager.MergeTower(hit.transform.gameObject, hit.point);
                }
                else {
                    towerManager.BuildTower(hit.point);
                }
             }
            return hit.point;
        }

        return new Vector3();
    }
}
