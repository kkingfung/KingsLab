using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Managers.Macro;

namespace RandomTowerDefense.Managers.System
{
    /// <summary>
    /// ジャイロスコープ管理システム - デバイス向きとモーション検出の管理
    ///
    /// 主な機能:
    /// - モーションコントロール用デバイスジャイロスコープ統合
    /// - 特別なインタラクション用シェイクジェスチャー検出
    /// - 感度調整とユーザーキャリブレーション
    /// - マルチ向きサポート（縦向き/横向き）
    /// - ジャイロスコープ入力経由カメラ回転制御
    /// - デバイス向きベースUI要素回転
    /// - モーションベースゲームコントロール統合
    /// </summary>
    public class GyroscopeManager : MonoBehaviour
    {
        #region Constants
        private readonly float shakeThresholdH = 3f; // Horizontal shake detection threshold
        private readonly float shakeThresholdV = 2f; // Vertical shake detection threshold
        private readonly float timeInterval = 2f; // Minimum time between shake events
        private readonly float sensitiveAdjustment = 3f; // Sensitivity multiplier
        private readonly float pitchMultiplier = 3f; // Pitch axis sensitivity multiplier
        private readonly float PreventShaking = 1f; // Deadzone to prevent micro-movements
        #endregion

        #region Serialized Fields
        [Header("📱 Gyroscope Configuration")]
        public bool isFunctioning;
        public List<GameObject> gyroDiffUI;

        [Header("⚙️ Sensitivity Settings")]
        [Range(0, 1)]
        public float sensitivity = 0f;
        public List<Slider> senseSlider;

        [Header("🎮 Manager References")]
        public InputManager inputManager;
        public CameraManager cameraManager;
        public ISceneChange sceneManager;
        public InGameOperation gamesceneManager;
        #endregion

        #region Public Properties
        [HideInInspector]
        public bool LeftShake = false;
        [HideInInspector]
        public bool RightShake = false;
        [HideInInspector]
        public bool VerticalShake = false;
        #endregion

        #region Private Fields
        private Gyroscope _gyro;
        private bool _isActive;

        // Rotation values (device orientation)
        private float _pitch; // X axis rotation
        private float _yaw;   // Y axis rotation

        // Reference values for relative rotation
        private float _pitchRef; // X axis reference
        private float _yawRef;   // Y axis reference

        private float _timeRecord;
        #endregion
        #region Unity Lifecycle
        /// <summary>
        /// Initialize gyroscope hardware and enable motion detection
        /// </summary>
        private void Awake()
        {
            _gyro = Input.gyro;
            _gyro.enabled = true;
            _isActive = true;
        }

        /// <summary>
        /// Initialize gyroscope settings and component references
        /// </summary>
        private void Start()
        {
            isFunctioning = inputManager.GetUseTouch();
            sensitivity = PlayerPrefs.GetFloat("Gyro", 0);

            foreach (Slider slider in senseSlider)
            {
                slider.value = sensitivity;
            }

            _timeRecord = 0;
            _yaw = 0;
            _pitch = 0;

            ResetReference();
        }

        /// <summary>
        /// Update gyroscope input and apply rotations to UI elements
        /// </summary>
        private void Update()
        {
            if (isFunctioning && _isActive)
            {
                UpdateGyroYPR();
                GyroModify();
            }

            // Apply rotation to gyroscope-controlled UI elements
            if (gyroDiffUI.Count > 0)
            {
                foreach (GameObject uiElement in gyroDiffUI)
                {
                    uiElement.transform.localEulerAngles = new Vector3(0, 0, _yawRef);
                }
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Temporarily disable gyroscope input for calibration
        /// </summary>
        public void SetTempInactive()
        {
            _isActive = false;
            StartCoroutine(WaitToResume());
        }

        /// <summary>
        /// Set gyroscope sensitivity from UI slider
        /// </summary>
        /// <param name="value">Sensitivity value (0-1)</param>
        public void SetGyro(float value)
        {
            sensitivity = value;
            foreach (Slider slider in senseSlider)
            {
                slider.value = sensitivity;
            }
            PlayerPrefs.SetFloat("Gyro", sensitivity);
        }

        /// <summary>
        /// Manually adjust yaw reference value
        /// </summary>
        /// <param name="value">Yaw adjustment amount</param>
        public void SetYawChg(float value)
        {
            _yawRef += value;
        }

        /// <summary>
        /// Reset gyroscope reference values to neutral position
        /// </summary>
        public void ResetReference()
        {
            if (sceneManager && sceneManager.GetOptionStatus()) return;
            if (gamesceneManager)
            {
                gamesceneManager.AutoResetGyro();
            }
            if (cameraManager)
            {
                cameraManager.ResetGyroCam();
            }

            _yawRef = 0;
            _pitchRef = 0;
        }

        /// <summary>
        /// Get processed yaw value for world space rotation
        /// </summary>
        /// <returns>Yaw rotation adjusted for time scale</returns>
        public float GetWorldYaw()
        {
            if (Time.timeScale != 0)
            {
                return -_yaw / Time.timeScale;
            }
            return -_yaw;
        }

        /// <summary>
        /// Get processed pitch value for local rotation
        /// </summary>
        /// <returns>Pitch rotation adjusted for time scale</returns>
        public float GetLocalPitch()
        {
            if (Time.timeScale != 0)
            {
                return -_pitch / Time.timeScale;
            }
            return -_pitch;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Coroutine to wait before resuming gyroscope input
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator WaitToResume()
        {
            float timeRecord = 0;
            while (timeRecord < 0.2f)
            {
                timeRecord += Time.deltaTime;
                _yaw = 0;
                _pitch = 0;
                ResetReference();
                yield return new WaitForSeconds(0);
            }

            _isActive = true;
        }

        /// <summary>
        /// Process gyroscope input and detect shake gestures
        /// </summary>
        public void GyroModify()
        {
            //Reset Shake
            LeftShake = false;
            RightShake = false;
            VerticalShake = false;

            Vector3 rotRate = Input.gyro.rotationRate;

            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.Portrait:
                    if (rotRate.x > shakeThresholdV || rotRate.x < -shakeThresholdV)
                        VerticalShake = true;
                    if (rotRate.y < -shakeThresholdH)
                        LeftShake = true;
                    if (rotRate.y > shakeThresholdH)
                        RightShake = true;
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    if (rotRate.x > shakeThresholdV || rotRate.x < -shakeThresholdV)
                        VerticalShake = true;
                    if (rotRate.y < -shakeThresholdH)
                        RightShake = true;
                    if (rotRate.y > shakeThresholdH)
                        LeftShake = true;
                    break;
                case DeviceOrientation.LandscapeLeft:
                    if (rotRate.x > shakeThresholdV || rotRate.x < -shakeThresholdV)
                        VerticalShake = true;
                    if (rotRate.y < -shakeThresholdH)
                        LeftShake = true;
                    if (rotRate.y > shakeThresholdH)
                        RightShake = true;
                    break;
                case DeviceOrientation.LandscapeRight:
                    if (rotRate.x > shakeThresholdV || rotRate.x < -shakeThresholdV)
                        VerticalShake = true;
                    if (rotRate.y < -shakeThresholdH)
                        LeftShake = true;
                    if (rotRate.y > shakeThresholdH)
                        RightShake = true;
                    break;
            }

            if (VerticalShake && Time.time - _timeRecord > timeInterval)
            {
                _timeRecord = Time.time;
                LeftShake = false;
                RightShake = false;
            }

            if (LeftShake && Time.time - _timeRecord > timeInterval)
            {
                _timeRecord = Time.time;
                VerticalShake = false;
                RightShake = false;
            }

            if (RightShake && Time.time - _timeRecord > timeInterval)
            {
                _timeRecord = Time.time;
                VerticalShake = false;
                LeftShake = false;
            }
        }

        /// <summary>
        /// Convert gyroscope quaternion to Unity coordinate system
        /// </summary>
        /// <param name="q">Gyroscope quaternion</param>
        /// <returns>Unity-compatible quaternion</returns>
        private static Quaternion GyroToUnity(Quaternion q)
        {
            return new Quaternion(-q.x, -q.z, -q.y, q.w) * Quaternion.Euler(90, 0, 0);
        }

        /// <summary>
        /// Update gyroscope yaw, pitch, and roll values with sensitivity filtering
        /// </summary>
        private void UpdateGyroYPR()
        {
            _pitch = Input.gyro.rotationRateUnbiased.x * Mathf.Rad2Deg;
            _yaw = Input.gyro.rotationRateUnbiased.y * Mathf.Rad2Deg;

            // Apply deadzone and sensitivity to pitch
            if (_pitch > PreventShaking)
            {
                _pitch = (_pitch - PreventShaking) * sensitiveAdjustment * sensitivity * Time.deltaTime * pitchMultiplier;
            }
            else if (_pitch < -1f * PreventShaking)
            {
                _pitch = (_pitch + PreventShaking) * sensitiveAdjustment * sensitivity * Time.deltaTime * pitchMultiplier;
            }
            else
            {
                _pitch = 0;
            }

            // Apply deadzone and sensitivity to yaw
            if (_yaw > PreventShaking)
            {
                _yaw = (_yaw - PreventShaking) * sensitiveAdjustment * sensitivity * Time.deltaTime;
            }
            else if (_yaw < -1f * PreventShaking)
            {
                _yaw = (_yaw + PreventShaking) * sensitiveAdjustment * sensitivity * Time.deltaTime;
            }
            else
            {
                _yaw = 0;
            }

            // Update reference values
            _yawRef += _yaw;
            _pitchRef += _pitch;
        }
        #endregion
    }
}