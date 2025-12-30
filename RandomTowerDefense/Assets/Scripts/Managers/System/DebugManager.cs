using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomTowerDefense.Managers.Macro;

namespace RandomTowerDefense.Managers.System
{
    /// <summary>
    /// デバッグ管理システム - 開発テストユーティリティの管理
    ///
    /// 主な機能:
    /// - ゲームプレイデバッグ用タイムスケール制御
    /// - 自動テスト用シミュレーションサポート
    /// - パフォーマンス監視と設定取得
    /// - 開発ワークフロー用キーボードショートカット
    /// - Unity Simulationとの統合でバッチテスト実行
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
        #region Constants
        private readonly float timescaleChg = 1f;
        #endregion

        #region Serialized Fields
        [Header("🔧 Debug Configuration")]
        public WaveManager waveManager;

        [Header("🧪 Simulation Testing")]
        public bool isSimulationTest;
        #endregion

        #region Private Fields
        private float _myTimeScale;
        private int _currSpawningPoint;
        [HideInInspector]
        public bool isFetchDone;
        #endregion

        //public float towerrank_Damage = 0;
        //public float towerlvl_Damage = 0;
        //public float enemylvl_Health = 0;
        //public float enemylvl_Speed = 0;

        #region Unity Lifecycle
        private void Awake()
        {
            isFetchDone = false;
            if (isSimulationTest)
            {
                // Simulation configuration fetching would go here
                // GameSimManager.Instance.FetchConfig(OnFetchConfigDone);
            }
            else
            {
                isFetchDone = true;
            }
        }
        private void Start()
        {
            Initialize();
        }

        //private void OnFetchConfigDone(GameSimConfigResponse gameSimConfigResponse)
        //{
        //    towerrank_Damage = gameSimConfigResponse.GetFloat("towerrank_Damage");
        //    towerlvl_Damage = gameSimConfigResponse.GetFloat("towerlvl_Damage");
        //    enemylvl_Health = gameSimConfigResponse.GetFloat("enemylvl_Health");
        //    enemylvl_Speed = gameSimConfigResponse.GetFloat("enemylvl_Speed");
        //
        //    isFetchDone = true;
        //}

        private void Update()
        {
            if (waveManager)
            {
                _currSpawningPoint = waveManager.SpawnPointByAI;
            }

            HandleDebugInput();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Called when map is reset during simulation testing
        /// </summary>
        public void MapResetted()
        {
            if (isSimulationTest == false) return;

            // Record simulation metrics
            // GameSimManager.Instance.SetCounter("WaveArrived", waveManager.GetCurrentWaveNum());

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initialize debug manager with default settings
        /// </summary>
        private void Initialize()
        {
            _myTimeScale = 1.0f;
            _currSpawningPoint = 0;
            Time.timeScale = _myTimeScale;
            Time.fixedDeltaTime = Time.timeScale;
        }

        /// <summary>
        /// Handle keyboard input for debug controls
        /// </summary>
        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                Initialize(); // Reset time scale
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                ChangeSpeed(-1); // Slow down
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                ChangeSpeed(+1); // Speed up
            }
        }

        /// <summary>
        /// Change game time scale for debugging purposes
        /// </summary>
        /// <param name="direction">Direction to change speed (-1 for slower, +1 for faster)</param>
        private void ChangeSpeed(int direction)
        {
            _myTimeScale += direction * timescaleChg;
            if (_myTimeScale < 0)
            {
                _myTimeScale = 0;
            }

            Time.timeScale = _myTimeScale;
            Time.fixedDeltaTime = Time.timeScale;
        }
        #endregion
    }
}
