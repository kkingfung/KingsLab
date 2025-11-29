using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.DOTS.Spawner;
using RandomTowerDefense.Systems;

namespace RandomTowerDefense.Units
{
    /// <summary>
    /// アップグレードシステム管理クラス - タワー、城、スキルのアップグレードを統括
    /// </summary>
    public class UpgradesManager : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// 各項目の共通最大レベル
        /// </summary>
        static readonly int MaxLevel = 9;

        #endregion

        #region Enums

        /// <summary>
        /// ストアで購入可能なアイテムの種類
        /// </summary>
        public enum StoreItems
        {
            ArmySoulEater = 20,
            ArmyNightmare,
            ArmyTerrorBringer,
            ArmyUsurper,

            CastleHP = 30,
            BonusBossGreen,
            BonusBossPurple,
            BonusBossRed,

            MagicMeteor = 40,
            MagicBlizzard,
            MagicPetrification,
            MagicMinions,
        }

        #endregion

        #region Properties and Fields

        [SerializeField]
        CastleSpawner castleSpawner;

        [SerializeField]
        StoreManager storeManager;

        /// <summary>
        /// 現時点の各項目レベルを管理する辞書
        /// </summary>\
        public Dictionary<StoreItems, int> StoreLevel => storeLevel;
        private Dictionary<StoreItems, int> storeLevel;

        #endregion

        #region Public Methods

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            storeLevel = new Dictionary<StoreItems, int>();
            foreach (var item in System.Enum.GetValues(typeof(StoreItems)))
            {
                storeLevel.Add((StoreItems)item, 0);
            }
        }

        /// <summary>
        /// 指定項目のレベルを取得する
        /// </summary>
        /// <param name="item">取得したい項目</param>
        /// <returns>項目のレベル</returns>
        public int GetLevel(StoreItems item)
            => StoreLevel.TryGetValue(item, out var level) ? level : -1;


        /// <summary>
        /// 指定した項目がレベルアップ可能か確認
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public bool CheckTopLevel(StoreItems itemID)
        {
            var isArmy = itemID == StoreItems.ArmySoulEater || itemID == StoreItems.ArmyNightmare
                || itemID == StoreItems.ArmyTerrorBringer || itemID == StoreItems.ArmyUsurper;
            return !isArmy || StoreLevel[itemID] < MaxLevel;
        }

        /// <summary>
        /// 全スキルの合計レベルを取得
        /// </summary>
        /// <returns>合計レベル/returns>
        public int GetTotalLevel()
        {
            return StoreLevel.Values.Sum();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 指定スキルのレベルを増やす
        /// </summary>
        /// <param name="itemID">レベルアップする項目</param>
        /// <param name="lvUP">増加させるレベル数</param>
        /// <returns>レベルアップ成功/失敗</returns>
        bool AddSkillLevel(StoreItems itemID, int lvUP)
        {
            if (StoreLevel.TryGetValue(itemID, out var currentLevel))
            {
                if (currentLevel + lvUP > MaxLevel)
                {
                    return false;
                }

                StoreLevel[itemID] += lvUP;
                return true;
            }

            return false;
        }

        #endregion
    }
}
