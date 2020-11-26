using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.RemoteConfig;
using System.IO;

public class AITrainingOperation : InGameOperation
{
    public TowerManager towerManager;
    public TowerSpawner towerSpawner;
    public FilledMapGenerator filledMapGenerator;

    [HideInInspector]
    public GameObject pillar;

    private void Awake()
    {
        //StageInfo(Base) Assignment
        IslandNow = 3;
        PlayerPrefs.SetInt("IslandNow", 3);
        PlayerPrefs.SetFloat("waveNum", 999);
        PlayerPrefs.SetFloat("stageSize", 100);
        PlayerPrefs.SetFloat("hpMax", 9999);

        if (UseRemoteConfig)
        {
            ConfigManager.FetchCompleted += ApplyRemoteSettings;
            //ConfigManager.FetchCompleted += StageInfo.InitByRemote;
            ConfigManager.FetchCompleted += TowerInfo.InitByRemote;
            ConfigManager.FetchCompleted += EnemyInfo.InitByRemote;
            ConfigManager.FetchCompleted += SkillInfo.InitByRemote;
            ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
            if (Directory.Exists("Assets/AssetBundles"))
            {
                StageInfo.Init(true, "Assets/AssetBundles");
            }
            else
            {
                StageInfo.Init(false, null);
            }
            Debug.Log("UpdatedByRemoteConfig");
        }
        else if (UseFileAsset)
        {
            if (Directory.Exists("Assets/AssetBundles"))
            {
                StageInfo.Init(true, "Assets/AssetBundles");
                TowerInfo.InitByFile("Assets/AssetBundles/TowerInfo.txt");
                EnemyInfo.InitByFile("Assets/AssetBundles/EnemyInfo.txt");
                SkillInfo.InitByFile("Assets/AssetBundles/SkillInfo.txt");
                Debug.Log("UpdatedByFileAsset");
            }
            else
            {
                StageInfo.Init(false, null);
                TowerInfo.Init();
                EnemyInfo.Init();
                SkillInfo.Init();
                Debug.Log("UpdatedByScriptInput");
            }
        }
        else
        {
            StageInfo.Init(false, null);
            TowerInfo.Init();
            EnemyInfo.Init();
            SkillInfo.Init();
            Debug.Log("UpdatedByScriptInput");
        }

        Upgrades.init();
    }

    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        // レスポンス元に応じて設定を更新する
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                if (UseFileAsset && Directory.Exists("Assets/AssetBundles"))
                {
                    TowerInfo.InitByFile("Assets/AssetBundles/TowerInfo.txt");
                    EnemyInfo.InitByFile("Assets/AssetBundles/EnemyInfo.txt");
                    SkillInfo.InitByFile("Assets/AssetBundles/SkillInfo.txt");
                }
                else
                {
                    TowerInfo.Init();
                    EnemyInfo.Init();
                    SkillInfo.Init();
                }
                break;
            case ConfigOrigin.Cached:
                break;
            case ConfigOrigin.Remote:
                TowerInfo.InitByRemote(configResponse);
                EnemyInfo.InitByRemote(configResponse);
                SkillInfo.InitByRemote(configResponse);
                break;
        }

        if (UseFileAsset && Directory.Exists("Assets/AssetBundles"))
            StageInfo.Init(true, "Assets/AssetBundles");
        else
            StageInfo.Init(false, null);
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (resourceManager.GetCurrMaterial() >= 120)
        {
            //Random Spawn Tower
            if (pillar)
            {
                towerManager.BuildTower(pillar);
                pillar = null;
            }
        }
        else if(Random.Range(0,300)==0){
            int count;
            List<GameObject> targetList;
            //Check three in a kind to merge
            switch (Random.Range(0, 4*4)) {
                case 0 + 4 * 0:
                    targetList = new List<GameObject>(towerSpawner.TowerNightmareRank1);
                    break;
                case 1 + 4 * 0:
                    targetList = new List<GameObject>(towerSpawner.TowerNightmareRank2);
                    break;
                case 2 + 4 * 0:
                    targetList = new List<GameObject>(towerSpawner.TowerNightmareRank3);
                    break;
                case 3 + 4 * 0:
                    targetList = new List<GameObject>(towerSpawner.TowerNightmareRank4);
                    break;
                case 0 + 4 * 1:
                    targetList = new List<GameObject>(towerSpawner.TowerSoulEaterRank1);
                    break;
                case 1 + 4 * 1:
                    targetList = new List<GameObject>(towerSpawner.TowerSoulEaterRank2);
                    break;
                case 2 + 4 * 1:
                    targetList = new List<GameObject>(towerSpawner.TowerSoulEaterRank3);
                    break;
                case 3 + 4 * 1:
                    targetList = new List<GameObject>(towerSpawner.TowerSoulEaterRank4);
                    break;
                case 0 + 4 * 2:
                    targetList = new List<GameObject>(towerSpawner.TowerTerrorBringerRank1);
                    break;
                case 1 + 4 * 2:
                    targetList = new List<GameObject>(towerSpawner.TowerTerrorBringerRank2);
                    break;
                case 2 + 4 * 2:
                    targetList = new List<GameObject>(towerSpawner.TowerTerrorBringerRank3);
                    break;
                case 3 + 4 * 2:
                    targetList = new List<GameObject>(towerSpawner.TowerTerrorBringerRank4);
                    break;
                case 0 + 4 * 3:
                    targetList = new List<GameObject>(towerSpawner.TowerUsurperRank1);
                    break;
                case 1 + 4 * 3:
                    targetList = new List<GameObject>(towerSpawner.TowerUsurperRank2);
                    break;
                case 2 + 4 * 3:
                    targetList = new List<GameObject>(towerSpawner.TowerUsurperRank3);
                    break;
                case 3 + 4 * 3:
                    targetList = new List<GameObject>(towerSpawner.TowerUsurperRank4);
                    break;
                default:
                    targetList = new List<GameObject>();
                    break;
            }
            count = targetList.Count;
            if (count > 2)
                towerManager.MergeTower(targetList[Random.Range(0, count)]);
        }
    }
}