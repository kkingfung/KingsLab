using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RandomTowerDefense.Scene;

namespace RandomTowerDefense.Managers.System
{
    /// <summary>
    /// 時間管理システム - ゲームタイムスケールと時間メカニクスの制御
    ///
    /// 主な機能:
    /// - 動的タイムスケール調整（1x、1x、4x速度）
    /// - 精密制御用スローモーションエフェクト
    /// - タイムスケールUI表示と管理
    /// - フレームレート非依存時間計算
    /// - ゲーム状態と入力システムとの統合
    /// - 時間デバッグと開発ツール
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("⏰ Time Scale Configuration")]
        /// <summary>
        /// Time scale factor for slow motion adjustment
        /// </summary>
        public float timeFactor = 0.05f;

        /// <summary>
        /// Fixed timestep length for time adjustment
        /// </summary>
        public float timeLength = 0.02f;

        [Header("🖥️ UI Display")]
        /// <summary>
        /// Text UI elements for displaying current time scale
        /// </summary>
        public List<Text> text;

        [Header("🎮 Manager References")]
        /// <summary>
        /// Game scene manager reference
        /// </summary>
        public InGameOperation sceneManager;

        /// <summary>
        /// Input manager reference
        /// </summary>
        public InputManager inputManager;
        #endregion

        #region Private Fields
        private float _originalTimeScale;
        private float _originalFixedTimeScale;
        private readonly float[] _timeScaleFactor = { 1f, 2f, 4f };
        private readonly int[] _timeScaleShow = { 1, 2, 3 };
        private int _timeScaleId;
        private bool _isControl = false;
        #endregion
        #region Unity Lifecycle
        /// <summary>
        /// Initialize time scale settings and UI display
        /// </summary>
        private void Start()
        {
            _timeScaleId = 0;
            Time.timeScale = 1.0f;
            _originalTimeScale = Time.timeScale;
            _originalFixedTimeScale = Time.fixedDeltaTime;

            foreach (Text textElement in text)
            {
                textElement.text = "X" + (int)Time.timeScale;
            }
        }
        /// <summary>
        /// Restore time scale to original values if not under manual control
        /// </summary>
        private void Update()
        {
            if (_isControl) return;
            Time.timeScale = _originalTimeScale;
            Time.fixedDeltaTime = _originalFixedTimeScale;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Adjust game time scale and fixed timestep for slow motion effects
        /// </summary>
        public void AdjustTime()
        {
            Time.timeScale = timeFactor;
            Time.fixedDeltaTime = Time.timeScale * timeLength;
        }

        /// <summary>
        /// Toggle time control mode on/off
        /// </summary>
        public void TimeControl()
        {
            _isControl = !_isControl;
            if (_isControl)
            {
                AdjustTime();
            }
        }

        /// <summary>
        /// Change time scale by specified amount
        /// </summary>
        /// <param name="chg">Amount to change time scale (default: 1)</param>
        public void ChgTimeScale(int chg = 1)
        {
            if (sceneManager && sceneManager.GetOptionStatus()) return;
            SetTimeScale(_timeScaleId + chg);
            if (inputManager)
            {
                inputManager.TapTimeRecord = 0;
            }
        }
        /// <summary>
        /// Set time scale to specified target value
        /// </summary>
        /// <param name="target">Target time scale ID</param>
        public void SetTimeScale(int target)
        {
            _timeScaleId = target;
            _timeScaleId %= _timeScaleFactor.Length;
            Time.timeScale = _timeScaleFactor[_timeScaleId];
            _originalTimeScale = Time.timeScale;

            foreach (Text textElement in text)
            {
                textElement.text = "X" + _timeScaleShow[_timeScaleId];
            }
        }
        /// <summary>
        /// Check if time manager has manual control enabled
        /// </summary>
        /// <returns>True if under manual time control</returns>
        public bool HasControl()
        {
            return _isControl;
        }
        #endregion
    }
}
