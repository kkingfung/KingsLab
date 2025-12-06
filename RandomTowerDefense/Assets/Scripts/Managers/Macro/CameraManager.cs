using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using RandomTowerDefense.Managers.System;
using RandomTowerDefense.Scene;

namespace RandomTowerDefense.Managers.Macro
{
    /// <summary>
    /// カメラ管理システム - カメラ動作と向き切り替えの制御
    ///
    /// 主な機能:
    /// - 横向き/縦向き向き用マルチカメラシステム
    /// - 沈浸型体験用ジャイロスコープ制御カメラ回転
    /// - ユーザー設定付き動的ズーム機能
    /// - スムーズカメラ遷移とアニメーション
    /// - デバイス向き検出との統合
    /// - FOV管理と制約処理
    /// - UI主導カメラ制御インターフェース
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        #region Constants
        private readonly float defaultFOV = 60f;
        private readonly float minFOV = 5f;
        private readonly int rotateFrame = 60;
        #endregion

        #region Serialized Fields
        [Header("📷 Landscape Cameras")]
        public List<Camera> LandscapeCam_Main;
        public List<Camera> LandscapeCam_Sub;

        [Header("📱 Portrait Cameras")]
        public List<Camera> PortraitCam_Main;
        public List<Camera> PortraitCam_Sub;

        [Header("🌍 Gyroscope Cameras")]
        public List<GameObject> GyroCamGp;
        public List<Camera> ZoomCamGp;

        [Header("🔍 Zoom Controls")]
        public List<Slider> zoomSlider;
        #endregion

        #region Public Properties
        [HideInInspector]
        public bool isOpening;
        [HideInInspector]
        public bool isGyroEnabled;
        #endregion

        #region Private Fields
        private GyroscopeManager _gyroscopeManager;
        private ISceneChange _sceneManager;
        private TutorialManager _tutorialManager;
        #endregion
        #region Unity Lifecycle
        /// <summary>
        /// Initialize camera settings and component references
        /// </summary>
        private void OnEnable()
        {
            isGyroEnabled = false;
            _sceneManager = FindObjectOfType<ISceneChange>();

            foreach (Slider slider in zoomSlider)
            {
                slider.value = PlayerPrefs.GetFloat("zoomRate", 0);
            }
        }

        /// <summary>
        /// Initialize camera system with default settings
        /// </summary>
        private void Start()
        {
            isOpening = true;
            _gyroscopeManager = FindObjectOfType<GyroscopeManager>();

            // Initialize zoom cameras if gyroscope functionality is available
            if (GyroCamGp.Count > 0 && _gyroscopeManager && _gyroscopeManager.isFunctioning)
            {
                for (int i = 0; i < ZoomCamGp.Count; ++i)
                {
                    ZoomCamGp[i].fieldOfView = defaultFOV;
                }
            }

            // Apply saved zoom setting
            Zoom(PlayerPrefs.GetFloat("zoomRate", 0));
        }

        /// <summary>
        /// Update camera states based on orientation and gyroscope input
        /// </summary>
        private void Update()
        {
            if (_sceneManager == null) return;

            UpdateCameraOrientations();
            UpdateZoomControls();
            UpdateGyroscopeRotation();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Set camera zoom level and update UI
        /// </summary>
        /// <param name="zoomRate">Zoom rate between 0 and 1</param>
        public void Zoom(float zoomRate)
        {
            if (ZoomCamGp.Count > 0)
            {
                foreach (Camera camera in ZoomCamGp)
                {
                    camera.fieldOfView = Mathf.Clamp(defaultFOV - zoomRate * (defaultFOV - minFOV), minFOV, defaultFOV);
                }

                foreach (Slider slider in zoomSlider)
                {
                    slider.value = zoomRate;
                }
            }

            PlayerPrefs.SetFloat("zoomRate", zoomRate);
        }

        /// <summary>
        /// Rotate camera to target angle with smooth animation
        /// </summary>
        /// <param name="targetAngle">Target rotation angle in degrees</param>
        public void RotateCam(float targetAngle)
        {
            StartCoroutine(RotateMainCamera(targetAngle));
        }

        /// <summary>
        /// Reset gyroscope-controlled cameras to neutral position
        /// </summary>
        public void ResetGyroCam()
        {
            for (int i = 0; i < GyroCamGp.Count; ++i)
            {
                GyroCamGp[i].transform.localEulerAngles = Vector3.zero;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update camera visibility based on orientation and state
        /// </summary>
        private void UpdateCameraOrientations()
        {
            bool isLandscape = _sceneManager.OrientationLand;

            // Update landscape cameras
            foreach (Camera camera in LandscapeCam_Main)
            {
                camera.enabled = isOpening && isLandscape;
            }
            foreach (Camera camera in LandscapeCam_Sub)
            {
                camera.enabled = !isOpening && isLandscape;
            }

            // Update portrait cameras
            foreach (Camera camera in PortraitCam_Main)
            {
                camera.enabled = isOpening && !isLandscape;
            }
            foreach (Camera camera in PortraitCam_Sub)
            {
                camera.enabled = !isOpening && !isLandscape;
            }
        }

        /// <summary>
        /// Update zoom control UI interactability
        /// </summary>
        private void UpdateZoomControls()
        {
            bool isInteractable = !_sceneManager.GetOptionStatus();
            foreach (Slider slider in zoomSlider)
            {
                slider.interactable = isInteractable;
            }
        }

        /// <summary>
        /// Update camera rotation based on gyroscope or input
        /// </summary>
        private void UpdateGyroscopeRotation()
        {
            if (GyroCamGp.Count == 0 || !_gyroscopeManager) return;

            if (_gyroscopeManager.isFunctioning)
            {
                HandleGyroscopeInput();
            }
            else
            {
                HandleKeyboardInput();
            }
        }

        /// <summary>
        /// Handle gyroscope-based camera rotation
        /// </summary>
        private void HandleGyroscopeInput()
        {
            if (Time.timeScale == 0) return;

            for (int i = 0; i < GyroCamGp.Count; ++i)
            {
                GyroCamGp[i].transform.Rotate(new Vector3(_gyroscopeManager.GetLocalPitch(), 0, 0), Space.Self);
                GyroCamGp[i].transform.Rotate(new Vector3(0, _gyroscopeManager.GetWorldYaw(), 0), Space.World);
            }
        }

        /// <summary>
        /// Handle keyboard-based camera rotation fallback
        /// </summary>
        private void HandleKeyboardInput()
        {
            if (Time.timeScale == 0) return;

            for (int i = 0; i < GyroCamGp.Count; ++i)
            {
                float verticalInput = Input.GetAxis("Vertical") * Time.deltaTime * -50f / Time.timeScale;
                float horizontalInput = Input.GetAxis("Horizontal") * Time.deltaTime * 50f / Time.timeScale;

                GyroCamGp[i].transform.Rotate(new Vector3(verticalInput, 0, 0), Space.Self);
                GyroCamGp[i].transform.Rotate(new Vector3(0, horizontalInput, 0), Space.World);
            }
        }

        /// <summary>
        /// Smoothly rotate main camera to target angle over time
        /// </summary>
        /// <param name="targetAngle">Target rotation angle</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator RotateMainCamera(float targetAngle)
        {
            int frame = 0;
            float angleChangePerFrame = (targetAngle - transform.localEulerAngles.x) / rotateFrame;

            while (frame < rotateFrame)
            {
                Vector3 currentEuler = transform.localEulerAngles;
                transform.localEulerAngles = new Vector3(
                    currentEuler.x + angleChangePerFrame,
                    currentEuler.y,
                    currentEuler.z
                );
                frame++;
                yield return new WaitForSeconds(0f);
            }
        }
        #endregion
    }
}
