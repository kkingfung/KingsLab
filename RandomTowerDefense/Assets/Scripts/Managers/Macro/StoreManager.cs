using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    private readonly int MaxItemPerCategory = 4;
    private readonly int[] cdCounter = {60,180,300 };
    private readonly Color OriColor = new Color(1, 0.675f, 0, 1 );
    private Dictionary<Upgrades.StoreItems, int[]> ItemPrice;

    public List<TextMesh> ArmyLvTextObj;
    public List<TextMesh> ArmyPriceTextObj;

    public List<TextMesh> TowerHPTextObj;
    public List<TextMesh> TowerPriceTextObj;
    public List<TextMesh> MonsterCDTextObj;

    public List<TextMesh> SkillPriceTextObj;
    public List<TextMesh> SkillLvTextObj;

    InGameOperation sceneManager;
    StageManager stageManager;
    ResourceManager resourceManager;

    int[] pendToKart= { 0,0,0,0};
    int[] costToKart = { 0, 0, 0, 0 };

    int[] bonusBossCooldown = { 0, 0, 0};

    readonly int[] PriceForArmy1 = { 100, 150, 210, 270, 330, 390, 440, 500, 570, 650 };
    readonly int[] PriceForArmy2 = { 100, 150, 210, 270, 330, 390, 440, 500, 570, 650 };
    readonly int[] PriceForArmy3 = { 100, 150, 210, 270, 330, 390, 440, 500, 570, 650 };
    readonly int[] PriceForArmy4 = { 100, 150, 210, 270, 330, 390, 440, 500, 570, 650 };

    readonly int[] PriceForCastleHP = { 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 };
    readonly int[] PriceForBonusBoss1 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    readonly int[] PriceForBonusBoss2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    readonly int[] PriceForBonusBoss3 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    readonly int[] PriceForMagicMeteor = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
    readonly int[] PriceForMagicBlizzard = {10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
    readonly int[] PriceForMagicMinions = {10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
    readonly int[] PriceForMagicPetrification = { 10, 20, 30, 50, 60, 70, 80, 90, 100 };

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
        ItemPrice.Add(Upgrades.StoreItems.MagicMinions, PriceForMagicMinions);
        ItemPrice.Add(Upgrades.StoreItems.MagicPetrification, PriceForMagicPetrification);
    }

    private void Update()
    {
        for (int i = 0, s = bonusBossCooldown.Length; i < s; ++i) {
            if (bonusBossCooldown[i] > 0) {
                bonusBossCooldown[i]--;
            }
            if (sceneManager.CheckIfTutorial())
                bonusBossCooldown[i] = 99;
        }

        UpdatePrice();
    }

    private void UpdatePrice() {

        //int fullitemID = (sceneManager.currScreenShown + 1) * Upgrades.MaxItemPerSlot;

        switch (sceneManager.currScreenShown)
        {
            case (int)InGameOperation.ScreenShownID.SSIDTop:
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
                    int cd = bonusBossCooldown[i % bonusBossCooldown.Length];
                    MonsterCDTextObj[i].text = "CD" + cd.ToString();
                    MonsterCDTextObj[i].color = (cd > 0 || SkillStack.CheckFullStocks()) ? new Color(1, 0, 0, 1) : OriColor;
                }
                break;
            case (int)InGameOperation.ScreenShownID.SSIDTopLeft:
                for (int i = 0; i < ArmyPriceTextObj.Count; ++i) {
                    Upgrades.StoreItems itemID = Upgrades.StoreItems.Army1 + i % MaxItemPerCategory;
                    int price = ItemPrice[itemID][Upgrades.GetLevel(itemID)];
                    if (Upgrades.CheckTopLevel(itemID))
                    {
                        ArmyPriceTextObj[i].text = price.ToString() + "G";
                        ArmyPriceTextObj[i].color = (price > resourceManager.GetCurrMaterial()) ? new Color(1, 0, 0, 1) : OriColor;
                    }
                    else 
                    {
                        ArmyPriceTextObj[i].text = "-";
                        ArmyPriceTextObj[i].color= new Color(0, 0, 0, 1) ;
                    }
                }
 
                for (int i = 0; i < ArmyLvTextObj.Count; ++i)
                {
                    switch (i % MaxItemPerCategory)
                    {
                        case 0: ArmyLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.Army1).ToString(); break;
                        case 1: ArmyLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.Army2).ToString(); break;
                        case 2: ArmyLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.Army3).ToString(); break;
                        case 3: ArmyLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.Army4).ToString(); break;
                    }
                }
                break;
            case (int)InGameOperation.ScreenShownID.SSIDTopRight:
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
                        case 0: SkillLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.MagicMeteor).ToString(); break;
                        case 1: SkillLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard).ToString(); break;
                        case 2: SkillLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.MagicPetrification).ToString(); break;
                        case 3: SkillLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.MagicMinions).ToString(); break;
                    }
                }
                break;
        }
    } 
    public int GetPrice(int itemID) {
        return GetPrice((Upgrades.StoreItems)(itemID + (sceneManager.currScreenShown + 1) * Upgrades.MaxItemPerSlot));
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
        return GetCost((Upgrades.StoreItems)(itemID + (sceneManager.currScreenShown + 1) * Upgrades.MaxItemPerSlot), pendToKart[itemID]);
    }

    public int GetCost(Upgrades.StoreItems itemID,int additionLv)
    {
        if (ItemPrice.ContainsKey(itemID) == false) return 0;
        return ItemPrice[itemID][Upgrades.GetLevel(itemID)+ additionLv];
    }


    public void ItemSold(int itemID)
    {
        Upgrades.StoreItems FullitemID = (Upgrades.StoreItems)(itemID + (sceneManager.currScreenShown + 1) * Upgrades.MaxItemPerSlot);
        Upgrades.StoreUpgrade(FullitemID,pendToKart[itemID]);

        resourceManager.ChangeMaterial(-1 * costToKart[itemID]);

        pendToKart[itemID] = 0;
        costToKart[itemID] = 0;
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

    public bool ItemPendingAdd(int itemID)
    {
        Upgrades.StoreItems fullitemID = (Upgrades.StoreItems)(itemID + (sceneManager.currScreenShown + 1) * Upgrades.MaxItemPerSlot);

        int price = CheckEnoughResource(fullitemID);
        if (price >= 0)
        {
            switch (fullitemID)
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

            pendToKart[itemID]++;
            costToKart[itemID] += price;
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
        if (resourceManager.GetCurrMaterial() - totalCosttoPurchase > price)
            return price;
        return -1;
    }

    public bool ItemPendingSubtract(int itemID)
    {
        if (pendToKart[itemID] < 0) return false;

        pendToKart[itemID]--;
        costToKart[itemID] -= GetCost(itemID);
        return true;
    }

    public void SetBossCD(int bossID)
    {
        bonusBossCooldown[bossID] = cdCounter[bossID];
    }

    public int GetBossCD(int bossID) {
        return bonusBossCooldown[bossID];
    }

    public void raycastAction(int itemID, int infoID)
    {
        if(ItemPendingAdd(itemID))
        ItemSold(itemID);

        //-1 :subtract 0:purchase 1:add
        //switch (infoID) {
        //    case -1:
        //        ItemPendingSubtract(itemID);
        //        break;
        //    case 0:
        //        ItemSold(itemID);
        //        break;
        //    case 1:
        //        ItemPendingAdd(itemID);
        //        break;
        //}
    }

    public void raycastAction(Upgrades.StoreItems fullitemID,int infoID) { //-1 :subtract 0:purchase 1:add
        raycastAction((int)fullitemID% Upgrades.MaxItemPerSlot, infoID);
    }
}
