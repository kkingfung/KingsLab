using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.RemoteConfig;
using System.IO;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Common;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Info;
using RandomTowerDefense.Systems;
using RandomTowerDefense.Units;
using RandomTowerDefense.FileSystem;

namespace RandomTowerDefense.Scene
{
    /// <summary>
    /// インゲームオペレーションクラス - メインゲームシーンの統合制御システム
    ///
    /// 主な機能:
    /// - ゲームプレイカメラ制御と視点切り替えシステム（Arena、TopLeft、Top、TopRight）
    /// - リアルタイムUI管理（島名表示、ゴールド表示、矢印ナビゲーション）
    /// - タワー建設・売却・合成システムとリソース管理統合
    /// - 魔法スキルシステム（Meteor、Blizzard、Petrification、Minions）管理
    /// - チュートリアルモード制御と進行管理
    /// - ファイル更新機能とリモート設定統合
    /// - 石化エフェクト視覚システムとマテリアル制御
    /// - ゲーム状態追跡とシーン遷移制御
    /// </summary>
    public class InGameOperation : ISceneChange
    {
        #region Constants

        /// <summary>
        /// チュートリアル時間調整因子
        /// </summary>
        private const float TUTORIAL_TIME_FACTOR = 1f; // 通常時間係数（デバッグ用：0.2f）

        /// <summary>
        /// 基本床マテリアルサイズ
        /// </summary>
        private const int BASIC_FLOOR_MATERIAL_SIZE = 3;

        /// <summary>
        /// 初期島番号
        /// </summary>
        private const int INITIAL_ISLAND_INDEX = 0;

        /// <summary>
        /// 初期有効島数
        /// </summary>
        private const int INITIAL_ENABLED_ISLANDS = 1;

        /// <summary>
        /// 初期画面表示位置
        /// </summary>
        private const int INITIAL_SCREEN_POSITION = 0;

        /// <summary>
        /// ダブルヒット防止待機時間（秒）
        /// </summary>
        private const float TIME_WAIT_FOR_DOUBLE_HIT_PREVENTION = 0.01f;

        /// <summary>
        /// 最大カメラ位置変更時間（秒）
        /// </summary>
        private const float MAX_CAMERA_POSITION_CHANGE_TIME = 0.2f;

        #endregion

        public enum ScreenShownID
        {
            SSIDArena = 0,
            SSIDTopLeft = 1,
            SSIDTop = 2,
            SSIDTopRight = 3,
        }

        /// <summary>
        /// カメラ開始/停止/終了ポイント設定
        /// </summary>
        [Header("MainCamera Settings")]
        public GameObject MainCam;
        public List<Vector3> MainCamStayPt;
        public List<Vector3> MainCamRotationAngle;
        /// <summary>
        /// ストアグループ（メインカメラと連動）
        /// </summary>
        public GameObject StoreGp;
        public GameObject DarkenCam;

        [Header("UI Settings")]
        public List<Text> UIIslandName;
        public List<Text> UICurrentGold;

        [Header("Button Settings")]
        public List<Button> OptionButton;

        [Header("Arrow Settings")]
        public List<GameObject> UIUpArrow;
        public List<GameObject> UIDownArrow;
        public List<GameObject> UILeftArrow;
        public List<GameObject> UIRightArrow;

        [Header("Petrify Settings")]
        public List<Image> PetrifyImgs;
        public List<RawImage> PetrifyRImgs;
        public List<SpriteRenderer> PetrifySpr;
        public List<Material> PetrifyMat;

        [Header("FileUpdate Settings")]
        [SerializeField]
        public bool UseFileAsset;
        [SerializeField]
        public bool UseRemoteConfig;


        [Header("FadeObj Settings")]
        public MeshRenderer LandscapeFadeImg;
        public MeshRenderer PortraitFadeImg;

        private FadeEffect LandscapeFade;
        private FadeEffect PortraitFade;

        [Header("Other Settings")]
        public GameObject Agent;
        public Material FloorMat;
        public Material ObstacleMat;
        public MeshRenderer ArenaEffectQuad;
        private Material ArenaEffectMat;
        public List<Image> AimMarkImg;

        /// <summary>
        /// インゲームデバッガー用フラグ（未使用）
        /// </summary>
        //public bool isDebugging;

        /// <summary>
        /// チュートリアルフラグ（第1ステージのみ）
        /// </summary>
        private bool isTutorial;

        /// <summary>
        /// ボリュームプロファイル（予備）
        /// </summary>
        //public VolumeProfile volumeProfile;

        /// <summary>
        /// 現在の島番号（海と空の色変更用）
        /// </summary>
        protected int IslandNow = INITIAL_ISLAND_INDEX;

        /// <summary>
        /// 有効な島の数（勝利判定用）
        /// </summary>
        protected int IslandEnabled = INITIAL_ENABLED_ISLANDS;

        [HideInInspector]
        /// <summary>
        /// 現在表示中の画面（0:メイン、1:左上、2:上、3:右上）
        /// </summary>
        public int currScreenShown;

        [HideInInspector]
        /// <summary>
        /// 次に表示する画面
        /// </summary>
        public int nextScreenShown = INITIAL_SCREEN_POSITION;
        private bool isScreenChanging = false;

        [Header("Manager Linkages")]
        /// <summary>
        /// ゲーム管理システム群
        /// </summary>
        public AudioManager AudioManager;
        public CameraManager CameraManager;
        public CanvaManager CanvaManager;
        public InputManager InputManager;
        public GyroscopeManager GyroscopeManager;
        public ResourceManager resourceManager;
        public WaveManager waveManager;
        public TimeManager timeManager;
        public TutorialManager tutorialManager;
        public ScoreCalculation scoreCalculation;
        public RecordManager recordManager;

        /// <summary>
        /// ダブルヒット防止用時間記録
        /// </summary>
        private float TimeRecord = 0;

        /// <summary>
        /// シーン変更待機フラグ
        /// </summary>
        private bool WaitSceneChg;

        /// <summary>
        /// リモート設定構造体定義
        /// </summary>
        public struct userAttributes { }
        public struct appAttributes { }

        private void OnDestroy()
        {
            PlayerPrefs.Save();
        }
        protected override void Awake()
        {
            base.Awake();
            currScreenShown = (int)ScreenShownID.SSIDArena;

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
            else
            {
                waveManager.doneDownloading = true;
                if (UseFileAsset)
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
            }

            SkillStack.Init();
            UpgradesManager.Init();

            IslandNow = PlayerPrefs.GetInt("IslandNow", INITIAL_ISLAND_INDEX);
            IslandEnabled = PlayerPrefs.GetInt("IslandEnabled", INITIAL_ENABLED_ISLANDS);
            isTutorial = (IslandNow == 0);
            WaitSceneChg = false;
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

            waveManager.doneDownloading = true;
        }

        // Start is called before the first frame update
        private void Start()
        {
            base.SceneIn();
            MainCam.transform.position = MainCamStayPt[0];
            MainCam.transform.rotation = Quaternion.Euler(MainCamRotationAngle[0]);

            DarkenCam.SetActive(false);

            LandscapeFade = LandscapeFadeImg.gameObject.GetComponent<FadeEffect>();
            PortraitFade = PortraitFadeImg.gameObject.GetComponent<FadeEffect>();

            ArenaEffectMat = ArenaEffectQuad.material;

            //InputManager = FindObjectOfType<InputManager>();
            //AudioManager = FindObjectOfType<AudioManager>();
            //CameraManager = FindObjectOfType<CameraManager>();
            //CanvaManager = FindObjectOfType<CanvaManager>();
            //GyroscopeManager = FindObjectOfType<GyroscopeManager>();
            //waveManager = FindObjectOfType<WaveManager>();
            //resourceManager = FindObjectOfType<ResourceManager>();
            //timeManager = FindObjectOfType<TimeManager>();
            //scoreCalculation = FindObjectOfType<ScoreCalculation>();
            //tutorialManager = FindObjectOfType<TutorialManager>();

            if (tutorialManager)
            {
                if (isTutorial == false)
                    tutorialManager.SetTutorialStage(TutorialManager.TutorialStageID.TutorialProgress_FreeBattle);
                else
                    Destroy(Agent);
            }

            foreach (Material mat in PetrifyMat)
            {
                mat.SetFloat("_Progress", 0);
            }

            if (PetrifyImgs.Count > 0)
            {
                foreach (Image i in PetrifyImgs)
                    i.material.SetFloat("_Progress", 0);
            }
            if (PetrifyRImgs.Count > 0)
            {
                foreach (RawImage i in PetrifyRImgs)
                    i.material.SetFloat("_Progress", 0);
            }
            if (PetrifySpr.Count > 0)
            {
                foreach (SpriteRenderer i in PetrifySpr)
                    i.material.SetFloat("_Progress", 0);
            }

            FloorMat.SetFloat("_ShapesSides", BasicFloorMatSize + IslandNow);
            foreach (Text i in UIIslandName)
            {
                switch (IslandNow)
                {
                    case 0:
                        i.text = "ヒジリカ島";
                        FloorMat.SetColor("_Color", new Color(0.34f, 1f, 0f));
                        ObstacleMat.SetColor("_Color", new Color(0.34f, 1f, 0f));
                        ArenaEffectMat.SetColor("_BaseColor", new Color(0.34f, 1f, 0f));
                        break;
                    case 1:
                        i.text = "テンシュキ島";
                        FloorMat.SetColor("_Color", new Color(0.82f, 0.47f, 1f));
                        ObstacleMat.SetColor("_Color", new Color(0.82f, 0.47f, 1f));
                        ArenaEffectMat.SetColor("_BaseColor", new Color(0.82f, 0.47f, 1f));
                        break;
                    case 2:
                        i.text = "ニモハサ島";
                        FloorMat.SetColor("_Color", new Color(1f, 0.2f, 0f));
                        ObstacleMat.SetColor("_Color", new Color(1f, 0.2f, 0f));
                        ArenaEffectMat.SetColor("_BaseColor", new Color(1f, 0.2f, 0f));
                        break;
                    case 3:
                        i.text = "ギイシカ島";
                        FloorMat.SetColor("_Color", new Color(1f, 0.7f, 0f));
                        ObstacleMat.SetColor("_Color", new Color(1f, 0.7f, 0f));
                        ArenaEffectMat.SetColor("_BaseColor", new Color(1f, 0.7f, 0f));
                        break;
                }
            }

            toDrag = -1;
            isOption = false;

        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            foreach (Text i in UICurrentGold)
            {
                i.text = resourceManager.GetCurrMaterial().ToString() + "G";
            }

            if (!isOption)
            {
                ArrowOperation();
                ChangeScreenShownByGyro();
                ChangeScreenShownByDrag();
            }

            if (tutorialManager && isTutorial)
            {
                if (tutorialManager.WaitingResponds)
                {
                    if (timeManager.timeFactor != tutorialTimeFactor)
                    {
                        timeManager.timeFactor = tutorialTimeFactor;
                        timeManager.TimeControl();
                    }
                }
            }

            foreach (Button i in OptionButton)
                i.interactable = !isSceneFinished;

            //Change Scene
            if (isSceneFinished && ((LandscapeFade && LandscapeFade.isReady) || (PortraitFade && PortraitFade.isReady)))
            {
                if (UseRemoteConfig)
                {
                    ConfigManager.FetchCompleted -= ApplyRemoteSettings;
                    //ConfigManager.FetchCompleted -= StageInfoList.InitByRemote;
                    //ConfigManager.FetchCompleted -= TowerInfo.InitByRemote;
                    //ConfigManager.FetchCompleted -= EnemyInfo.InitByRemote;
                    //ConfigManager.FetchCompleted -= SkillInfo.InitByRemote;
                }
                SceneManager.LoadScene("LoadingScene");
                return;
            }

            //if (tutorialManager == null || (tutorialManager.GetTutorialStage()> TutorialManager.TutorialStageID.TutorialProgress_StoreSkill
            //    ||(tutorialManager.GetTutorialStage() == TutorialManager.TutorialStageID.TutorialProgress_StoreSkill && tutorialManager.GetTutorialStageProgress() > 8)))//8:referred to TutorialManager(varies)
            if (isScreenChanging == false)
            {
                Vector3 angle = CameraManager.GyroCamGp[0].transform.rotation.eulerAngles;
                while (angle.x > 180) angle.x -= 360;
                while (angle.x < -180) angle.x += 360;
                if (angle.x > 0) { currScreenShown = (int)ScreenShownID.SSIDArena; nextScreenShown = (int)ScreenShownID.SSIDArena; }
                else
                {
                    while (angle.y > 180) angle.y -= 360;
                    while (angle.y < -180) angle.y += 360;
                    if (angle.y > -60 && angle.y < 60)
                    {
                        currScreenShown = (int)ScreenShownID.SSIDTop; nextScreenShown = (int)ScreenShownID.SSIDTop;
                    }
                    else if (angle.y > -180 && angle.y < -60)
                    {
                        currScreenShown = (int)ScreenShownID.SSIDTopLeft; nextScreenShown = (int)ScreenShownID.SSIDTopLeft;
                    }
                    else if (angle.y > 60 && angle.y < 180)
                    {
                        currScreenShown = (int)ScreenShownID.SSIDTopRight; nextScreenShown = (int)ScreenShownID.SSIDTopRight;
                    }
                }
            }
        }

        public void AutoResetGyro()
        {
            MainCam.transform.localEulerAngles = MainCamRotationAngle[currScreenShown];
        }

        private void LateUpdate()
        {
            foreach (Image i in AimMarkImg)
                i.enabled = !isOption;
        }
        #region CommonOperation
        public void ArrowOperation()
        {
            //0:Main, 1:Top-Left, 2:Top, 3:Top-Right
            switch ((ScreenShownID)currScreenShown)
            {
                case ScreenShownID.SSIDArena:
                    foreach (GameObject i in UIUpArrow) i.SetActive(true);
                    foreach (GameObject i in UIDownArrow) i.SetActive(false);
                    foreach (GameObject i in UILeftArrow) i.SetActive(false);
                    foreach (GameObject i in UIRightArrow) i.SetActive(false);
                    break;
                case ScreenShownID.SSIDTopLeft:
                    foreach (GameObject i in UIUpArrow) i.SetActive(false);
                    foreach (GameObject i in UIDownArrow) i.SetActive(true);
                    foreach (GameObject i in UILeftArrow) i.SetActive(true);
                    foreach (GameObject i in UIRightArrow) i.SetActive(true);
                    break;
                case ScreenShownID.SSIDTop:
                    foreach (GameObject i in UIUpArrow) i.SetActive(false);
                    foreach (GameObject i in UIDownArrow) i.SetActive(true);
                    foreach (GameObject i in UILeftArrow) i.SetActive(true);
                    foreach (GameObject i in UIRightArrow) i.SetActive(true);
                    break;
                case ScreenShownID.SSIDTopRight:
                    foreach (GameObject i in UIUpArrow) i.SetActive(false);
                    foreach (GameObject i in UIDownArrow) i.SetActive(true);
                    foreach (GameObject i in UILeftArrow) i.SetActive(true);
                    foreach (GameObject i in UIRightArrow) i.SetActive(true);
                    break;
            }
        }

        public void MoveToStage(int SceneID)
        {
            if (WaitSceneChg)
            {
                return;
            }
            if (Time.time - TimeRecord < TIME_WAIT_FOR_DOUBLE_HIT_PREVENTION)
            {
                return;
            }
            if (scoreCalculation.Inputting)
            {
                return;
            }
            WaitSceneChg = true;
            StartCoroutine(PetrifyAnimation(SceneID));
            TimeRecord = Time.time;

        }

        public void OptionStatus()
        {
            if (Time.time - TimeRecord < TIME_WAIT_FOR_DOUBLE_HIT_PREVENTION)
            {
                return;
            }
            if (timeManager.timeFactor == 0)
            {
                return;
            }
            isOption = !isOption;
            CanvaManager.isOption = isOption;
            TimeRecord = Time.time;
            timeManager.TimeControl();
            InputManager.TapTimeRecord = 0;
            InputManager.isDragging = false;
            DarkenCam.SetActive(isOption);
            AudioManager.PlayAudio("se_Button");
        }

        private IEnumerator PetrifyAnimation(int sceneID)
        {
            float progress = 0f;
            int frame = 15;
            float rate = 1 / (float)frame;
            while (frame-- > 0)
            {
                progress += rate;
                foreach (Image i in PetrifyImgs)
                {
                    //i.material.EnableKeyword("_Progress");
                    i.material.SetFloat("_Progress", progress);
                }
                foreach (RawImage i in PetrifyRImgs)
                {
                    //i.material.EnableKeyword("_Progress");
                    i.material.SetFloat("_Progress", progress);
                }
                foreach (SpriteRenderer i in PetrifySpr)
                {
                    //i.material.EnableKeyword("_Progress");
                    i.material.SetFloat("_Progress", progress);
                }
                yield return new WaitForSeconds(0f);
            }

            SetNextScene(sceneID == 0 ? "GameScene" : "StageSelection");

            PlayerPrefs.SetInt("IslandNow", IslandNow);
            SceneOut();
            isSceneFinished = true;
        }
        #endregion


        #region CameraOperation
        public void ChangeScreenShownByButton(int chgValue)
        {
            if (isOption) return;
            //DownArrow:0,LeftArrow:1,UpArrow:2 ,RightArrow:3
            switch (chgValue)
            {
                case 0:
                    if (currScreenShown == (int)InGameOperation.ScreenShownID.SSIDArena) return;
                    nextScreenShown = (int)InGameOperation.ScreenShownID.SSIDArena;
                    break;
                case 1:
                    if (currScreenShown == (int)ScreenShownID.SSIDTop || currScreenShown == (int)ScreenShownID.SSIDTopRight) nextScreenShown = currScreenShown - 1;
                    else
                        if (currScreenShown == (int)ScreenShownID.SSIDTopLeft) nextScreenShown = (int)ScreenShownID.SSIDTopRight;
                    else
                        return;
                    break;
                case 2:
                    if (currScreenShown == (int)ScreenShownID.SSIDArena) nextScreenShown = (int)ScreenShownID.SSIDTop;
                    else return;
                    break;
                case 3:
                    if (currScreenShown == (int)ScreenShownID.SSIDTopLeft || currScreenShown == (int)ScreenShownID.SSIDTop) nextScreenShown = currScreenShown + 1;
                    else
                        if (currScreenShown == (int)ScreenShownID.SSIDTopRight) nextScreenShown = (int)ScreenShownID.SSIDTopLeft;
                    else
                        return;
                    break;
                default:
                    return;
            }

            GyroscopeManager.GyroModify();

            if (nextScreenShown != currScreenShown)
            {
                isScreenChanging = true;
                StartCoroutine(ChangeScreenShown());
            }
        }

        private void ChangeScreenShownByGyro()
        {
            if (isScreenChanging) return;
            //Gyroscope Operation
            if (GyroscopeManager.LeftShake)
                ChangeScreenShownByButton(1);
            if (GyroscopeManager.RightShake)
                ChangeScreenShownByButton(3);
            if (GyroscopeManager.VerticalShake)
                ChangeScreenShownByButton(currScreenShown == (int)ScreenShownID.SSIDArena ? (int)ScreenShownID.SSIDTop : (int)ScreenShownID.SSIDArena);
        }

        private void ChangeScreenShownByDrag()
        {
            if (isScreenChanging) return;

            //Touch Operation
            switch (toDrag)
            {
                default: break;
                case 0:
                case 1:
                case 2:
                case 3:
                    ChangeScreenShownByButton(toDrag);
                    break;
            }

            toDrag = -1;
        }

        private IEnumerator ChangeScreenShown()
        {
            Vector3 spd;
            float timer = 0;
            Vector3 ori = CameraManager.GyroCamGp[0].transform.rotation.eulerAngles;
            MainCam.transform.localEulerAngles = ori;
            GyroscopeManager.ResetReference();

            spd = MainCamRotationAngle[nextScreenShown] - ori;

            while (spd.y > 180f) spd.y -= 360f;
            while (spd.y < -180f) spd.y += 360f;
            while (spd.x > 180f) spd.x -= 360f;
            while (spd.x < -180f) spd.x += 360f;
            while (spd.z > 180f) spd.z -= 360f;
            while (spd.z < -180f) spd.z += 360f;

            spd /= MAX_CAMERA_POSITION_CHANGE_TIME;

            while (timer < MAX_CAMERA_POSITION_CHANGE_TIME)
            {
                InputManager.isDragging = false;
                timer += Time.deltaTime;
                if (timer > MAX_CAMERA_POSITION_CHANGE_TIME) timer = MAX_CAMERA_POSITION_CHANGE_TIME;
                MainCam.transform.localEulerAngles = MainCamRotationAngle[nextScreenShown] - spd * (MAX_CAMERA_POSITION_CHANGE_TIME - timer);
                yield return new WaitForSeconds(0f);
            }

            MainCam.transform.localEulerAngles = MainCamRotationAngle[nextScreenShown];

            currScreenShown = nextScreenShown;
            isScreenChanging = false;
            GyroscopeManager.SetTempInactive();
        }

        #endregion

        #region GameProcessing
        public int GetCurrIsland() { return IslandNow; }
        public int GetEnabledIsland() { return IslandEnabled; }

        public bool CheckIfTutorial() { return isTutorial; }
        #endregion
    }
}