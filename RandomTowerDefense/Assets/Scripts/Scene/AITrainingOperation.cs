using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.RemoteConfig;
using System.IO;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.MapGenerator;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Units;
using RandomTowerDefense.Info;

namespace RandomTowerDefense.Scene
{
    /// <summary>
    /// AI訓練オペレーションクラス - ML-Agents AI訓練シーンの制御と管理
    ///
    /// 主な機能:
    /// - ML-Agentsトレーニング環境の自動設定と管理
    /// - プロシージャル生成マップでのAI学習環境構築
    /// - リモート設定統合によるハイパーパラメーター調整
    /// - タワー配置AIとエネミー戦略AIの競争学習システム
    /// - 訓練データ収集と学習進度追跡機能
    /// - ランダムマップ生成とバランス調整システム
    /// </summary>
    public class AITrainingOperation : InGameOperation
    {
        public TowerManager towerManager;
        public TowerSpawner towerSpawner;
        public FilledMapGenerator filledMapGenerator;

        [HideInInspector]
        public bool isFetchDone;

        [HideInInspector]
        public GameObject pillar;
        public bool tellMerge;

        private System.Random prng;

        protected override void Awake()
        {
            prng = new System.Random((int)Time.time);

            isFetchDone = false;
            // StageInfo(Base) Assignment
            IslandNow = 3;
            PlayerPrefs.SetInt("IslandNow", 3);
            PlayerPrefs.SetFloat("waveNum", 999);
            PlayerPrefs.SetFloat("stageSize", 64);
            PlayerPrefs.SetFloat("obstaclePercent", prng.Next(5, 10) * 0.1f);
            PlayerPrefs.SetFloat("hpMax", 9999);

            if (UseRemoteConfig)
            {
                ConfigManager.FetchCompleted += ApplyRemoteSettings;
                ConfigManager.FetchCompleted += StageInfoList.InitByRemote;
                ConfigManager.FetchCompleted += TowerInfo.InitByRemote;
                ConfigManager.FetchCompleted += EnemyInfo.InitByRemote;
                ConfigManager.FetchCompleted += SkillInfo.InitByRemote;
                ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
                if (Directory.Exists("Assets/AssetBundles"))
                {
                    StageInfoDetail.Init(true, "Assets/AssetBundles");
                }
                else
                {
                    StageInfoDetail.Init(false, null);
                }
            }
            else if (UseFileAsset)
            {
                if (Directory.Exists("Assets/AssetBundles"))
                {
                    StageInfoDetail.Init(true, "Assets/AssetBundles");
                    TowerInfo.InitByFile("Assets/AssetBundles/TowerInfo.txt");
                    EnemyInfo.InitByFile("Assets/AssetBundles/EnemyInfo.txt");
                    SkillInfo.InitByFile("Assets/AssetBundles/SkillInfo.txt");
                }
                else
                {
                    StageInfoDetail.Init(false, null);
                    TowerInfo.Init();
                    EnemyInfo.Init();
                    SkillInfo.Init();
                }
            }
            else
            {
                StageInfoDetail.Init(false, null);
                TowerInfo.Init();
                EnemyInfo.Init();
                SkillInfo.Init();
            }

            UpgradesManager.Init();
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
            {
                StageInfoDetail.Init(true, "Assets/AssetBundles");
            }
            else
            {
                StageInfoDetail.Init(false, null);
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            tellMerge = false;
        }

        // Update is called once per frame
        protected override void Update()
        {
            if (resourceManager.GetCurrMaterial() >= 100)
            {
                //Random Spawn Tower
                if (pillar && TowerInfo.infoUpdated)
                {
                    towerManager.BuildTower(pillar);
                    pillar = null;
                }
            }
            else if (tellMerge)
            {
                int count;
                List<GameObject> targetList;
                //Check three in a kind to merge
                switch (prng.Next(0, 4 * 4))
                {
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

                tellMerge = false;
                count = targetList.Count;
                if (count > 2)
                {
                    towerManager.MergeTower(targetList[prng.Next(0, count)]);

                }
            }
        }
    }
}
