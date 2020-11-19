using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class TowerManager : MonoBehaviour
{
    private readonly int NumReqToMerge = 3;
    private readonly int MonsterColorNumber = 4;
    private readonly int NumTowerType = 4;

    [Header("Tower Settings")]
    //ToTowerSpawner
    //public List<GameObject> TowerNightmare;
    //public List<GameObject> TowerSoulEater;
    //public List<GameObject> TowerTerrorBringer;
    //public List<GameObject> TowerUsurper;

    public GameObject TowerBuild;
    public GameObject TowerLevelUp;
    public GameObject TowerDisappear;
    public GameObject TowerSell;

    //[Header("TowerAtk Settings")]
    //public GameObject TowerNightmareAtk;
    //public GameObject TowerSoulEaterAtk;
    //public GameObject TowerTerrorBringerAtk;
    //public GameObject TowerUsurperAtk;

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

    [Header("TowerInfo Settings")]
    public List<GameObject> TargetInfo;
    private List<Text> TargetInfoText;
    public List<Slider> TargetInfoSlider;

    private ResourceManager resourceManager;
    private FilledMapGenerator filledMapGenerator;
    //private InGameOperation sceneManager;
    //private TutorialManager tutorialManager;
    private TowerSpawner towerSpawner;

    // Start is called before the first frame update
    void Start()
    {
        TowerNightmareList = new List<GameObject>();
        TowerSoulEaterList = new List<GameObject>();
        TowerTerrorBringerList = new List<GameObject>();
        TowerUsurperList = new List<GameObject>();

        resourceManager = FindObjectOfType<ResourceManager>();
        filledMapGenerator = FindObjectOfType<FilledMapGenerator>();
        //sceneManager = FindObjectOfType<InGameOperation>();
        towerSpawner = FindObjectOfType<TowerSpawner>();
        //tutorialManager = FindObjectOfType<TutorialManager>();

        TargetInfoText = new List<Text>();
        foreach (GameObject i in TargetInfo)
        {
            TargetInfoText.Add(i.GetComponent<Text>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateInfo(GameObject target)
    {
        for(int i = 0; i < TargetInfo.Count; ++i) {
            TargetInfo[i].SetActive(target != null);
            foreach (Slider j in TargetInfoSlider)
            {

                j.transform.gameObject.SetActive(target != null);
            }
            if (target == null) continue;

            if (TargetInfoText[i])
            {
                Tower towerinfo = target.GetComponent<Tower>();
                TargetInfoText[i].text = "Rank" + towerinfo.rank + " Lv" + towerinfo.level;
                switch (towerinfo.type)
                {
                    case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                        TargetInfoText[i].color = Color.yellow;
                        break;
                    case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                        TargetInfoText[i].color = Color.grey;
                        break;
                    case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                        TargetInfoText[i].color = Color.cyan;
                        break;
                    case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                        TargetInfoText[i].color = Color.magenta;
                        break;
                }
                foreach (Slider j in TargetInfoSlider)
                {
                    j.maxValue = towerinfo.RequiredExp();
                    j.value= Mathf.Min(towerinfo.exp, j.maxValue);
                }
            }
        }
    }

    public void BuildTower(GameObject pillar, int rank = 1)
    {
        if (rank == 1 && filledMapGenerator.ChkPillarStatusEmpty(pillar) == false) return;

        Vector3 location = pillar.transform.position + Vector3.up * filledMapGenerator.UpdatePillarStatus(pillar);
        BuildTower(pillar,location, rank);
    }
        public void BuildTower(GameObject pillar, Vector3 location, int rank = 1)
    {
        GameObject tower;
        int[] entityIDList;
        if (resourceManager.ChkAndBuild(rank) == false)
        {
            return;
        }
        #region SpareCodeForTutorial
        /*
        if (sceneManager.CheckIfTutorial() && tutorialManager && tutorialManager.FreeToBuild == false)
        {
            if (towerSpawner.GameObjects.Length > 0)
            {
                int type = (int)towerSpawner.GameObjects[0].GetComponent<Tower>().type;
                entityIDList = towerSpawner.Spawn(rank - 1 + MonsterColorNumber * type,
                       location, new Unity.Mathematics.float3(), 0, float.MaxValue, float.MaxValue);
                tower = towerSpawner.GameObjects[entityIDList[0]];
                tower.transform.localScale = 0.1f * new Vector3(1, 1, 1);

                switch (type)
                {
                    case (int)TowerInfo.TowerInfoID.Enum_TowerNightmare:
                        tower.GetComponent<Tower>().newTower(entityIDList[0], pillar, TowerLevelUp, TowerNightmareAura,
                            TowerInfo.TowerInfoID.Enum_TowerNightmare, 1, rank);
                        TowerNightmareList.Add(tower);
                        break;
                    case (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                        tower.GetComponent<Tower>().newTower(entityIDList[0], pillar, TowerLevelUp, TowerSoulEaterAura,
                             TowerInfo.TowerInfoID.Enum_TowerSoulEater, 1, rank);
                        TowerSoulEaterList.Add(tower);
                        break;
                    case (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                        tower.GetComponent<Tower>().newTower(entityIDList[0], pillar, TowerLevelUp, TowerTerrorBringerAura,
                               TowerInfo.TowerInfoID.Enum_TowerTerrorBringer, 1, rank);
                        TowerTerrorBringerList.Add(tower);
                        break;
                    case (int)TowerInfo.TowerInfoID.Enum_TowerUsurper:
                        tower.GetComponent<Tower>().newTower(entityIDList[0], pillar, TowerLevelUp, TowerUsurperAura,
                            TowerInfo.TowerInfoID.Enum_TowerUsurper, 1, rank);
                        TowerUsurperList.Add(tower);
                        break;
                }
            }
        }
        else
        */
        #endregion
        {
            switch (UnityEngine.Random.Range(0, NumTowerType))
            {
                case (int)TowerInfo.TowerInfoID.Enum_TowerNightmare:
                    entityIDList = towerSpawner.Spawn(rank - 1 + MonsterColorNumber * (int)TowerInfo.TowerInfoID.Enum_TowerNightmare,
                        location);
                    tower = towerSpawner.GameObjects[entityIDList[0]];
                    tower.GetComponent<Tower>().newTower(entityIDList[0], towerSpawner, pillar, TowerLevelUp, TowerNightmareAura,
                        TowerInfo.TowerInfoID.Enum_TowerNightmare, 1, rank);
                    tower.transform.localScale = 0.1f * new Vector3(1, 1, 1);
                    TowerNightmareList.Add(tower);
                    break;
                case (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                    entityIDList = towerSpawner.Spawn(rank - 1 + MonsterColorNumber * (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater,
                        location);
                    tower = towerSpawner.GameObjects[entityIDList[0]];
                    tower.GetComponent<Tower>().newTower(entityIDList[0], towerSpawner, pillar, TowerLevelUp, TowerSoulEaterAura,
                        TowerInfo.TowerInfoID.Enum_TowerSoulEater, 1, rank);
                    tower.transform.localScale = 0.1f * new Vector3(1, 1, 1);
                    TowerSoulEaterList.Add(tower);
                    break;
                case (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                    entityIDList = towerSpawner.Spawn(rank - 1 + MonsterColorNumber * (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer,
                        location);
                    tower = towerSpawner.GameObjects[entityIDList[0]];
                    tower.GetComponent<Tower>().newTower(entityIDList[0], towerSpawner, pillar, TowerLevelUp, TowerTerrorBringerAura,
                        TowerInfo.TowerInfoID.Enum_TowerTerrorBringer, 1, rank);
                    tower.transform.localScale = 0.1f * new Vector3(1, 1, 1);
                    TowerTerrorBringerList.Add(tower);
                    break;
                case (int)TowerInfo.TowerInfoID.Enum_TowerUsurper:
                    entityIDList = towerSpawner.Spawn(rank - 1 + MonsterColorNumber * (int)TowerInfo.TowerInfoID.Enum_TowerUsurper,
                        location);
                    tower = towerSpawner.GameObjects[entityIDList[0]];
                    tower.GetComponent<Tower>().newTower(entityIDList[0], towerSpawner, pillar, TowerLevelUp, TowerUsurperAura,
                        TowerInfo.TowerInfoID.Enum_TowerUsurper, 1, rank);
                    tower.transform.localScale = 0.1f * new Vector3(1, 1, 1);
                    TowerUsurperList.Add(tower);
                    break;
            }
        }
        GameObject temp = Instantiate<GameObject>(TowerBuild, location, Quaternion.identity);
        Destroy(temp, 5.0f);
    }

    public bool MergeTower(GameObject targetedTower, Vector3 spawnPoint)
    {
        //Check Type
        TowerInfo.TowerInfoID type;
        if (TowerNightmareList.Contains(targetedTower)) type = TowerInfo.TowerInfoID.Enum_TowerNightmare;
        else if (TowerSoulEaterList.Contains(targetedTower)) type = TowerInfo.TowerInfoID.Enum_TowerSoulEater;
        else if (TowerTerrorBringerList.Contains(targetedTower)) type = TowerInfo.TowerInfoID.Enum_TowerTerrorBringer;
        else if (TowerUsurperList.Contains(targetedTower)) type = TowerInfo.TowerInfoID.Enum_TowerUsurper;
        else return false;

        Tower targetedTowerScript = targetedTower.GetComponent<Tower>();
        //count same towers at max level
        List<GameObject> candidateList = new List<GameObject>();
        List<GameObject> tempList;
        int count = 0;
        //Find Candidates
        switch (type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                tempList = new List<GameObject>(TowerNightmareList);
                tempList.Remove(targetedTower);
                while (tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList[randNum];
                    tempList.Remove(chkTarget);
                    Tower chkTowerScript = chkTarget.GetComponent<Tower>();
                    if (chkTowerScript.rank!= targetedTowerScript.rank)
                        continue;
                    else if (chkTowerScript.CheckLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;
                    }
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                tempList = new List<GameObject>(TowerSoulEaterList);
                tempList.Remove(targetedTower);
                while (tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList[randNum];
                    tempList.Remove(chkTarget);
                    Tower chkTowerScript = chkTarget.GetComponent<Tower>();
                    if (chkTowerScript.rank != targetedTowerScript.rank)
                        continue;
                    else if (chkTowerScript.CheckLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;
                    }
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                tempList = new List<GameObject>(TowerTerrorBringerList);
                tempList.Remove(targetedTower);
                while (tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList[randNum];
                    tempList.Remove(chkTarget);
                    Tower chkTowerScript = chkTarget.GetComponent<Tower>();
                    if (chkTowerScript.rank != targetedTowerScript.rank)
                        continue;
                    else if (chkTowerScript.CheckLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;
                    }
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                tempList = new List<GameObject>(TowerUsurperList);
                tempList.Remove(targetedTower);
                while (tempList.Count > 0)
                {
                    int randNum = Random.Range(0, tempList.Count);
                    GameObject chkTarget = tempList[randNum];
                    tempList.Remove(chkTarget);
                    Tower chkTowerScript = chkTarget.GetComponent<Tower>();
                    if (chkTowerScript.rank != targetedTowerScript.rank)
                        continue;
                    else if (chkTowerScript.CheckLevel())
                    {
                        candidateList.Add(chkTarget);
                        count++;
                    }
                }
                break;
        }

        if (count < NumReqToMerge - 1) return false;

        count = NumReqToMerge-1;
        //Remove Candidates
        while (count-- > 0) {
            int randCandidate = Random.Range(0, candidateList.Count);
            GameObject candidate = candidateList[randCandidate];
            candidateList.Remove(candidate);
            GameObject temp = Instantiate<GameObject>(TowerDisappear, candidate.transform.position, Quaternion.identity);
            VisualEffect tempVFX = temp.GetComponent<VisualEffect>();
            Tower candidateTowerScript = candidate.GetComponent<Tower>();
            switch (candidateTowerScript.type)
            {
                case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                    tempVFX.SetVector4("MainColor", new Vector4(1, 1, 0, 1));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                    tempVFX.SetVector4("MainColor", new Vector4(0, 1, 0, 1));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                    tempVFX.SetVector4("MainColor", new Vector4(0, 0, 1, 1));
                    break;
                case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                    tempVFX.SetVector4("MainColor", new Vector4(1, 0, 0, 1));
                    break;
            }
            tempVFX.SetVector3("TargetLocation", targetedTower.transform.position);
            StartCoroutine(WaitToKillVFX(temp, 5, 10));
            removeTowerFromList(candidate);
        }
        removeTowerFromList(targetedTower);

        //Build 
        BuildTower(targetedTowerScript.pillar,targetedTower.transform.position, targetedTowerScript.rank + 1);

        return true;
    }

    private void removeTowerFromList(GameObject targetedTower)
    {
        Tower targetedTowerScript = targetedTower.GetComponent<Tower>();
        switch (targetedTowerScript.type)
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
        targetedTowerScript.Destroy();
    }

    public void SellTower(Tower targetedTower)
    {
        if (resourceManager.SellTower(targetedTower))
        {
            filledMapGenerator.UpdatePillarStatus(targetedTower.gameObject,0);
            GameObject temp = Instantiate(TowerSell, targetedTower.transform.position, Quaternion.identity);
           // Destroy(temp,10.0f);
            removeTowerFromList(targetedTower.gameObject);
        }
    }
    private IEnumerator WaitToKillVFX(GameObject targetVFX, int waittime, int killtime)
    {
        int frame = waittime;
        while (frame-- > 0)
        {
            yield return new WaitForSeconds(0f);
        }
        targetVFX.GetComponent<VisualEffect>().Stop();
        Destroy(targetVFX,killtime);
    }

}
