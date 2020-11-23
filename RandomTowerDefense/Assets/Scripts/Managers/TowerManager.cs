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

    //[HideInInspector]
    //public List<GameObject> TowerNightmareList;
    //[HideInInspector]
    //public List<GameObject> TowerSoulEaterList;
    //[HideInInspector]
    //public List<GameObject> TowerTerrorBringerList;
    //[HideInInspector]
    //public List<GameObject> TowerUsurperList;

    [Header("TowerInfo Settings")]
    public List<GameObject> TargetInfo;
    private List<Text> TargetInfoText;
    public List<Slider> TargetInfoSlider;

    public ResourceManager resourceManager;
    public FilledMapGenerator filledMapGenerator;
    //public InGameOperation sceneManager;
    //public TutorialManager tutorialManager;
    public TowerSpawner towerSpawner;
    public CastleSpawner castleSpawner;

    public AudioManager audioManager;
    public AttackSpawner attackSpawner;

    // Start is called before the first frame update
    void Start()
    {
        //TowerNightmareList = new List<GameObject>();
        //TowerSoulEaterList = new List<GameObject>();
        //TowerTerrorBringerList = new List<GameObject>();
        //TowerUsurperList = new List<GameObject>();

        //resourceManager = FindObjectOfType<ResourceManager>();
        //filledMapGenerator = FindObjectOfType<FilledMapGenerator>();
        //sceneManager = FindObjectOfType<InGameOperation>();
        //towerSpawner = FindObjectOfType<TowerSpawner>();
        //tutorialManager = FindObjectOfType<TutorialManager>();
        //castleSpawner = FindObjectOfType<CastleSpawner>();

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
                TargetInfoText[i].text = "Rank" + (towerinfo.CheckMaxRank() ? "MAX" : towerinfo.rank.ToString())
                    + " Lv" + (towerinfo.CheckMaxLevel() ? "MAX" : towerinfo.level.ToString());

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
        BuildTower(pillar, location, rank);
    }
    public void BuildTower(GameObject pillar, Vector3 location, int rank = 1)
    {
        GameObject tower;
        int[] entityIDList;
        if (resourceManager.ChkAndBuild(rank) == false)
        {
            return;
        }

        {
            Tower script;

            switch (UnityEngine.Random.Range(0, NumTowerType))
            {
                case (int)TowerInfo.TowerInfoID.Enum_TowerNightmare:
                    entityIDList = towerSpawner.Spawn(rank - 1 + MonsterColorNumber * (int)TowerInfo.TowerInfoID.Enum_TowerNightmare,
                        location, castleSpawner.castle.transform.position);
                    tower = towerSpawner.GameObjects[entityIDList[0]];
                    script = tower.GetComponent<Tower>();
                    script.linkingManagers(towerSpawner, audioManager, attackSpawner, filledMapGenerator, resourceManager);
                    script.newTower(entityIDList[0], towerSpawner, pillar, TowerLevelUp, TowerNightmareAura,
                        TowerInfo.TowerInfoID.Enum_TowerNightmare, 1, rank);
                    tower.transform.localScale = 0.1f * new Vector3(1, 1, 1);
                    //TowerNightmareList.Add(tower);
                    break;
                case (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                    entityIDList = towerSpawner.Spawn(rank - 1 + MonsterColorNumber * (int)TowerInfo.TowerInfoID.Enum_TowerSoulEater,
                        location, castleSpawner.castle.transform.position);
                    tower = towerSpawner.GameObjects[entityIDList[0]];
                    script = tower.GetComponent<Tower>();
                    script.linkingManagers(towerSpawner, audioManager, attackSpawner, filledMapGenerator, resourceManager);
                    script.newTower(entityIDList[0], towerSpawner, pillar, TowerLevelUp, TowerSoulEaterAura,
                        TowerInfo.TowerInfoID.Enum_TowerSoulEater, 1, rank);
                    tower.transform.localScale = 0.1f * new Vector3(1, 1, 1);
                    //TowerSoulEaterList.Add(tower);
                    break;
                case (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                    entityIDList = towerSpawner.Spawn(rank - 1 + MonsterColorNumber * (int)TowerInfo.TowerInfoID.Enum_TowerTerrorBringer,
                        location, castleSpawner.castle.transform.position);
                    tower = towerSpawner.GameObjects[entityIDList[0]];
                    script = tower.GetComponent<Tower>();
                    script.linkingManagers(towerSpawner, audioManager, attackSpawner, filledMapGenerator, resourceManager);
                    script.newTower(entityIDList[0], towerSpawner, pillar, TowerLevelUp, TowerTerrorBringerAura,
                        TowerInfo.TowerInfoID.Enum_TowerTerrorBringer, 1, rank);
                    tower.transform.localScale = 0.1f * new Vector3(1, 1, 1);
                    //TowerTerrorBringerList.Add(tower);
                    break;
                case (int)TowerInfo.TowerInfoID.Enum_TowerUsurper:
                    entityIDList = towerSpawner.Spawn(rank - 1 + MonsterColorNumber * (int)TowerInfo.TowerInfoID.Enum_TowerUsurper,
                        location, castleSpawner.castle.transform.position);
                    tower = towerSpawner.GameObjects[entityIDList[0]];
                    script = tower.GetComponent<Tower>();
                    script.linkingManagers(towerSpawner, audioManager, attackSpawner, filledMapGenerator, resourceManager);
                    script.newTower(entityIDList[0], towerSpawner, pillar, TowerLevelUp, TowerUsurperAura,
                        TowerInfo.TowerInfoID.Enum_TowerUsurper, 1, rank);
                    tower.transform.localScale = 0.1f * new Vector3(1, 1, 1);
                    //TowerUsurperList.Add(tower);
                    break;
            }
        }
        GameObject.Instantiate<GameObject>(TowerBuild, location, Quaternion.identity);
    }

    public bool MergeTower(GameObject targetedTower)
    {
        //Check Type
        TowerInfo.TowerInfoID type;
        Tower towerScript = targetedTower.GetComponent<Tower>();

        //if (TowerNightmareList.Contains(targetedTower)) type = TowerInfo.TowerInfoID.Enum_TowerNightmare;
        //else if (TowerSoulEaterList.Contains(targetedTower)) type = TowerInfo.TowerInfoID.Enum_TowerSoulEater;
        //else if (TowerTerrorBringerList.Contains(targetedTower)) type = TowerInfo.TowerInfoID.Enum_TowerTerrorBringer;
        //else if (TowerUsurperList.Contains(targetedTower)) type = TowerInfo.TowerInfoID.Enum_TowerUsurper;
        //else return false;

        Tower targetedTowerScript = targetedTower.GetComponent<Tower>();
        //count same towers at max level
        List<GameObject> candidateList = new List<GameObject>();
        List<GameObject> tempList;
        int count = 0;
        //Find Candidates
        switch (targetedTowerScript.type)
        {
            case TowerInfo.TowerInfoID.Enum_TowerNightmare:
                switch (targetedTowerScript.rank) {
                    case 1:
                        tempList = new List<GameObject>(towerSpawner.TowerNightmareRank1);
                        break;
                    case 2:
                        tempList = new List<GameObject>(towerSpawner.TowerNightmareRank2);
                        break;
                    case 3:
                        tempList = new List<GameObject>(towerSpawner.TowerNightmareRank3);
                        break;
                    default:
                        tempList = new List<GameObject>();
                        break;
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
                switch (targetedTowerScript.rank)
                {
                    case 1:
                        tempList = new List<GameObject>(towerSpawner.TowerSoulEaterRank1);
                        break;
                    case 2:
                        tempList = new List<GameObject>(towerSpawner.TowerSoulEaterRank2);
                        break;
                    case 3:
                        tempList = new List<GameObject>(towerSpawner.TowerSoulEaterRank3);
                        break;
                    default:
                        tempList = new List<GameObject>();
                        break;
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
                switch (targetedTowerScript.rank)
                {
                    case 1:
                        tempList = new List<GameObject>(towerSpawner.TowerTerrorBringerRank1);
                        break;
                    case 2:
                        tempList = new List<GameObject>(towerSpawner.TowerTerrorBringerRank2);
                        break;
                    case 3:
                        tempList = new List<GameObject>(towerSpawner.TowerTerrorBringerRank3);
                        break;
                    default:
                        tempList = new List<GameObject>();
                        break;
                }
                break;
            case TowerInfo.TowerInfoID.Enum_TowerUsurper:
                switch (targetedTowerScript.rank)
                {
                    case 1:
                        tempList = new List<GameObject>(towerSpawner.TowerUsurperRank1);
                        break;
                    case 2:
                        tempList = new List<GameObject>(towerSpawner.TowerUsurperRank2);
                        break;
                    case 3:
                        tempList = new List<GameObject>(towerSpawner.TowerUsurperRank3);
                        break;
                    default:
                        tempList = new List<GameObject>();
                        break;
                }
                break;
            default:
                tempList = new List<GameObject>();
                break;
        }

        tempList.Remove(targetedTower);
        while (tempList.Count > 0)
        {
            int randNum = Random.Range(0, tempList.Count);
            GameObject chkTarget = tempList[randNum];
            tempList.Remove(chkTarget);
            if (chkTarget.activeSelf)
            {
                Tower chkTowerScript = chkTarget.GetComponent<Tower>();
                if (chkTowerScript.CheckLevel())
                {
                    candidateList.Add(chkTarget);
                    count++;
                }
            }
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
        //switch (targetedTowerScript.type)
        //{
        //    case TowerInfo.TowerInfoID.Enum_TowerNightmare:
        //        //TowerNightmareList.Remove(targetedTower);
        //        break;
        //    case TowerInfo.TowerInfoID.Enum_TowerSoulEater:
        //        //TowerSoulEaterList.Remove(targetedTower);
        //        break;
        //    case TowerInfo.TowerInfoID.Enum_TowerTerrorBringer:
        //        //TowerTerrorBringerList.Remove(targetedTower);
        //        break;
        //    case TowerInfo.TowerInfoID.Enum_TowerUsurper:
        //        //TowerUsurperList.Remove(targetedTower);
        //        break;
        //}
        targetedTowerScript.Destroy();
    }

    public void SellTower(Tower targetedTower)
    {
        if (resourceManager.SellTower(targetedTower))
        {
            filledMapGenerator.UpdatePillarStatus(targetedTower.gameObject,0);
            GameObject.Instantiate(TowerSell, targetedTower.transform.position, Quaternion.identity);
            removeTowerFromList(targetedTower.gameObject);
        }
    }
}
