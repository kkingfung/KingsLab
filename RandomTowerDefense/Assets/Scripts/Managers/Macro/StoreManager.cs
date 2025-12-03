using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Units;
using RandomTowerDefense.Systems;
using RandomTowerDefense.DOTS.Spawner;
using StoreItems = RandomTowerDefense.Units.UpgradesManager.StoreItems;

namespace RandomTowerDefense.Managers.Macro
{
    /// <summary>
    /// アップグレード購入とアイテム管理を統合するモジュール
    ///
    /// 主な機能:
    /// - 複数ティア構成のタワーアップグレードシステム
    /// - 城体力の強化購入
    /// - 魔法スキルの成長およびクールダウン管理
    /// - アップグレードレベルに応じた動的価格設定
    /// - リソース連携とコスト検証
    /// - リアルタイム価格表示のUI統合
    /// - クールダウンを伴うボーナスボス能力
    /// </summary>
    public class StoreManager : MonoBehaviour
    {
        #region Constants
        private readonly int MaxItemPerCategory = 4;
        private readonly int[] cdCounter = { 60, 90, 150 };
        private readonly Color OriColor = new Color(1, 0.675f, 0, 1);

        /// <summary>
        /// 各アップグレードカテゴリの価格配列
        /// </summary>
        private readonly int[] PriceForArmySoulEater = { 50, 75, 100, 150, 200, 250, 350, 450, 550, 700 };
        private readonly int[] PriceForArmyNightmare = { 50, 75, 100, 150, 200, 250, 350, 450, 550, 700 };
        private readonly int[] PriceForArmyTerrorBringer = { 50, 75, 100, 150, 200, 250, 350, 450, 550, 700 };
        private readonly int[] PriceForArmyUsurper = { 50, 75, 100, 150, 200, 250, 350, 450, 550, 700 };

        private readonly int[] PriceForCastleHP = { 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 };
        private readonly int[] PriceForBonusBossGreen = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly int[] PriceForBonusBossPurple = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly int[] PriceForBonusBossRed = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private readonly int[] PriceForMagicMeteor = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
        private readonly int[] PriceForMagicBlizzard = { 8, 16, 24, 32, 40, 48, 56, 64, 72, 80 };
        private readonly int[] PriceForMagicMinions = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
        private readonly int[] PriceForMagicPetrification = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
        #endregion

        #region Serialized Fields
        [Header("🪖 Army Upgrade UI")]
        public List<TextMesh> ArmyLvTextObj;
        public List<TextMesh> ArmyPriceTextObj;

        [Header("🏰 Tower/Castle UI")]
        public List<TextMesh> TowerHPTextObj;
        public List<TextMesh> TowerPriceTextObj;
        public List<TextMesh> MonsterCDTextObj;

        [Header("✨ Magic Skills UI")]
        public List<TextMesh> SkillPriceTextObj;
        public List<TextMesh> SkillLvTextObj;

        [Header("🎮 Manager References")]
        public InGameOperation sceneManager;
        public StageManager stageManager;
        public ResourceManager resourceManager;

        public UpgradesManager upgradesManager;
        public CastleSpawner castleSpawner;

        #endregion

        #region Private Fields
        private Dictionary<UpgradesManager.StoreItems, int[]> _itemPrice;
        private readonly int[] _pendToKart = { 0, 0, 0, 0 };
        private readonly int[] _costToKart = { 0, 0, 0, 0 };
        private readonly float[] _bonusBossCooldown = { 0, 0, 0 };
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// ストアシステムと価格マッピングの初期化
        /// </summary>
        private void Start()
        {
            InitializePriceDictionary();
        }

        /// <summary>
        /// ストア状態とUI要素の更新
        /// </summary>
        private void Update()
        {
            if (sceneManager && sceneManager.currScreenShown != 0)
            {
                UpdateBonusBossCooldowns();
                UpdatePrice();
            }
        }
        #endregion

        /// <summary>
        /// 指定項目のレベルを増やす
        /// </summary>
        /// <param name="itemID">レベルアップする項目</param>
        /// <param name="lvUP">増加レベル数</param>
        /// <returns>レベルアップ成功/失敗</returns>
        public bool OnStoreItemSold(UpgradesManager.StoreItems itemID, int lvUP)
        {
            var hasUpgrade = upgradesManager.UpgradeLevel(itemID, lvUP);

            // ストックアイテムのチェック
            switch (itemID)
            {
                case StoreItems.CastleHP:
                    if (castleSpawner != null && castleSpawner.castle != null)
                        castleSpawner.castle.AddedHealth();
                    break;
                case StoreItems.BonusBossGreen:
                case StoreItems.BonusBossPurple:
                case StoreItems.BonusBossRed:
                    SkillStack.AddStock(itemID);
                    SetBossCD((int)(itemID - UpgradesManager.StoreItems.BonusBossGreen));
                    break;
                case UpgradesManager.StoreItems.ArmySoulEater:
                case UpgradesManager.StoreItems.ArmyNightmare:
                case UpgradesManager.StoreItems.ArmyTerrorBringer:
                case UpgradesManager.StoreItems.ArmyUsurper:
                    // 上記のupgradesManager.UpgradeLevelで既に処理済み
                    break;
                case StoreItems.MagicMeteor:
                case StoreItems.MagicBlizzard:
                case StoreItems.MagicMinions:
                case StoreItems.MagicPetrification:
                    SkillStack.AddStock(itemID);
                    break;
            }
            return hasUpgrade;
        }
        #region Private Methods
        /// <summary>
        /// 全ストアアイテムの価格辞書を初期化
        /// </summary>
        private void InitializePriceDictionary()
        {
            _itemPrice = new Dictionary<UpgradesManager.StoreItems, int[]>();

            _itemPrice.Add(UpgradesManager.StoreItems.ArmySoulEater, PriceForArmySoulEater);
            _itemPrice.Add(UpgradesManager.StoreItems.ArmyNightmare, PriceForArmyNightmare);
            _itemPrice.Add(UpgradesManager.StoreItems.ArmyTerrorBringer, PriceForArmyTerrorBringer);
            _itemPrice.Add(UpgradesManager.StoreItems.ArmyUsurper, PriceForArmyUsurper);

            _itemPrice.Add(UpgradesManager.StoreItems.CastleHP, PriceForCastleHP);
            _itemPrice.Add(UpgradesManager.StoreItems.BonusBossGreen, PriceForBonusBossGreen);
            _itemPrice.Add(UpgradesManager.StoreItems.BonusBossPurple, PriceForBonusBossPurple);
            _itemPrice.Add(UpgradesManager.StoreItems.BonusBossRed, PriceForBonusBossRed);

            _itemPrice.Add(UpgradesManager.StoreItems.MagicMeteor, PriceForMagicMeteor);
            _itemPrice.Add(UpgradesManager.StoreItems.MagicBlizzard, PriceForMagicBlizzard);
            _itemPrice.Add(UpgradesManager.StoreItems.MagicPetrification, PriceForMagicPetrification);
            _itemPrice.Add(UpgradesManager.StoreItems.MagicMinions, PriceForMagicMinions);
        }

        /// <summary>
        /// ボーナスボスのクールダウンタイマーを更新
        /// </summary>
        private void UpdateBonusBossCooldowns()
        {
            for (int i = 0, s = _bonusBossCooldown.Length; i < s; ++i)
            {
                if (_bonusBossCooldown[i] > 0)
                {
                    _bonusBossCooldown[i] -= Time.deltaTime;
                }
                if (sceneManager.CheckIfTutorial())
                {
                    _bonusBossCooldown[i] = 99;
                }
            }
        }

        /// <summary>
        /// 全ストアアイテムの価格とUI表示を更新
        /// </summary>
        private void UpdatePrice()
        {

            int fullitemID = (sceneManager.currScreenShown + 1) * MaxItemPerCategory;

            // タワーアイテム
            for (int i = 0; i < TowerPriceTextObj.Count; ++i)
            {
                // 現在全て同じ価格
                int price = PriceForCastleHP[0];
                TowerPriceTextObj[i].text = price.ToString() + "G";
                TowerPriceTextObj[i].color = (price > resourceManager.GetCurrMaterial()) ? new Color(1, 0, 0, 1) : OriColor;

            }
            for (int i = 0; i < TowerHPTextObj.Count; ++i)
                TowerHPTextObj[i].text = stageManager.GetCurrHP().ToString() + "/" + stageManager.GetMaxHP().ToString();
            // ボーナスボスアイテム
            for (int i = 0; i < MonsterCDTextObj.Count; ++i)
            {
                int cd = (int)(_bonusBossCooldown[i % _bonusBossCooldown.Length]);
                MonsterCDTextObj[i].text = "CD" + cd.ToString();
                MonsterCDTextObj[i].color = (cd > 0 || SkillStack.CheckFullStocks()) ? new Color(1, 0, 0, 1) : OriColor;
            }

            for (int i = 0; i < ArmyPriceTextObj.Count; ++i)
            {
                UpgradesManager.StoreItems itemID = UpgradesManager.StoreItems.ArmySoulEater + i % MaxItemPerCategory;
                int price = _itemPrice[itemID][upgradesManager.GetLevel(itemID)];
                if (upgradesManager.CheckTopLevel(itemID))
                {
                    ArmyPriceTextObj[i].text = price.ToString() + "G";
                    ArmyPriceTextObj[i].color = (price > resourceManager.GetCurrMaterial()) ? new Color(1, 0, 0, 1) : OriColor;
                }
                else
                {
                    ArmyPriceTextObj[i].text = "";
                    ArmyPriceTextObj[i].color = new Color(0, 0, 0, 1);
                }
            }

            for (int i = 0; i < ArmyLvTextObj.Count; ++i)
            {
                switch (i % MaxItemPerCategory)
                {
                    case 0:
                        ArmyLvTextObj[i].text = "LV." + (!upgradesManager.CheckTopLevel(UpgradesManager.StoreItems.ArmySoulEater) ? "MAX" :
                            (upgradesManager.GetLevel(UpgradesManager.StoreItems.ArmySoulEater) + 1).ToString()); break;
                    case 1:
                        ArmyLvTextObj[i].text = "LV." + (!upgradesManager.CheckTopLevel(UpgradesManager.StoreItems.ArmyNightmare) ? "MAX" :
                            (upgradesManager.GetLevel(UpgradesManager.StoreItems.ArmyNightmare) + 1).ToString()); break;
                    case 2:
                        ArmyLvTextObj[i].text = "LV." + (!upgradesManager.CheckTopLevel(UpgradesManager.StoreItems.ArmyTerrorBringer) ? "MAX" :
                            (upgradesManager.GetLevel(UpgradesManager.StoreItems.ArmyTerrorBringer) + 1).ToString()); break;
                    case 3:
                        ArmyLvTextObj[i].text = "LV." + (!upgradesManager.CheckTopLevel(UpgradesManager.StoreItems.ArmyUsurper) ? "MAX" :
                            (upgradesManager.GetLevel(UpgradesManager.StoreItems.ArmyUsurper) + 1).ToString()); break;
                }
            }

            for (int i = 0; i < SkillPriceTextObj.Count; ++i)
            {
                UpgradesManager.StoreItems itemID = UpgradesManager.StoreItems.MagicMeteor + i % MaxItemPerCategory;
                int price = _itemPrice[itemID][upgradesManager.GetLevel(itemID)];
                SkillPriceTextObj[i].text = price.ToString() + "G";
                SkillPriceTextObj[i].color = (price > resourceManager.GetCurrMaterial() || SkillStack.CheckFullStocks()) ? new Color(1, 0, 0, 1) : OriColor;
            }

            for (int i = 0; i < SkillLvTextObj.Count; ++i)
            {
                switch (i % MaxItemPerCategory)
                {
                    case 0:
                        SkillLvTextObj[i].text = "LV." + (upgradesManager.CheckTopLevel(UpgradesManager.StoreItems.MagicMeteor) ? "MAX" :
                           (upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicMeteor) + 1).ToString()); break;
                    case 1:
                        SkillLvTextObj[i].text = "LV." + (upgradesManager.CheckTopLevel(UpgradesManager.StoreItems.MagicBlizzard) ? "MAX" :
                        (upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicBlizzard) + 1).ToString()); break;
                    case 2:
                        SkillLvTextObj[i].text = "LV." + (upgradesManager.CheckTopLevel(UpgradesManager.StoreItems.MagicPetrification) ? "MAX" :
                       (upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicPetrification) + 1).ToString()); break;
                    case 3:
                        SkillLvTextObj[i].text = "LV." + (upgradesManager.CheckTopLevel(UpgradesManager.StoreItems.MagicMinions) ? "MAX" :
                       (upgradesManager.GetLevel(UpgradesManager.StoreItems.MagicMinions) + 1).ToString()); break;
                }
            }
        }

        #endregion

        #region Public Methods
        public int GetPrice(int itemID)
        {
            return GetPrice((UpgradesManager.StoreItems)itemID);
        }

        public int GetPrice(UpgradesManager.StoreItems itemID)
        {
            if (_itemPrice.ContainsKey(itemID) == false) return 0;

            switch (itemID)
            {
                case UpgradesManager.StoreItems.BonusBossGreen:
                    return _bonusBossCooldown[0] > 0 ? -1 : 0;
                case UpgradesManager.StoreItems.BonusBossPurple:
                    return _bonusBossCooldown[1] > 0 ? -1 : 0;
                case UpgradesManager.StoreItems.BonusBossRed:
                    return _bonusBossCooldown[2] > 0 ? -1 : 0;
            }

            if (upgradesManager.GetLevel(itemID) + 1 >= _itemPrice[itemID].Length)
            {
                return -1;
            }
            return _itemPrice[itemID][upgradesManager.GetLevel(itemID)];
        }

        public int GetCost(int itemID)
        {
            return GetCost((UpgradesManager.StoreItems)itemID, _pendToKart[itemID % 10]);
        }

        public int GetCost(UpgradesManager.StoreItems itemID, int additionLv)
        {
            if (_itemPrice.ContainsKey(itemID) == false) return 0;
            return _itemPrice[itemID][upgradesManager.GetLevel(itemID) + additionLv];
        }


        public void ItemSold(UpgradesManager.StoreItems itemID)
        {
            UpgradesManager.StoreUpgrade(itemID, _pendToKart[(int)itemID % 10]);

            resourceManager.ChangeMaterial(-1 * _costToKart[(int)itemID % 10]);

            _pendToKart[(int)itemID % 10] = 0;
            _costToKart[(int)itemID % 10] = 0;
        }

        public void ClearToPurchase()
        {
            for (int i = 0, s = _pendToKart.Length; i < s; ++i)
                _pendToKart[i] = 0;
            for (int i = 0, s = _costToKart.Length; i < s; ++i)
                _costToKart[i] = 0;
        }

        public int CosttoPurchaseCalculation()
        {
            int totalCost = 0;
            for (int i = 0, s = _costToKart.Length; i < s; ++i)
                totalCost += _costToKart[i];
            return totalCost;
        }

        public bool ItemPendingAdd(UpgradesManager.StoreItems itemID)
        {
            int price = CheckEnoughResource(itemID);
            if (price >= 0)
            {
                switch (itemID)
                {
                    case UpgradesManager.StoreItems.BonusBossGreen:
                    case UpgradesManager.StoreItems.BonusBossPurple:
                    case UpgradesManager.StoreItems.BonusBossRed:
                    case UpgradesManager.StoreItems.MagicBlizzard:
                    case UpgradesManager.StoreItems.MagicMeteor:
                    case UpgradesManager.StoreItems.MagicPetrification:
                    case UpgradesManager.StoreItems.MagicMinions:
                        if (SkillStack.CheckFullStocks())
                            return false;
                        break;
                }

                _pendToKart[(int)itemID % 10]++;
                _costToKart[(int)itemID % 10] += price;
                return true;
            }
            return false;
        }


        public bool ItemPendingSubtract(UpgradesManager.StoreItems itemID)
        {
            if (_pendToKart[(int)itemID % 10] < 0) return false;

            _pendToKart[(int)itemID % 10]--;
            _costToKart[(int)itemID % 10] -= GetCost((int)itemID);
            return true;
        }

        public int CheckEnoughResource(UpgradesManager.StoreItems itemID)
        {
            int totalCosttoPurchase = CosttoPurchaseCalculation();
            int price = GetPrice(itemID);
            if (price < 0) return -1;
            if (price == 0) return 0;
            if (resourceManager.GetCurrMaterial() - totalCosttoPurchase >= price)
                return price;
            return -1;
        }

        public void SetBossCD(int bossID)
        {
            _bonusBossCooldown[bossID] = cdCounter[bossID];
        }

        public float GetBossCD(int bossID)
        {
            return _bonusBossCooldown[bossID];
        }

        public void RaycastAction(UpgradesManager.StoreItems itemID, int infoID)
        {
            if (ItemPendingAdd(itemID))
                ItemSold(itemID);

            // -1: 減算, 0: 購入, 1: 追加
            switch (infoID)
            {
                case -1:
                    ItemPendingSubtract(itemID);
                    break;
                case 0:
                    ItemSold(itemID);
                    break;
                case 1:
                    ItemPendingAdd(itemID);
                    break;
            }
        }

        /// <summary>
        /// レイキャストアクション - ストアアイテムとの相互作用を処理
        /// </summary>
        /// <param name="fullitemID">アイテムID</param>
        /// <param name="infoID">情報ID (-1: 減算, 0: 購入, 1: 追加)</param>
        public void RaycastAction(int fullitemID, int infoID)
        {
            // -1: 減算, 0: 購入, 1: 追加
            RaycastAction((UpgradesManager.StoreItems)fullitemID, infoID);
        }

        #endregion
    }
}
