using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Common;
using RandomTowerDefense.FileSystem;

namespace RandomTowerDefense.Scene
{
    /// <summary>
    /// タイトル画面オペレーションクラス - タイトルシーンのUI制御とメニュー管理
    ///
    /// 主な機能:
    /// - マルチカメラシステム制御（右カメラ、下カメラ、エッグカメラ）
    /// - タイトルメニューボタン管理（スタート、オプション、クレジット）
    /// - VFXエフェクト制御とタイトル画面演出
    /// - クロスプラットフォーム対応UI切り替え（縦横画面）
    /// - シーン遷移とフェードエフェクト統合
    /// </summary>
    public class TitleOperation : ISceneChange
    {
        #region Constants

        private const float _timeWait = 0.2f;
        private const float _targetCamAngle = -45f;
        private const int _maxRecStatus = 4;
        private const float _maxRecTimer = 0.5f;
        private const float _maxRecWaitTimer = 5;

        #endregion

        #region Enums

        /// <summary>
        /// カメラ状態管理列挙型
        /// </summary>
        private enum RecordCameraState
        {
            StarttoStay,
            Stay,
            StaytoExit,
            Exit
        }

        #endregion

        #region Serialized Fields

        [Header("Camera Settings")]
        [SerializeField] public GameObject DarkenCam;
        [SerializeField] public List<GameObject> EggCam;
        [SerializeField] public GameObject RightCam;
        [SerializeField] public List<Vector3> RightCamStartPt;
        [SerializeField] public List<Vector3> RightCamStayPt;
        [SerializeField] public List<Vector3> RightCamEndPt;
        [SerializeField] public GameObject BottomCam;
        [SerializeField] public List<Vector3> BottomCamStartPt;
        [SerializeField] public List<Vector3> BottomCamStayPt;
        [SerializeField] public List<Vector3> BottomCamEndPt;

        [Header("Button Settings")]
        [SerializeField] public List<Button> StartButton;
        [SerializeField] public List<Button> OptionButton;
        [SerializeField] public List<Button> CreditButton;

        [Header("Petrify Settings")]
        [SerializeField] public List<Image> PetrifyImgs;
        [SerializeField] public List<RawImage> PetrifyRImgs;
        [SerializeField] public List<SpriteRenderer> PetrifySpr;
        [SerializeField] public List<Material> PetrifyMat;

        [Header("Title Settings")]
        [SerializeField] public List<GameObject> TitleImg;
        [SerializeField] public List<GameObject> TitleEffectImg;
        [SerializeField] public List<string> bundleUrl;
        [SerializeField] public List<string> bundleName;
        [SerializeField] public GameObject BoidSpawn;
        [SerializeField] public RawImage LandscapeFadeImg;
        [SerializeField] public RawImage PortraitFadeImg;

        #endregion

        #region Private Fields

        private Camera _rightCamComponent;
        private Camera _bottomCamComponent;
        private List<RawImage> _titleImgRaw;
        private List<Text> _titleImgText;
        private List<VisualEffect> _titleEffectImgVFX;
        private FadeEffect _landscapeFade;
        private FadeEffect _portraitFade;

        private bool _isOpening;
        private bool _isWaiting;
        private bool _showCredit;
        private float _timeRecord = 0;

        private int _currentRecStatusRightCam;
        private int _nextRecStatusRightCam;
        private RecordCameraState _rightCamState;
        private int _currentRecStatusBottomCam;
        private int _nextRecStatusBottomCam;
        private RecordCameraState _bottomCamState;

        #endregion

        #region Manager References

        [Header("Manager References")]
        [SerializeField] public AudioManager AudioManager;
        [SerializeField] public CameraManager CameraManager;
        [SerializeField] public CanvaManager CanvaManager;
        [SerializeField] public InputManager InputManager;
        [SerializeField] public GyroscopeManager GyroscopeManager;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// ゲームオブジェクト有効化時処理
        /// </summary>
        private void OnEnable()
        {
            Time.timeScale = 1;
        }

        /// <summary>
        /// 初期化処理 - PlayerPrefs設定とゲーム状態初期化
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            InitializePlayerPrefs();
        }

        /// <summary>
        /// スタート処理 - UIコンポーネント初期化とエフェクト設定
        /// </summary>
        private void Start()
        {
            InitializeGameState();
            InitializeCameraState();
            InitializeComponents();
            InitializePetrifyEffects();
            InitializeTitleElements();
            InitializeAssetBundles();

            AudioManager.PlayAudio("bgm_Opening");
        }

        /// <summary>
        /// 毎フレーム更新 - UI状態管理と入力処理
        /// </summary>
        protected override void Update()
        {
            base.Update();

            UpdateButtonInteractability();

            if (HandleSceneTransition()) return;
            if (_isWaiting || isSceneFinished) return;

            HandleInputEvents();
            HandleCameraOperations();

            DarkenCam.SetActive(isOption || _showCredit);
        }

        /// <summary>
        /// フレーム終了時処理 - オプション時のカメラ制御
        /// </summary>
        private void LateUpdate()
        {
            if (isOption)
            {
                _rightCamComponent.enabled = false;
                _bottomCamComponent.enabled = false;
            }
        }

        #endregion
        #region Public API

        /// <summary>
        /// ステージ選択シーンへの移行処理
        /// </summary>
        public void MoveToStageSelection()
        {
            if (Time.time - _timeRecord < _timeWait) return;

            StartCoroutine(PetrifyAnimation());
            _timeRecord = Time.time;
            _isWaiting = true;
            AudioManager.PlayAudio("se_Button");
        }

        /// <summary>
        /// オプション状態切り替え処理
        /// </summary>
        public void OptionStatus()
        {
            if (Time.time - _timeRecord < _timeWait) return;

            isOption = !isOption;
            CanvaManager.isOption = isOption;
            GyroscopeManager.isFunctioning = !isOption;
            _timeRecord = Time.time;
            AudioManager.PlayAudio("se_Button");
        }

        /// <summary>
        /// クレジット表示切り替え処理
        /// </summary>
        public void EnableCredit()
        {
            if (Time.time - _timeRecord < _timeWait) return;

            _showCredit = !_showCredit;
            UpdateCameraStatusForCredit();
            _timeRecord = Time.time;

            if (_showCredit) AudioManager.PlayAudio("se_Button");
        }

        #endregion
        #region Private Methods

        /// <summary>
        /// PlayerPrefs初期化
        /// </summary>
        private void InitializePlayerPrefs()
        {
            PlayerPrefs.SetInt("StageID", 0);
            PlayerPrefs.SetFloat("zoomRate", 0f);
            PlayerPrefs.SetFloat("waveNum", 1);
            PlayerPrefs.SetFloat("stageSize", 1);
            PlayerPrefs.SetFloat("enemyNum", 1);
            PlayerPrefs.SetFloat("enemyAttr", 1);
            PlayerPrefs.SetFloat("obstaclePercent", 1);
            PlayerPrefs.SetFloat("spawnSpeed", 1);
            PlayerPrefs.SetFloat("hpMax", 10);
            PlayerPrefs.SetFloat("resource", 1);
        }

        /// <summary>
        /// ゲーム状態初期化
        /// </summary>
        private void InitializeGameState()
        {
            _isOpening = true;
            isOption = false;
            _showCredit = false;
        }

        /// <summary>
        /// カメラ状態初期化
        /// </summary>
        private void InitializeCameraState()
        {
            _rightCamState = RecordCameraState.Exit;
            _currentRecStatusRightCam = 1;
            _nextRecStatusRightCam = (_currentRecStatusRightCam % _maxRecStatus) + 1;
            _bottomCamState = RecordCameraState.Exit;
            _currentRecStatusBottomCam = 1;
            _nextRecStatusBottomCam = (_currentRecStatusBottomCam % _maxRecStatus) + 1;
        }

        /// <summary>
        /// コンポーネント初期化
        /// </summary>
        private void InitializeComponents()
        {
            _landscapeFade = LandscapeFadeImg.gameObject.GetComponent<FadeEffect>();
            _portraitFade = PortraitFadeImg.gameObject.GetComponent<FadeEffect>();
            _rightCamComponent = RightCam.GetComponent<Camera>();
            _bottomCamComponent = BottomCam.GetComponent<Camera>();
        }

        /// <summary>
        /// ペトリファイエフェクト初期化
        /// </summary>
        private void InitializePetrifyEffects()
        {
            foreach (Material mat in PetrifyMat)
            {
                mat.SetFloat("_Progress", 0);
            }

            foreach (Image img in PetrifyImgs)
            {
                img.material.SetFloat("_Progress", 0);
            }

            foreach (RawImage rawImg in PetrifyRImgs)
            {
                rawImg.material.SetFloat("_Progress", 0);
            }

            foreach (SpriteRenderer spr in PetrifySpr)
            {
                spr.material.SetFloat("_Progress", 0);
            }
        }

        /// <summary>
        /// タイトル要素初期化
        /// </summary>
        private void InitializeTitleElements()
        {
            _titleImgRaw = new List<RawImage>();
            _titleImgText = new List<Text>();
            foreach (GameObject obj in TitleImg)
            {
                _titleImgRaw.Add(obj.GetComponent<RawImage>());
                _titleImgText.Add(obj.GetComponent<Text>());
            }

            _titleEffectImgVFX = new List<VisualEffect>();
            foreach (GameObject obj in TitleEffectImg)
            {
                var vfx = obj.GetComponent<VisualEffect>();
                _titleEffectImgVFX.Add(vfx);
                vfx.Stop();
            }
        }

        /// <summary>
        /// アセットバンドル初期化
        /// </summary>
        private void InitializeAssetBundles()
        {
            for (int i = 0; i < bundleUrl.Count; ++i)
            {
                LoadBundle.LoadAssetBundle(bundleUrl[i], bundleName[i], "Assets/AssetBundles");
            }
        }

        /// <summary>
        /// ボタンインタラクション状態更新
        /// </summary>
        private void UpdateButtonInteractability()
        {
            bool canInteractStart = !_showCredit && !isOption && !_isWaiting;
            bool canInteractOption = !_showCredit && !_isWaiting;
            bool canInteractCredit = !isOption && !_isWaiting;

            foreach (Button button in StartButton)
                button.interactable = canInteractStart;
            foreach (Button button in OptionButton)
                button.interactable = canInteractOption;
            foreach (Button button in CreditButton)
                button.interactable = canInteractCredit;
        }

        /// <summary>
        /// シーン遷移処理
        /// </summary>
        /// <returns>true if scene transition occurred</returns>
        private bool HandleSceneTransition()
        {
            if (isSceneFinished && ((_landscapeFade && _landscapeFade.isReady) || (_portraitFade && _portraitFade.isReady)))
            {
                SceneManager.LoadScene("LoadingScene");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 入力イベント処理
        /// </summary>
        private void HandleInputEvents()
        {
            if (_isOpening && InputManager.GetAnyInput())
            {
                _isWaiting = true;
                foreach (VisualEffect vfx in _titleEffectImgVFX)
                {
                    vfx.Play();
                }
                StartCoroutine(PreparationToMain());
            }

            if (_showCredit && InputManager.GetAnyInput())
            {
                EnableCredit();
            }
        }

        /// <summary>
        /// カメラ操作処理
        /// </summary>
        private void HandleCameraOperations()
        {
            if (!_isOpening)
            {
                if (_rightCamState == RecordCameraState.Exit)
                    StartCoroutine(RecRightCamOperation());
                if (_bottomCamState == RecordCameraState.Exit)
                    StartCoroutine(RecBottomCamOperation());
            }
        }

        /// <summary>
        /// クレジット用カメラ状態更新
        /// </summary>
        private void UpdateCameraStatusForCredit()
        {
            if (_showCredit)
            {
                _nextRecStatusRightCam = 0;
                _nextRecStatusBottomCam = 0;
            }
            else
            {
                _nextRecStatusRightCam = 1;
                _nextRecStatusBottomCam = 1;
            }
        }

        /// <summary>
        /// ペトリファイアニメーションコルーチン
        /// </summary>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator PetrifyAnimation()
        {
            float progress = 0f;
            int frame = 15;
            float rate = 1f / frame;

            while (frame-- > 0)
            {
                progress += rate;
                SetPetrifyProgress(progress);
                yield return null;
            }

            SetNextScene("StageSelection");
            SceneOut();
            isSceneFinished = true;
        }

        /// <summary>
        /// ペトリファイ進行度設定
        /// </summary>
        /// <param name="progress">進行度値</param>
        private void SetPetrifyProgress(float progress)
        {
            foreach (Image img in PetrifyImgs)
            {
                img.material.SetFloat("_Progress", progress);
            }
            foreach (RawImage rawImg in PetrifyRImgs)
            {
                rawImg.material.SetFloat("_Progress", progress);
            }
            foreach (SpriteRenderer spr in PetrifySpr)
            {
                spr.material.SetFloat("_Progress", progress);
            }
        }

        #endregion

        #region Camera Operations

        /// <summary>
        /// 右カメラ操作コルーチン
        /// </summary>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator RecRightCamOperation()
        {
            yield return StartCoroutine(ExecuteCameraTransition(
                RightCam.transform,
                RightCamStartPt[_currentRecStatusRightCam],
                RightCamStayPt[_currentRecStatusRightCam],
                RightCamEndPt[_currentRecStatusRightCam],
                (state) => _rightCamState = state,
                () => _showCredit && _currentRecStatusRightCam == 0
            ));

            _currentRecStatusRightCam = _nextRecStatusRightCam;
            _nextRecStatusRightCam = (_currentRecStatusRightCam % _maxRecStatus) + 1;
        }

        /// <summary>
        /// 下カメラ操作コルーチン
        /// </summary>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator RecBottomCamOperation()
        {
            yield return StartCoroutine(ExecuteCameraTransition(
                BottomCam.transform,
                BottomCamStartPt[_currentRecStatusBottomCam],
                BottomCamStayPt[_currentRecStatusBottomCam],
                BottomCamEndPt[_currentRecStatusBottomCam],
                (state) => _bottomCamState = state,
                () => _showCredit && _currentRecStatusBottomCam == 0
            ));

            _currentRecStatusBottomCam = _nextRecStatusBottomCam;
            _nextRecStatusBottomCam = (_currentRecStatusBottomCam % _maxRecStatus) + 1;
        }

        /// <summary>
        /// カメラ遷移実行コルーチン
        /// </summary>
        /// <param name="cameraTransform">カメラのTransform</param>
        /// <param name="startPos">開始位置</param>
        /// <param name="stayPos">停止位置</param>
        /// <param name="endPos">終了位置</param>
        /// <param name="stateCallback">状態更新コールバック</param>
        /// <param name="creditCondition">クレジット条件チェック</param>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator ExecuteCameraTransition(
            Transform cameraTransform,
            Vector3 startPos,
            Vector3 stayPos,
            Vector3 endPos,
            System.Action<RecordCameraState> stateCallback,
            System.Func<bool> creditCondition)
        {
            // Start to Stay
            stateCallback(RecordCameraState.StarttoStay);
            yield return StartCoroutine(MoveCameraToPosition(cameraTransform, startPos, stayPos));

            // Stay
            stateCallback(RecordCameraState.Stay);
            if (creditCondition())
            {
                while (creditCondition())
                    yield return null;
            }
            else
            {
                float timer = 0;
                while (timer < _maxRecWaitTimer && !_showCredit)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            // Stay to Exit
            stateCallback(RecordCameraState.StaytoExit);
            yield return StartCoroutine(MoveCameraToPosition(cameraTransform, stayPos, endPos));

            // Exit
            stateCallback(RecordCameraState.Exit);
        }

        /// <summary>
        /// カメラ位置移動コルーチン
        /// </summary>
        /// <param name="cameraTransform">カメラのTransform</param>
        /// <param name="fromPos">開始位置</param>
        /// <param name="toPos">目標位置</param>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator MoveCameraToPosition(Transform cameraTransform, Vector3 fromPos, Vector3 toPos)
        {
            cameraTransform.position = fromPos;
            Vector3 speed = (toPos - fromPos) / _maxRecTimer;
            float timer = 0;

            while (timer < _maxRecTimer)
            {
                timer += Time.deltaTime;
                if (timer > _maxRecTimer) timer = _maxRecTimer;
                cameraTransform.position = fromPos + speed * timer;
                yield return null;
            }
        }

        #endregion

        #region Opening Operations

        /// <summary>
        /// オープニングアニメーション実行
        /// </summary>
        private void OpeningAnimation()
        {
            CameraManager.RotateCam(_targetCamAngle);

            GameObject[] upperEggs = GameObject.FindGameObjectsWithTag("UpperEgg");
            if (upperEggs.Length >= 2)
            {
                StartCoroutine(UpperEggAnimation(upperEggs[0], 0.4f));
                StartCoroutine(UpperEggAnimation(upperEggs[1], 0.4f));
            }

            GameObject[] lowerEggs = GameObject.FindGameObjectsWithTag("LowerEgg");
            if (lowerEggs.Length >= 2)
            {
                StartCoroutine(LowerEggAnimation(lowerEggs[0], -0.5f));
                StartCoroutine(LowerEggAnimation(lowerEggs[1], -0.2f));
            }

            _isOpening = false;
            AudioManager.PlayAudio("se_Button");
        }

        /// <summary>
        /// 上側エッグアニメーションコルーチン
        /// </summary>
        /// <param name="upperEgg">上側エッグオブジェクト</param>
        /// <param name="distance">移動距離</param>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator UpperEggAnimation(GameObject upperEgg, float distance)
        {
            int frames = 30;
            float posChangePerFrame = (distance - transform.localPosition.y) / frames;

            while (frames-- > 0)
            {
                Vector3 currentPos = upperEgg.transform.localPosition;
                upperEgg.transform.localPosition = new Vector3(
                    currentPos.x,
                    currentPos.y + posChangePerFrame,
                    currentPos.z
                );
                yield return null;
            }
        }

        /// <summary>
        /// 下側エッグアニメーションコルーチン
        /// </summary>
        /// <param name="lowerEgg">下側エッグオブジェクト</param>
        /// <param name="distance">移動距離</param>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator LowerEggAnimation(GameObject lowerEgg, float distance)
        {
            int frames = 50;
            float posChangePerFrame = (distance - transform.localPosition.y) / frames;

            while (frames-- > 0)
            {
                Vector3 currentPos = lowerEgg.transform.localPosition;
                lowerEgg.transform.localPosition = new Vector3(
                    currentPos.x,
                    currentPos.y + posChangePerFrame,
                    currentPos.z
                );
                yield return null;
            }

            foreach (GameObject eggCam in EggCam)
                eggCam.SetActive(false);
        }

        /// <summary>
        /// メインシーン切り替え準備コルーチン
        /// </summary>
        /// <returns>コルーチンの進行状況</returns>
        private IEnumerator PreparationToMain()
        {
            int frames = 20;

            while (frames-- > 0)
            {
                float alpha = frames / 20f;
                float colorIntensity = frames / 40f;

                for (int i = 0; i < TitleImg.Count; ++i)
                {
                    if (_titleImgRaw[i])
                    {
                        _titleImgRaw[i].color = new Color(colorIntensity, colorIntensity, colorIntensity, colorIntensity);
                    }
                    if (_titleImgText[i])
                    {
                        _titleImgText[i].color = new Color(1, 1, 1, alpha);
                    }
                }

                yield return null;
            }

            OpeningAnimation();
            _isWaiting = false;

            if (CameraManager) CameraManager.isOpening = _isOpening;
            if (CanvaManager) CanvaManager.isOpening = _isOpening;
        }

        #endregion
    }
}
