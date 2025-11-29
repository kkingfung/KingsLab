using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Units;

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

        // Price arrays for different upgrade categories
        private readonly int[] PriceForArmy1 = { 50, 75, 100, 150, 200, 250, 350, 450, 550, 700 };
        private readonly int[] PriceForArmy2 = { 50, 75, 100, 150, 200, 250, 350, 450, 550, 700 };
        private readonly int[] PriceForArmy3 = { 50, 75, 100, 150, 200, 250, 350, 450, 550, 700 };
        private readonly int[] PriceForArmy4 = { 50, 75, 100, 150, 200, 250, 350, 450, 550, 700 };

        private readonly int[] PriceForCastleHP = { 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 };
        private readonly int[] PriceForBonusBoss1 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly int[] PriceForBonusBoss2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly int[] PriceForBonusBoss3 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

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
        #endregion

        #region Private Fields
        private Dictionary<Upgrades.StoreItems, int[]> _itemPrice;
        private readonly int[] __pendToKart = { 0, 0, 0, 0 };
        private readonly int[] __costToKart = { 0, 0, 0, 0 };
        private readonly float[] __bonusBossCooldown = { 0, 0, 0 };
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Initialize store system and price mappings
        /// </summary>
        private void Start()
        {
            InitializePriceDictionary();
        }

        /// <summary>
        /// Update store state and UI elements
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

        #region Private Methods
        /// <summary>
        /// Initialize the price dictionary with all store items
        /// </summary>
        private void InitializePriceDictionary()
        {
            _itemPrice = new Dictionary<Upgrades.StoreItems, int[]>();

            _itemPrice.Add(Upgrades.StoreItems.Army1, PriceForArmy1);
            _itemPrice.Add(Upgrades.StoreItems.Army2, PriceForArmy2);
            _itemPrice.Add(Upgrades.StoreItems.Army3, PriceForArmy3);
            _itemPrice.Add(Upgrades.StoreItems.Army4, PriceForArmy4);

            _itemPrice.Add(Upgrades.StoreItems.CastleHP, PriceForCastleHP);
            _itemPrice.Add(Upgrades.StoreItems.BonusBoss1, PriceForBonusBoss1);
            _itemPrice.Add(Upgrades.StoreItems.BonusBoss2, PriceForBonusBoss2);
            _itemPrice.Add(Upgrades.StoreItems.BonusBoss3, PriceForBonusBoss3);

            _itemPrice.Add(Upgrades.StoreItems.MagicMeteor, PriceForMagicMeteor);
            _itemPrice.Add(Upgrades.StoreItems.MagicBlizzard, PriceForMagicBlizzard);
            _itemPrice.Add(Upgrades.StoreItems.MagicPetrification, PriceForMagicPetrification);
            _itemPrice.Add(Upgrades.StoreItems.MagicMinions, PriceForMagicMinions);
        }

        /// <summary>
        /// Update bonus boss cooldown timers
        /// </summary>
        private void UpdateBonusBossCooldowns()
        {
            for (int i = 0, s = __bonusBossCooldown.Length; i < s; ++i)
            {
                if (__bonusBossCooldown[i] > 0)
                {
                    __bonusBossCooldown[i] -= Time.deltaTime;
                }
                if (sceneManager.CheckIfTutorial())
                {
                    __bonusBossCooldown[i] = 99;
                }
            }
        }

        /// <summary>
        /// Update all store item prices and UI displays
        /// </summary>
        private void UpdatePrice()
        {

            int fullitemID = (sceneManager.currScreenShown + 1) * Upgrades.MaxItemPerSlot;

            //Tower Items
            for (int i = 0; i < TowerPriceTextObj.Count; ++i)
            {
                //Currently all same price
                int price = PriceForCastleHP[0];
                TowerPriceTextObj[i].text = price.ToString() + "G";
                TowerPriceTextObj[i].color = (price > resourceManager.GetCurrMaterial()) ? new Color(1, 0, 0, 1) : OriColor;

            }
            for (int i = 0; i < TowerHPTextObj.Count; ++i)
                TowerHPTextObj[i].text = stageManager.GetCurrHP().ToString() + "/" + stageManager.GetMaxHP().ToString();
            //Bonus Boss Items
            for (int i = 0; i < MonsterCDTextObj.Count; ++i)
            {
                int cd = (int)(__bonusBossCooldown[i % __bonusBossCooldown.Length]);
                MonsterCDTextObj[i].text = "CD" + cd.ToString();
                MonsterCDTextObj[i].color = (cd > 0 || SkillStack.CheckFullStocks()) ? new Color(1, 0, 0, 1) : OriColor;
            }

            for (int i = 0; i < ArmyPriceTextObj.Count; ++i)
            {
                Upgrades.StoreItems itemID = Upgrades.StoreItems.Army1 + i % MaxItemPerCategory;
                int price = _itemPrice[itemID][Upgrades.GetLevel(itemID)];
                if (Upgrades.CheckArmyTopLevel(itemID))
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
                        ArmyLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.Army1) ? "MAX" :
                            (Upgrades.GetLevel(Upgrades.StoreItems.Army1) + 1).ToString()); break;
                    case 1:
                        ArmyLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.Army2) ? "MAX" :
                            (Upgrades.GetLevel(Upgrades.StoreItems.Army2) + 1).ToString()); break;
                    case 2:
                        ArmyLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.Army3) ? "MAX" :
                            (Upgrades.GetLevel(Upgrades.StoreItems.Army3) + 1).ToString()); break;
                    case 3:
                        ArmyLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.Army4) ? "MAX" :
                            (Upgrades.GetLevel(Upgrades.StoreItems.Army4) + 1).ToString()); break;
                }
            }

            for (int i = 0; i < SkillPriceTextObj.Count; ++i)
            {
                Upgrades.StoreItems itemID = Upgrades.StoreItems.MagicMeteor + i % MaxItemPerCategory;
                int price = _itemPrice[itemID][Upgrades.GetLevel(itemID)];
                SkillPriceTextObj[i].text = price.ToString() + "G";
                SkillPriceTextObj[i].color = (price > resourceManager.GetCurrMaterial() || SkillStack.CheckFullStocks()) ? new Color(1, 0, 0, 1) : OriColor;
            }

            for (int i = 0; i < SkillLvTextObj.Count; ++i)
            {
                switch (i % MaxItemPerCategory)
                {
                    case 0:
                        SkillLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.MagicMeteor) ? "MAX" :
                           (Upgrades.GetLevel(Upgrades.StoreItems.MagicMeteor) + 1).ToString()); break;
                    case 1:
                        SkillLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.MagicBlizzard) ? "MAX" :
                        (Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard) + 1).ToString()); break;
                    case 2:
                        SkillLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.MagicPetrification) ? "MAX" :
                       (Upgrades.GetLevel(Upgrades.StoreItems.MagicPetrification) + 1).ToString()); break;
                    case 3:
                        SkillLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.MagicMinions) ? "MAX" :
                       (Upgrades.GetLevel(Upgrades.StoreItems.MagicMinions) + 1).ToString()); break;
                }
            }
        }

        #endregion

        #region Public Methods
        public int GetPrice(int itemID)
        {
            return GetPrice((Upgrades.StoreItems)itemID);
        }

        public int GetPrice(Upgrades.StoreItems itemID)
        {
            if (_itemPrice.ContainsKey(itemID) == false) return 0;

            switch (itemID)
            {
                case Upgrades.StoreItems.BonusBoss1:
                    return _bonusBossCooldown[0] > 0 ? -1 : 0;
                case Upgrades.StoreItems.BonusBoss2:
                    return _bonusBossCooldown[1] > 0 ? -1 : 0;
                case Upgrades.StoreItems.BonusBoss3:
                    return _bonusBossCooldown[2] > 0 ? -1 : 0;
            }

            if (Upgrades.GetLevel(itemID) + 1 >= _itemPrice[itemID].Length)
            {
                return -1;
            }
            return _itemPrice[itemID][Upgrades.GetLevel(itemID)];
        }

        public int GetCost(int itemID)
        {
            return GetCost((Upgrades.StoreItems)itemID, _pendToKart[itemID % 10]);
        }

        public int GetCost(Upgrades.StoreItems itemID, int additionLv)
        {
            if (_itemPrice.ContainsKey(itemID) == false) return 0;
            return _itemPrice[itemID][Upgrades.GetLevel(itemID) + additionLv];
        }


        public void ItemSold(Upgrades.StoreItems itemID)
        {
            Upgrades.StoreUpgrade(itemID, _pendToKart[(int)itemID % 10]);

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

        public bool ItemPendingAdd(Upgrades.StoreItems itemID)
        {
            int price = CheckEnoughResource(itemID);
            if (price >= 0)
            {
                switch (itemID)
                {
                    case Upgrades.StoreItems.BonusBoss1:
                    case Upgrades.StoreItems.BonusBoss2:
                    case Upgrades.StoreItems.BonusBoss3:
                    case Upgrades.StoreItems.MagicBlizzard:
                    case Upgrades.StoreItems.MagicMeteor:
                    case Upgrades.StoreItems.MagicPetrification:
                    case Upgrades.StoreItems.MagicMinions:
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

        public int CheckEnoughResource(Upgrades.StoreItems itemID)
        {
            int totalCosttoPurchase = CosttoPurchaseCalculation();
            int price = GetPrice(itemID);
            if (price < 0) return -1;
            if (price == 0) return 0;
            if (resourceManager.GetCurrMaterial() - totalCosttoPurchase >= price)
                return price;
            return -1;
        }

        public bool ItemPendingSubtract(int itemID)
        {
            if (_pendToKart[itemID % 10] < 0) return false;

            _pendToKart[itemID % 10]--;
            _costToKart[itemID % 10] -= GetCost(itemID);
            return true;
        }

        public void SetBossCD(int bossID)
        {
            _bonusBossCooldown[bossID] = cdCounter[bossID];
        }

        public float GetBossCD(int bossID)
        {
            return _bonusBossCooldown[bossID];
        }

        public void RaycastAction(Upgrades.StoreItems itemID, int infoID)
        {
            if (ItemPendingAdd(itemID))
                ItemSold(itemID);

            //-1 :subtract 0:purchase 1:add
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
            //-1 :subtract 0:purchase 1:add
            raycastAction((Upgrades.StoreItems)fullitemID, infoID);
        }

        #endregion
    }
}
