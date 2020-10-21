using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    private readonly int NumReqToMerge = 3;
    private readonly int MonsterColorNumber = 4;
    private readonly int NumTowerType = 4;

    [Header("Tower Settings")]
    public List<GameObject> TowerNightmare;
    public List<GameObject> TowerSoulEater;
    public List<GameObject> TowerTerrorBringer;
    public List<GameObject> TowerUsurper;

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
    public List<GameObject> TowerNightmareList;
    [HideInInspector]
    public List<GameObject> TowerSoulEaterList;
    [HideInInspector]
    public List<GameObject> TowerTerrorBringerList;
    [HideInInspector]
    public List<GameObject> TowerUsurperList;

    PlayerManager playerManager;
    ResourceManager resourceManager;

    // Start is called before the first frame update
    void Start()
    {
        TowerNightmareList = new List<GameObject>();
        TowerSoulEaterList = new List<GameObject>();
        TowerTerrorBringerList = new List<GameObject>();
        TowerUsurperList = new List<GameObject>();

        playerManager = FindObjectOfType<PlayerManager>();
        resourceManager = FindObjectOfType<ResourceManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BuildTower(Vector3 location, int rank = 1)
    {
        GameObject tower;
        if (resourceManager.ChkAndBuild(rank) == false)
        {
            return;
        }
        switch (UnityEngine.Random.Range(0, NumTowerType))
        {
            case (int)TowerInfo.TowerInfoID.Enum_TowerNightmare:
                tower = Instantiate(TowerNightmare[rank-1], location, Quaternion.identity);
                tower.GetComponent<Tower>().newTower(TowerNightmareAtk, TowerLevelUp, TowerNightmareAura,
                    TowerInfo.TowerInfoID.Enum_TowerNightmare, 1, 1);
                TowerNightmareList.Add(tower);
                break;
            case (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                tower = Instantiate(TowerSoulEater[rank - 1], location, Quaternion.identity);
                tower.GetComponent<Tower>().newTower(TowerSoulEaterAtk, TowerLevelUp, TowerSoulEaterAura,
                    TowerInfo.TowerInfoID.Enum_TowerSoulEater, 1, 1);
                TowerSoulEaterList.Add(tower);
                break;
            case (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                tower = Instantiate(TowerTerrorBringer[rank - 1], location, Quaternion.identity);
                tower.GetComponent<Tower>().newTower(TowerTerrorBringerAtk, TowerLevelUp, TowerTerrorBringerAura,
                    TowerInfo.TowerInfoID.Enum_TowerTerrorBringer, 1, 1);
                TowerTerrorBringerList.Add(tower);
                break;
            case (int)TowerInfo.TowerInfoID.Enum_TowerUsurper:
                tower = Instantiate(TowerUsurper[rank - 1], location, Quaternion.identity);
                tower.GetComponent<Tower>().newTower(TowerUsurperAtk, TowerLevelUp, TowerUsurperAura,
                    TowerInfo.TowerInfoID.Enum_TowerUsurper, 1, 1);
                TowerUsurperList.Add(tower);
                break;
        }
        GameObject.Instantiate<GameObject>(TowerBuild, location, Quaternion.identity);
    }

    public bool MergeTower(GameObject targetedTower, Vector3 spawnPoint)
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
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                tempList = new List<GameObject>(TowerNightmareList);
                tempList.Remove(targetedTower);
                while (count < NumReqToMerge-1 && tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList[randNum];
                    if (chkTarget.GetComponent<Tower>().CheckMaxLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;

                    }
                    tempList.Remove(chkTarget);
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                tempList = new List<GameObject>(TowerSoulEaterList);
                tempList.Remove(targetedTower);
                while (count < NumReqToMerge - 1 && tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList[randNum];
                    if (chkTarget.GetComponent<Tower>().CheckMaxLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;

                    }
                    tempList.Remove(chkTarget);
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                tempList = new List<GameObject>(TowerTerrorBringerList);
                tempList.Remove(targetedTower);
                while (count < NumReqToMerge - 1 && tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList[randNum];
                    if (chkTarget.GetComponent<Tower>().CheckMaxLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;

                    }
                    tempList.Remove(chkTarget);
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                tempList = new List<GameObject>(TowerUsurperList);
                tempList.Remove(targetedTower);
                while (count < NumReqToMerge - 1 && tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList[randNum];
                    if (chkTarget.GetComponent<Tower>().CheckMaxLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;

                    }
                    tempList.Remove(chkTarget);
                }
                break;
        }

        if (count < NumReqToMerge - 1) return false;

        //Remove Candidates
        foreach (GameObject i in candidateList)
        {
            GameObject.Instantiate<GameObject>(TowerDisappear, i.transform.position, Quaternion.identity);

            removeTowerFromList(i);
        }
        removeTowerFromList(targetedTower);

        //Build 
        BuildTower(targetedTower.transform.position, targetedTower.GetComponent<Tower>().rank + 1);

        return true;
    }

    private void removeTowerFromList(GameObject targetedTower)
    {
        switch (targetedTower.GetComponent<Tower>().type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                TowerNightmareList.Remove(targetedTower);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                TowerSoulEaterList.Remove(targetedTower);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                TowerTerrorBringerList.Remove(targetedTower);
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                TowerUsurperList.Remove(targetedTower);
                break;
        }
        Destroy(targetedTower);
    }

    public void SellTower(GameObject targetedTower)
    {
        if (resourceManager.SellTower(targetedTower))
        {
            GameObject.Instantiate(TowerSell, targetedTower.transform.position, Quaternion.identity);
            removeTowerFromList(targetedTower);
        }
        playerManager.isSelling = false;
    }
}
