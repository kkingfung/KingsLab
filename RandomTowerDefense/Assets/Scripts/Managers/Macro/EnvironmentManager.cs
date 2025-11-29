using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomTowerDefense.Scene;

namespace RandomTowerDefense.Managers.Macro
{
    /// <summary>
    /// 環境管理システム - 視覚環境と大気エフェクトの制御
    ///
    /// 主な機能:
    /// - ステージ進行に基づく動的スカイボックス管理
    /// - 異なる島環境用地形切り替え
    /// - シェーダーアニメーション付き昼夜サイクルシミュレーション
    /// - ステージ固有の環境テーマ
    /// - リアルタイムスカイボックスパラメータ調整
    /// - シーン管理システムとの統合
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        #region Constants
        public readonly float daytimeFactor = 30f;
        #endregion

        #region Serialized Fields
        [Header("🌅 Skybox Materials")]
        public List<Material> skyboxMat;

        [Header("🏔️ Terrain Objects")]
        public List<GameObject> terrainObj;
        public bool showTerrain = false;

        [Header("🎮 Scene Management")]
        public InGameOperation sceneManager;
        #endregion

        #region Private Fields
        private int _stageID;
        private float _shaderInput;
        private readonly int _maxSkyboxCubemap = 3;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Initialize environment settings based on current stage
        /// </summary>
        private void Start()
        {
            _stageID = sceneManager ? sceneManager.GetCurrIsland() : 0;
            _shaderInput = Time.time;

            // Set appropriate skybox for current stage
            if (_stageID < skyboxMat.Count)
            {
                RenderSettings.skybox = skyboxMat[_stageID];
            }

            // Configure terrain visibility if enabled
            if (showTerrain)
            {
                SetupTerrainVisibility();
            }
        }

        /// <summary>
        /// Update environment effects, particularly day/night cycle
        /// </summary>
        private void Update()
        {
            // Handle dynamic skybox animation for specific stages
            if (_stageID == _maxSkyboxCubemap && _stageID < skyboxMat.Count)
            {
                UpdateSkyboxAnimation();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Setup terrain object visibility based on current stage
        /// </summary>
        private void SetupTerrainVisibility()
        {
            for (int i = 0; i < terrainObj.Count; ++i)
            {
                terrainObj[i].SetActive(i == _stageID);
            }
        }

        /// <summary>
        /// Update skybox shader animation for day/night cycle
        /// </summary>
        private void UpdateSkyboxAnimation()
        {
            _shaderInput += Time.deltaTime / daytimeFactor;

            // Wrap shader input to stay within valid range
            while (_shaderInput > _maxSkyboxCubemap)
            {
                _shaderInput -= _maxSkyboxCubemap;
            }

            // Apply shader parameter to skybox material
            skyboxMat[_stageID].SetFloat("SkyboxFactor", _shaderInput);
        }
        #endregion
    }
}
