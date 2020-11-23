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
    public FilledMapGenerator filledMapGenerator;

    private void Awake()
    {
        //StageInfo(Base) Assignment
        IslandNow = 3;
        PlayerPrefs.SetInt("IslandNow", 3);
        PlayerPrefs.SetFloat("waveNum", 999);
        PlayerPrefs.SetFloat("stageSize", 400);
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
        if (resourceManager.GetCurrMaterial() >= 150)
        {
            //Random Spawn Tower
            GameObject pillar = GetRandomFreePillar(true);
            if (pillar)
                towerManager.BuildTower(pillar);
        }
        else {
            int count;
            //Check three in a kind to merge
            switch (Random.Range(0, 4)) {
                case 0:
                    count = towerManager.TowerNightmareList.Count;
                    if (count > 2)
                    towerManager.MergeTower(towerManager.TowerNightmareList[Random.Range(0, count)]);
                    break;
                case 1:
                    count = towerManager.TowerSoulEaterList.Count;
                    if (count > 2)
                        towerManager.MergeTower(towerManager.TowerSoulEaterList[Random.Range(0, count)]);
                    break;
                case 2:
                    count = towerManager.TowerTerrorBringerList.Count;
                    if (count > 2)
                        towerManager.MergeTower(towerManager.TowerTerrorBringerList[Random.Range(0, count)]);
                    break;
                case 3:
                    count = towerManager.TowerUsurperList.Count;
                    if (count > 2)
                        towerManager.MergeTower(towerManager.TowerUsurperList[Random.Range(0, count)]);
                    break;
            }
        }
    }

    private GameObject GetRandomFreePillar(bool isFree) {
        GameObject targetPillar = null;

        int cnt = 0;
        while (targetPillar == null && cnt< filledMapGenerator.PillarList.Count) {
            int id = Random.Range(0, filledMapGenerator.PillarList.Count);
            if (filledMapGenerator.PillarList[id].state == (isFree?0:1))
                return filledMapGenerator.PillarList[id].obj;
            cnt++;
        }
        return null;
    }
}