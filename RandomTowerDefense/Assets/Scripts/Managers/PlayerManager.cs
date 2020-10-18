using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private readonly int NumReqToMerge = 3;
    private readonly float CancelDist = 5f;

    [Header("Tower Settings")]
    public GameObject TowerNightmare;
    public GameObject TowerSoulEater;
    public GameObject TowerTerrorBringer;
    public GameObject TowerUsurper;

    public GameObject TowerBuild;
    public GameObject TowerLevelUp;
    public GameObject TowerDisappear;
    public GameObject TowerSell;

    [Header("TowerAtk Settings")]
    public GameObject TowerNightmareAtk;
    public GameObject TowerSoulEaterAtk;
    public GameObject TowerTerrorBringerAtk;
    public GameObject TowerUsurperAtk;

    [Header("TowerAura Settings")]
    public GameObject TowerNightmareAura;
    public GameObject TowerSoulEaterAura;
    public GameObject TowerTerrorBringerAura;
    public GameObject TowerUsurperAura;

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


    [Header("Stock Settings")]
    public GameObject StockOperatorPrefab;

    bool isChecking;

    [HideInInspector]
    public List<GameObject> TowerNightmareList;
    [HideInInspector]
    public List<GameObject> TowerSoulEaterList;
    [HideInInspector]
    public List<GameObject> TowerTerrorBringerList;
    [HideInInspector]
    public List<GameObject> TowerUsurperList;

    GameObject CurrentSkill;

    EnemyManager enemyManager;
    StageManager stageManager;
    ResourceManager resourceManager;

    // Start is called before the first frame update
    void Awake()
    {
    }

    private void Start()
    {
        TowerNightmareList=new List<GameObject>();
        TowerSoulEaterList = new List<GameObject>();
        TowerTerrorBringerList = new List<GameObject>();
        TowerUsurperList = new List<GameObject>();

        enemyManager = FindObjectOfType<EnemyManager>();
        stageManager = FindObjectOfType<StageManager>();
        resourceManager = FindObjectOfType<ResourceManager>();
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

    private void BuildTower(Vector3 location,int rank=1) {
        GameObject tower;
        if (resourceManager.ChkAndBuild(rank) ==false) {
            return;
        }
        switch (UnityEngine.Random.Range(0,4)) {
            case 0:
                tower = Instantiate(TowerNightmare, location, Quaternion.identity);
                tower.GetComponent<Tower>().newTower(TowerNightmareAtk, TowerLevelUp, TowerNightmareAura, 0, 1,1);
                TowerNightmareList.Add(tower);
                break;
            case 1:
                tower = Instantiate(TowerSoulEater, location, Quaternion.identity);
                tower.GetComponent<Tower>().newTower(TowerSoulEaterAtk, TowerLevelUp, TowerSoulEaterAura, 1, 1, 1);
                TowerSoulEaterList.Add(tower);
                break;
            case 2:
                tower = Instantiate(TowerTerrorBringer, location, Quaternion.identity);
                tower.GetComponent<Tower>().newTower(TowerTerrorBringerAtk, TowerLevelUp, TowerTerrorBringerAura, 2, 1, 1);
                TowerTerrorBringerList.Add(tower);
                break;
            case 3:
                tower = Instantiate(TowerUsurper, location, Quaternion.identity);
                tower.GetComponent<Tower>().newTower(TowerUsurperAtk, TowerLevelUp, TowerUsurperAura, 3, 1, 1);
                TowerUsurperList.Add(tower);
                break;
        }
        GameObject.Instantiate<GameObject>(TowerBuild, location, Quaternion.identity);
    }

    private bool MergeTower(GameObject targetedTower,Vector3 spawnPoint)
    {
        //Check Type
        int type;
        if (TowerNightmareList.Contains(targetedTower)) type = 0;
        else if (TowerSoulEaterList.Contains(targetedTower)) type = 1;
        else if (TowerTerrorBringerList.Contains(targetedTower)) type = 2;
        else if (TowerUsurperList.Contains(targetedTower)) type = 3;
        else return false;

        //count same towers at max level
        List<GameObject> candidateList = new List<GameObject>();
        List<int> candidateID = new List<int>();
        List<GameObject> tempList;
        int count = 0;
        //Find Candidates
        switch (targetedTower.GetComponent<Tower>().type)
        {
            case 0:
                tempList = new List<GameObject>(TowerNightmareList);
                tempList.Remove(targetedTower);
                while (count < 2 && tempList.Count>0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList.ElementAt(randNum);
                    if (chkTarget.GetComponent<Tower>().CheckMaxLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;

                    }
                    tempList.Remove(chkTarget);
                }
                break;
            case 1:
                tempList = new List<GameObject>(TowerSoulEaterList);
                tempList.Remove(targetedTower);
                while (count < 2 && tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList.ElementAt(randNum);
                    if (chkTarget.GetComponent<Tower>().CheckMaxLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;

                    }
                    tempList.Remove(chkTarget);
                }
                break;
            case 2:
                tempList = new List<GameObject>(TowerTerrorBringerList);
                tempList.Remove(targetedTower);
                while (count < 2 && tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList.ElementAt(randNum);
                    if (chkTarget.GetComponent<Tower>().CheckMaxLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;

                    }
                    tempList.Remove(chkTarget);
                }
                break;
            case 3:
                tempList = new List<GameObject>(TowerUsurperList);
                tempList.Remove(targetedTower);
                while (count < 2 && tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList.ElementAt(randNum);
                    if (chkTarget.GetComponent<Tower>().CheckMaxLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;

                    }
                    tempList.Remove(chkTarget);
                }
                break;
        }

        if (count < 2) return false;

        //Remove Candidates
        foreach (GameObject i in candidateList) {
            GameObject.Instantiate<GameObject>(TowerDisappear,i.transform.position, Quaternion.identity);

            removeTowerFromList(i);
        }
        removeTowerFromList(targetedTower);

        //Build 
        BuildTower(targetedTower.transform.position, targetedTower.GetComponent<Tower>().rank+1);

        return true;
    }

    private void removeTowerFromList(GameObject targetedTower) {
        switch (targetedTower.GetComponent<Tower>().type)
        {
            case 0:
                TowerNightmareList.Remove(targetedTower);
                break;
            case 1:
                TowerSoulEaterList.Remove(targetedTower);
                break;
            case 2:
                TowerTerrorBringerList.Remove(targetedTower);
                break;
            case 3:
                TowerUsurperList.Remove(targetedTower);
                break;
        }
        Destroy(targetedTower);
    }

    private void SellTower(GameObject targetedTower) {
        if (resourceManager.SellTower(targetedTower)) {
            GameObject.Instantiate(TowerSell,targetedTower.transform.position,Quaternion.identity);
            removeTowerFromList(targetedTower);
        }
        isSelling = false;
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
                SellTower(hit.transform.gameObject);
            }
            else {
                if (Physics.Raycast(ray, out hit, 1000, LayerMask.NameToLayer("Tower")))
                {
                    MergeTower(hit.transform.gameObject, hit.point);
                }
                else {
                    BuildTower(hit.point);
                }
             }
            return hit.point;
        }

        return new Vector3();
    }
}
