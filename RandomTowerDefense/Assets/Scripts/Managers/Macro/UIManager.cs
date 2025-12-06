using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTowerDefense.Managers.Macro
{
    /// <summary>
    /// UI管理システム - グローバルUI表示と状態管理の制御
    ///
    /// 主な機能:
    /// - グローバルUI表示切り替え機能
    /// - 自動UI要素発見と登録
    /// - レンダラーベースUIコンポーネント管理
    /// - 効率的なUI状態変更検出と適用
    /// - レイヤーベースUI要素フィルタリング
    /// - 変更検出使用パフォーマンス最適化UI更新
    /// </summary>
    public class UIManager : MonoBehaviour
{
        #region Public Properties
        [HideInInspector]
        public bool isUIshown = true;
        #endregion

        #region Private Fields
        private bool _isUIshownHistory = true;
        private readonly List<Renderer> _allUI = new List<Renderer>();
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Initialize UI manager by discovering all UI renderers in the scene
        /// </summary>
        private void Start()
        {
            DiscoverUIElements();
        }

        /// <summary>
        /// Check for UI visibility changes and apply them to all UI renderers
        /// </summary>
        private void LateUpdate()
        {
            if (isUIshown != _isUIshownHistory)
            {
                ApplyUIVisibilityChange();
                _isUIshownHistory = isUIshown;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Discover and register all UI renderer components in the scene
        /// </summary>
        private void DiscoverUIElements()
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            int uiLayerMask = LayerMask.NameToLayer("UI");

            foreach (GameObject obj in allObjects)
            {
                if (obj.layer == uiLayerMask)
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        _allUI.Add(renderer);
                    }
                }
            }
        }

        /// <summary>
        /// Apply visibility changes to all registered UI renderers
        /// </summary>
        private void ApplyUIVisibilityChange()
        {
            foreach (Renderer uiRenderer in _allUI)
            {
                if (uiRenderer != null)
                {
                    uiRenderer.enabled = isUIshown;
                }
            }
        }
        #endregion
    }
}