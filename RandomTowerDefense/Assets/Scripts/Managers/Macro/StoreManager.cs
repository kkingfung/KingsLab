using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    private readonly int MaxItemPerCategory = 4;
    private readonly int[] cdCounter = { 60 , 90 , 150 };
    private readonly Color OriColor = new Color(1, 0.675f, 0, 1 );
    private Dictionary<Upgrades.StoreItems, int[]> ItemPrice;

    public List<TextMesh> ArmyLvTextObj;
    public List<TextMesh> ArmyPriceTextObj;

    public List<TextMesh> TowerHPTextObj;
    public List<TextMesh> TowerPriceTextObj;
    public List<TextMesh> MonsterCDTextObj;

    public List<TextMesh> SkillPriceTextObj;
    public List<TextMesh> SkillLvTextObj;

    public InGameOperation sceneManager;
    public StageManager stageManager;
    public ResourceManager resourceManager;

    private int[] pendToKart= { 0,0,0,0};
    private int[] costToKart = { 0, 0, 0, 0 };

    private float[] bonusBossCooldown = { 0, 0, 0};

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

    // Start is called before the first frame update
    private void Start()
    {
        sceneManager = FindObjectOfType<InGameOperation>();
        resourceManager = FindObjectOfType<ResourceManager>();
        stageManager = FindObjectOfType<StageManager>();

        ItemPrice = new Dictionary<Upgrades.StoreItems, int[]>();

        ItemPrice.Add(Upgrades.StoreItems.Army1, PriceForArmy1);
        ItemPrice.Add(Upgrades.StoreItems.Army2, PriceForArmy2);
        ItemPrice.Add(Upgrades.StoreItems.Army3, PriceForArmy3);
        ItemPrice.Add(Upgrades.StoreItems.Army4, PriceForArmy4);

        ItemPrice.Add(Upgrades.StoreItems.CastleHP, PriceForCastleHP);
        ItemPrice.Add(Upgrades.StoreItems.BonusBoss1, PriceForBonusBoss1);
        ItemPrice.Add(Upgrades.StoreItems.BonusBoss2, PriceForBonusBoss2);
        ItemPrice.Add(Upgrades.StoreItems.BonusBoss3, PriceForBonusBoss3);

        ItemPrice.Add(Upgrades.StoreItems.MagicMeteor, PriceForMagicMeteor);
        ItemPrice.Add(Upgrades.StoreItems.MagicBlizzard, PriceForMagicBlizzard);
        ItemPrice.Add(Upgrades.StoreItems.MagicPetrification, PriceForMagicPetrification);
        ItemPrice.Add(Upgrades.StoreItems.MagicMinions, PriceForMagicMinions);
    }

    private void Update()
    {
        if (sceneManager.currScreenShown != 0)
        {
            for (int i = 0, s = bonusBossCooldown.Length; i < s; ++i)
            {
                if (bonusBossCooldown[i] > 0)
                {
                    bonusBossCooldown[i]-=Time.deltaTime;
                }
                if (sceneManager.CheckIfTutorial())
                    bonusBossCooldown[i] = 99;
            }

            UpdatePrice();
        }
    }

    private void UpdatePrice() {

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
            int cd = (int)(bonusBossCooldown[i % bonusBossCooldown.Length]);
            MonsterCDTextObj[i].text = "CD" + cd.ToString();
            MonsterCDTextObj[i].color = (cd > 0 || SkillStack.CheckFullStocks()) ? new Color(1, 0, 0, 1) : OriColor;
        }

        for (int i = 0; i < ArmyPriceTextObj.Count; ++i)
        {
            Upgrades.StoreItems itemID = Upgrades.StoreItems.Army1 + i % MaxItemPerCategory;
            int price = ItemPrice[itemID][Upgrades.GetLevel(itemID)];
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
                case 0: ArmyLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.Army1) ? "MAX" :
                        (Upgrades.GetLevel(Upgrades.StoreItems.Army1)+1).ToString()); break;
                case 1: ArmyLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.Army2) ? "MAX" :
                        (Upgrades.GetLevel(Upgrades.StoreItems.Army2) + 1).ToString()); break;
                case 2: ArmyLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.Army3) ? "MAX" :
                        (Upgrades.GetLevel(Upgrades.StoreItems.Army3) + 1).ToString()); break;
                case 3: ArmyLvTextObj[i].text = "LV." + (Upgrades.CheckTopLevel(Upgrades.StoreItems.Army4) ? "MAX" :
                        (Upgrades.GetLevel(Upgrades.StoreItems.Army4) + 1).ToString()); break;
            }
        }

        for (int i = 0; i < SkillPriceTextObj.Count; ++i)
        {
            Upgrades.StoreItems itemID = Upgrades.StoreItems.MagicMeteor + i % MaxItemPerCategory;
            int price = ItemPrice[itemID][Upgrades.GetLevel(itemID)];
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

    public int GetPrice(int itemID) {
        return GetPrice((Upgrades.StoreItems)itemID);
    }

    public int GetPrice(Upgrades.StoreItems itemID)
    {
        if (ItemPrice.ContainsKey(itemID) == false) return 0;

        switch (itemID)
        {
            case Upgrades.StoreItems.BonusBoss1:
                return bonusBossCooldown[0] > 0 ? -1 : 0;
            case Upgrades.StoreItems.BonusBoss2:
                return bonusBossCooldown[1] > 0 ? -1 : 0;
            case Upgrades.StoreItems.BonusBoss3:
                return bonusBossCooldown[2] > 0 ? -1 : 0;
        }

        if (Upgrades.GetLevel(itemID)+1 >= ItemPrice[itemID].Length) {
            return -1;
        }
        return ItemPrice[itemID][Upgrades.GetLevel(itemID)];
    }

    public int GetCost(int itemID) {
        return GetCost((Upgrades.StoreItems)itemID, pendToKart[itemID%10]);
    }

    public int GetCost(Upgrades.StoreItems itemID,int additionLv)
    {
        if (ItemPrice.ContainsKey(itemID) == false) return 0;
        return ItemPrice[itemID][Upgrades.GetLevel(itemID)+ additionLv];
    }


    public void ItemSold(Upgrades.StoreItems itemID)
    {
        Upgrades.StoreUpgrade(itemID, pendToKart[(int)itemID % 10]);

        resourceManager.ChangeMaterial(-1 * costToKart[(int)itemID % 10]);

        pendToKart[(int)itemID % 10] = 0;
        costToKart[(int)itemID % 10] = 0;
    }

    public void ClearToPurchase()
    {
        for (int i = 0, s = pendToKart.Length; i < s; ++i)
            pendToKart[i] = 0;
        for (int i = 0, s = costToKart.Length; i < s; ++i)
            costToKart[i] = 0;
    }

    public int CosttoPurchaseCalculation()
    {
        int totalCost = 0;
        for (int i = 0, s = costToKart.Length; i < s; ++i)
            totalCost += costToKart[i];
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

            pendToKart[(int)itemID % 10]++;
            costToKart[(int)itemID % 10] += price;
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
        if (pendToKart[itemID % 10] < 0) return false;

        pendToKart[itemID % 10]--;
        costToKart[itemID % 10] -= GetCost(itemID);
        return true;
    }

    public void SetBossCD(int bossID)
    {
        bonusBossCooldown[bossID] = cdCounter[bossID];
    }

    public float GetBossCD(int bossID) {
        return bonusBossCooldown[bossID];
    }

    public void raycastAction(Upgrades.StoreItems itemID, int infoID)
    {
        if(ItemPendingAdd(itemID))
        ItemSold(itemID);

        //-1 :subtract 0:purchase 1:add
        switch (infoID) {
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

    public void raycastAction(int fullitemID,int infoID) { //-1 :subtract 0:purchase 1:add
        raycastAction((Upgrades.StoreItems)fullitemID, infoID);
    }
}
