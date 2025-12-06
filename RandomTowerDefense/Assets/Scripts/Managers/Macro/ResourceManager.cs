using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomTowerDefense.Units;

namespace RandomTowerDefense.Managers.Macro
{
    /// <summary>
    /// リソース管理システム - ゲーム内通貨と材料の管理
    ///
    /// 主な機能:
    /// - 材料/通貨追跡と検証
    /// - タワー建設コスト管理
    /// - タワー売却とリソース還元システム
    /// - リソース変更検証とトランザクション
    /// - 開始材料設定
    /// - 経済バランス管理
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        #region Constants
        private readonly int StartingMaterialNum = 300;
        private readonly int[] BuildPrice = { 120, 0, 0, 0, 0 };
        private readonly int[] SellPrice = { 50, 100, 150, 200, 250 };

        // 検証定数
        private const int MIN_TOWER_RANK = 1;
        #endregion

        #region Public Properties
        public int CurrentMaterial;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Initialize resource manager with starting materials
        /// </summary>
        private void Start()
        {
            ResetMaterial();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Reset current material to starting amount
        /// </summary>
        public void ResetMaterial()
        {
            CurrentMaterial = StartingMaterialNum;
        }

        /// <summary>
        /// Change current material by specified amount
        /// </summary>
        /// <param name="Chg">Amount to change (positive for gain, negative for cost)</param>
        /// <returns>True if change was successful, false if insufficient resources</returns>
        public bool ChangeMaterial(int Chg)
        {
            if (Chg < 0 && CurrentMaterial < -Chg) return false;
            CurrentMaterial += Chg;
            return true;
        }

        /// <summary>
        /// Get current material amount
        /// </summary>
        /// <returns>Current material count</returns>
        public int GetCurrMaterial()
        {
            return CurrentMaterial;
        }

        /// <summary>
        /// Check if player can afford to build and deduct cost if successful
        /// </summary>
        /// <param name="rank">Tower rank to build (1-based index)</param>
        /// <returns>True if build was successful and cost deducted</returns>
        public bool ChkAndBuild(int rank)
        {
            if (rank < MIN_TOWER_RANK || rank > BuildPrice.Length) return false;

            int cost = BuildPrice[rank - MIN_TOWER_RANK];
            if (CurrentMaterial < cost) return false;

            CurrentMaterial -= cost;
            return true;
        }

        /// <summary>
        /// Sell a tower and refund materials based on its rank
        /// </summary>
        /// <param name="targetTower">Tower to sell</param>
        /// <returns>True if tower was sold successfully</returns>
        public bool SellTower(Tower targetTower)
        {
            if (targetTower == null || targetTower.rank < MIN_TOWER_RANK || targetTower.rank > SellPrice.Length)
            {
                return false;
            }

            CurrentMaterial += SellPrice[targetTower.rank - MIN_TOWER_RANK];
            return true;
        }
        #endregion
    }
}
