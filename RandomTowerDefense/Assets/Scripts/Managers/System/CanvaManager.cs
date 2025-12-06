using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomTowerDefense.Scene;

namespace RandomTowerDefense.Managers.System
{
    /// <summary>
    /// キャンバス管理システム - 画面向きベースのUIキャンバス切り替え管理
    ///
    /// 主な機能:
    /// - 横向き/縦向きキャンバスの自動切り替え
    /// - メイン、サブ、オプションキャンバス状態管理
    /// - シーン向き検出との統合
    /// - リアルタイムレスポンシブUI適応
    /// - マルチキャンバス階層サポート
    /// </summary>
    public class CanvaManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("🖥️ Landscape Canvas Groups")]
        public List<GameObject> LandscapeCanva_Main;
        public List<GameObject> LandscapeCanva_Sub;
        public List<GameObject> LandscapeCanva_Opt;

        [Header("📱 Portrait Canvas Groups")]
        public List<GameObject> PortraitCanva_Main;
        public List<GameObject> PortraitCanva_Sub;
        public List<GameObject> PortraitCanva_Opt;
        #endregion

        #region Public Properties
        [HideInInspector]
        public bool isOpening;
        [HideInInspector]
        public bool isOption;
        #endregion

        #region Private Fields
        private ISceneChange _sceneManager;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Initialize canvas manager with default opening state
        /// </summary>
        private void Start()
        {
            isOpening = true;
            _sceneManager = FindObjectOfType<ISceneChange>();
        }

        /// <summary>
        /// Update canvas visibility based on orientation and state
        /// </summary>
        private void Update()
        {
            if (_sceneManager == null) return;

            bool isLandscape = _sceneManager.OrientationLand;
            bool isPortrait = !isLandscape;

            // Update landscape canvases
            UpdateCanvasGroup(LandscapeCanva_Main, isOpening && isLandscape);
            UpdateCanvasGroup(LandscapeCanva_Sub, !isOpening && isLandscape);
            UpdateCanvasGroup(LandscapeCanva_Opt, isOption && isLandscape);

            // Update portrait canvases
            UpdateCanvasGroup(PortraitCanva_Main, isOpening && isPortrait);
            UpdateCanvasGroup(PortraitCanva_Sub, !isOpening && isPortrait);
            UpdateCanvasGroup(PortraitCanva_Opt, isOption && isPortrait);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update the active state of all GameObjects in a canvas group
        /// </summary>
        /// <param name="canvasGroup">List of GameObjects to update</param>
        /// <param name="active">Whether the canvas group should be active</param>
        private void UpdateCanvasGroup(List<GameObject> canvasGroup, bool active)
        {
            foreach (GameObject canvas in canvasGroup)
            {
                if (canvas != null)
                {
                    canvas.SetActive(active);
                }
            }
        }
        #endregion
    }
}
