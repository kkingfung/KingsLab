using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.DOTS.Spawner;

namespace RandomTowerDefense.Units
{
    /// <summary>
    /// アップグレードシステム管理クラス - タワー、城、スキルのアップグレードを統括
    ///
    /// 主な機能:
    /// - タワー軍団アップグレード管理（ダメージ・攻撃速度向上）
    /// - 城防御力アップグレード管理（HP増強）
    /// - 魔法スキルアップグレード管理（効果・威力向上）
    /// - ボーナスアップグレード管理（特殊効果付与）
    /// - レベル上限管理とコスト計算
    /// - ECS統合によるリアルタイム性能反映
    /// - セーブデータ連携による永続化
    /// </summary>
    public class UpgradesManager : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// 各アップグレード項目の共通最大レベル
        /// </summary>
        private const int MAX_LEVEL = 9;
        /// <summary>
        /// アップグレードのベースコスト
        /// </summary>
        private const int BASE_UPGRADE_COST = 100;

        /// <summary>
        /// 軍団アップグレードのコスト倍率
        /// </summary>
        private const float ARMY_UPGRADE_COST_MULTIPLIER = 1.5f;

        /// <summary>
        /// その他のアップグレードのコスト倍率
        /// </summary>
        private const float OTHER_UPGRADE_COST_MULTIPLIER = 1.2f;

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

        #region Public Properties

        /// <summary>
        /// 現時点の各項目レベルを管理する辞書（読み取り専用）
        /// </summary>
        public Dictionary<StoreItems, int> StoreLevel => _storeLevel;

        #endregion

        #region Private Fields

        /// <summary>
        /// 各項目のアップグレードレベルを保持する内部辞書
        /// </summary>
        private Dictionary<StoreItems, int> _storeLevel;

        #endregion

        #region Manager References

        /// <summary>
        /// 城生成システムの参照
        /// </summary>
        [SerializeField]
        [Header("🏰 ECS Integration")]
        private CastleSpawner _castleSpawner;

        /// <summary>
        /// ストア管理システムの参照
        /// </summary>
        [SerializeField]
        [Header("🛒 Store Management")]
        private StoreManager _storeManager;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// アップグレードシステムの初期化処理
        /// </summary>
        private void Awake()
        {
            Init();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// アップグレードシステムの初期化 - 全項目をレベル0で初期化
        /// </summary>
        public void Init()
        {
            InitializeStoreLevelDictionary();
        }

        /// <summary>
        /// 指定項目の現在レベルを取得する
        /// </summary>
        /// <param name="item">取得したいアップグレード項目</param>
        /// <returns>項目の現在レベル（見つからない場合は-1）</returns>
        public int GetLevel(StoreItems item)
        {
            return _storeLevel.TryGetValue(item, out var level) ? level : -1;
        }


        /// <summary>
        /// 指定した項目がレベルアップ可能か確認
        /// </summary>
        /// <param name="itemID">確認したいアップグレード項目</param>
        /// <returns>レベルアップ可能な場合true</returns>
        public bool CheckTopLevel(StoreItems itemID)
        {
            return IsArmyUpgrade(itemID) ? _storeLevel[itemID] < MAX_LEVEL : true;
        }

        /// <summary>
        /// 全アップグレード項目の合計レベルを取得
        /// </summary>
        /// <returns>全項目の合計レベル</returns>
        public int GetTotalLevel()
        {
            return _storeLevel.Values.Sum();
        }

        /// <summary>
        /// 指定タイプのアップグレードレベルを上昇させる
        /// </summary>
        /// <param name="itemID">レベルアップする項目</param>
        /// <param name="levelIncrease">増加させるレベル数（デフォルト: 1）</param>
        /// <returns>レベルアップ成功の場合true</returns>
        public bool UpgradeLevel(StoreItems itemID, int levelIncrease = 1)
        {
            if (!CanUpgrade(itemID, levelIncrease))
            {
                return false;
            }

            return AddSkillLevel(itemID, levelIncrease);
        }

        /// <summary>
        /// 特定カテゴリのアップグレード情報を取得
        /// </summary>
        /// <param name="category">取得したいカテゴリ</param>
        /// <returns>カテゴリ別のアップグレード情報辞書</returns>
        public Dictionary<StoreItems, int> GetCategoryLevels(UpgradeCategory category)
        {
            var categoryItems = GetItemsByCategory(category);
            return categoryItems.ToDictionary(item => item, item => GetLevel(item));
        }

        /// <summary>
        /// アップグレードのコスト情報を取得
        /// </summary>
        /// <param name="itemID">コストを確認したい項目</param>
        /// <returns>次レベルへのアップグレードコスト</returns>
        public int GetUpgradeCost(StoreItems itemID)
        {
            int currentLevel = GetLevel(itemID);
            return CalculateUpgradeCost(itemID, currentLevel);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// ストアレベル辞書の初期化処理
        /// </summary>
        private void InitializeStoreLevelDictionary()
        {
            _storeLevel = new Dictionary<StoreItems, int>();
            foreach (StoreItems item in Enum.GetValues(typeof(StoreItems)))
            {
                _storeLevel.Add(item, 0);
            }
        }

        /// <summary>
        /// 軍団アップグレードかどうかを判定
        /// </summary>
        /// <param name="itemID">判定したい項目</param>
        /// <returns>軍団アップグレードの場合true</returns>
        private bool IsArmyUpgrade(StoreItems itemID)
        {
            return itemID == StoreItems.ArmySoulEater || itemID == StoreItems.ArmyNightmare
                || itemID == StoreItems.ArmyTerrorBringer || itemID == StoreItems.ArmyUsurper;
        }

        /// <summary>
        /// レベルアップが可能かどうかを確認
        /// </summary>
        /// <param name="itemID">確認したい項目</param>
        /// <param name="levelIncrease">増加させるレベル数</param>
        /// <returns>レベルアップ可能な場合true</returns>
        private bool CanUpgrade(StoreItems itemID, int levelIncrease)
        {
            if (!_storeLevel.ContainsKey(itemID))
            {
                return false;
            }

            int currentLevel = _storeLevel[itemID];
            int newLevel = currentLevel + levelIncrease;

            return IsArmyUpgrade(itemID) ? newLevel <= MAX_LEVEL : true;
        }

        /// <summary>
        /// 指定項目のレベルを増加させる内部処理
        /// </summary>
        /// <param name="itemID">レベルアップする項目</param>
        /// <param name="levelIncrease">増加させるレベル数</param>
        /// <returns>レベルアップ成功の場合true</returns>
        private bool AddSkillLevel(StoreItems itemID, int levelIncrease)
        {
            if (_storeLevel.TryGetValue(itemID, out var currentLevel))
            {
                if (IsArmyUpgrade(itemID) && currentLevel + levelIncrease > MAX_LEVEL)
                {
                    return false;
                }

                _storeLevel[itemID] += levelIncrease;
                return true;
            }

            return false;
        }

        /// <summary>
        /// カテゴリ別にアップグレード項目を取得
        /// </summary>
        /// <param name="category">取得したいカテゴリ</param>
        /// <returns>カテゴリ別のアップグレード項目リスト</returns>
        private List<StoreItems> GetItemsByCategory(UpgradeCategory category)
        {
            switch (category)
            {
                case UpgradeCategory.Army:
                    return new List<StoreItems> { StoreItems.ArmySoulEater, StoreItems.ArmyNightmare,
                                                  StoreItems.ArmyTerrorBringer, StoreItems.ArmyUsurper };
                case UpgradeCategory.Castle:
                    return new List<StoreItems> { StoreItems.CastleHP, StoreItems.BonusBossGreen,
                                                  StoreItems.BonusBossPurple, StoreItems.BonusBossRed };
                case UpgradeCategory.Magic:
                    return new List<StoreItems> { StoreItems.MagicMeteor, StoreItems.MagicBlizzard,
                                                  StoreItems.MagicPetrification, StoreItems.MagicMinions };
                default:
                    return new List<StoreItems>();
            }
        }

        /// <summary>
        /// アップグレードコストの計算
        /// </summary>
        /// <param name="itemID">コストを計算したい項目</param>
        /// <param name="currentLevel">現在のレベル</param>
        /// <returns>次レベルへのアップグレードコスト</returns>
        private int CalculateUpgradeCost(StoreItems itemID, int currentLevel)
        {
            // ベースコスト計算（レベルに応じて指数的に増加）
            int baseCost = BASE_UPGRADE_COST;
            float multiplier = IsArmyUpgrade(itemID) ? ARMY_UPGRADE_COST_MULTIPLIER : OTHER_UPGRADE_COST_MULTIPLIER;

            return Mathf.RoundToInt(baseCost * Mathf.Pow(multiplier, currentLevel));
        }

        #endregion
    }

    /// <summary>
    /// アップグレードカテゴリの分類
    /// </summary>
    public enum UpgradeCategory
    {
        /// <summary>
        /// 軍団アップグレード（タワー強化）
        /// </summary>
        Army,

        /// <summary>
        /// 城アップグレード（防御強化）
        /// </summary>
        Castle,

        /// <summary>
        /// 魔法アップグレード（スキル強化）
        /// </summary>
        Magic
    }
}
