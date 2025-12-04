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
        #region Public Properties

        [Header("AI Training Components")]
        [SerializeField] public TowerManager towerManager;
        [SerializeField] public TowerSpawner towerSpawner;
        [SerializeField] public FilledMapGenerator filledMapGenerator;

        [HideInInspector] public bool isFetchDone;
        [HideInInspector] public GameObject pillar;
        [SerializeField] public bool tellMerge;

        #endregion

        #region Private Fields

        private System.Random _prng;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// 初期化処理 - AIトレーニング環境セットアップと設定管理
        /// </summary>
        protected override void Awake()
        {
            InitializeRandomGenerator();
            InitializeTrainingSettings();
            InitializeConfigurationSystems();
        }

        /// <summary>
        /// スタート処理 - AIトレーニング開始状態設定
        /// </summary>
        private void Start()
        {
            tellMerge = false;
        }

        /// <summary>
        /// 毎フレーム更新 - AIトレーニング自動タワー建設とマージ処理
        /// </summary>
        protected override void Update()
        {
            HandleAutomaticTowerBuilding();
            HandleAutomaticTowerMerging();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 乱数ジェネレーター初期化
        /// </summary>
        private void InitializeRandomGenerator()
        {
            _prng = new System.Random((int)Time.time);
        }

        /// <summary>
        /// AIトレーニング設定初期化
        /// </summary>
        private void InitializeTrainingSettings()
        {
            isFetchDone = false;
            IslandNow = 3;
            PlayerPrefs.SetInt("IslandNow", 3);
            PlayerPrefs.SetFloat("waveNum", 999);
            PlayerPrefs.SetFloat("stageSize", 64);
            PlayerPrefs.SetFloat("obstaclePercent", _prng.Next(5, 10) * 0.1f);
            PlayerPrefs.SetFloat("hpMax", 9999);
        }

        /// <summary>
        /// 設定システム初期化
        /// </summary>
        private void InitializeConfigurationSystems()
        {
            if (UseRemoteConfig)
            {
                SetupRemoteConfiguration();
            }
            else if (UseFileAsset)
            {
                SetupFileAssetConfiguration();
            }
            else
            {
                SetupDefaultConfiguration();
            }
        }

        /// <summary>
        /// リモート設定セットアップ
        /// </summary>
        private void SetupRemoteConfiguration()
        {
            ConfigManager.FetchCompleted += ApplyRemoteSettings;
            ConfigManager.FetchCompleted += TowerInfo.InitByRemote;
            ConfigManager.FetchCompleted += EnemyInfo.InitByRemote;
            ConfigManager.FetchCompleted += SkillInfo.InitByRemote;
            ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
            InitializeStageInfoDetail();
        }

        /// <summary>
        /// ファイルアセット設定セットアップ
        /// </summary>
        private void SetupFileAssetConfiguration()
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
                SetupDefaultConfiguration();
            }
        }

        /// <summary>
        /// デフォルト設定セットアップ
        /// </summary>
        private void SetupDefaultConfiguration()
        {
            StageInfoDetail.Init(false, null);
            TowerInfo.Init();
            EnemyInfo.Init();
            SkillInfo.Init();
        }

        /// <summary>
        /// ステージ情報詳細初期化
        /// </summary>
        private void InitializeStageInfoDetail()
        {
            if (Directory.Exists("Assets/AssetBundles"))
            {
                StageInfoDetail.Init(true, "Assets/AssetBundles");
            }
            else
            {
                StageInfoDetail.Init(false, null);
            }
        }

        /// <summary>
        /// 自動タワー建設処理
        /// </summary>
        private void HandleAutomaticTowerBuilding()
        {
            if (resourceManager.GetCurrMaterial() >= 100)
            {
                if (pillar && TowerInfo.InfoUpdated)
                {
                    towerManager.BuildTower(pillar);
                    pillar = null;
                }
            }
        }

        /// <summary>
        /// 自動タワーマージ処理
        /// </summary>
        private void HandleAutomaticTowerMerging()
        {
            if (!tellMerge) return;

            List<GameObject> targetList = GetRandomTowerList();
            tellMerge = false;

            if (targetList.Count > 2)
            {
                GameObject randomTower = targetList[_prng.Next(0, targetList.Count)];
                towerManager.MergeTower(randomTower);
            }
        }

        /// <summary>
        /// ランダムタワーリスト取得
        /// </summary>
        /// <returns>選択されたタワーリスト</returns>
        private List<GameObject> GetRandomTowerList()
        {
            int selection = _prng.Next(0, 16); // 4 types × 4 ranks = 16 possibilities

            return selection switch
            {
                0 => new List<GameObject>(towerSpawner.TowerNightmareRank1),
                1 => new List<GameObject>(towerSpawner.TowerNightmareRank2),
                2 => new List<GameObject>(towerSpawner.TowerNightmareRank3),
                3 => new List<GameObject>(towerSpawner.TowerNightmareRank4),
                4 => new List<GameObject>(towerSpawner.TowerSoulEaterRank1),
                5 => new List<GameObject>(towerSpawner.TowerSoulEaterRank2),
                6 => new List<GameObject>(towerSpawner.TowerSoulEaterRank3),
                7 => new List<GameObject>(towerSpawner.TowerSoulEaterRank4),
                8 => new List<GameObject>(towerSpawner.TowerTerrorBringerRank1),
                9 => new List<GameObject>(towerSpawner.TowerTerrorBringerRank2),
                10 => new List<GameObject>(towerSpawner.TowerTerrorBringerRank3),
                11 => new List<GameObject>(towerSpawner.TowerTerrorBringerRank4),
                12 => new List<GameObject>(towerSpawner.TowerUsurperRank1),
                13 => new List<GameObject>(towerSpawner.TowerUsurperRank2),
                14 => new List<GameObject>(towerSpawner.TowerUsurperRank3),
                15 => new List<GameObject>(towerSpawner.TowerUsurperRank4),
                _ => new List<GameObject>()
            };
        }

        /// <summary>
        /// リモート設定適用処理
        /// </summary>
        /// <param name="configResponse">設定レスポンス</param>
        private void ApplyRemoteSettings(ConfigResponse configResponse)
        {
            switch (configResponse.requestOrigin)
            {
                case ConfigOrigin.Default:
                    ApplyDefaultOrFileSettings();
                    break;
                case ConfigOrigin.Cached:
                    // キャッシュされた設定は何も処理しない
                    break;
                case ConfigOrigin.Remote:
                    ApplyRemoteConfigSettings(configResponse);
                    break;
            }

            InitializeStageInfoDetail();
        }

        /// <summary>
        /// デフォルトまたはファイル設定適用
        /// </summary>
        private void ApplyDefaultOrFileSettings()
        {
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
        }

        /// <summary>
        /// リモート設定適用
        /// </summary>
        /// <param name="configResponse">設定レスポンス</param>
        private void ApplyRemoteConfigSettings(ConfigResponse configResponse)
        {
            TowerInfo.InitByRemote(configResponse);
            EnemyInfo.InitByRemote(configResponse);
            SkillInfo.InitByRemote(configResponse);
        }

        #endregion
    }
}
