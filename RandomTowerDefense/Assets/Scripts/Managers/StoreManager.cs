using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    private readonly int MaxStoreCategories = 3;
    private readonly int MaxItemPerCategory = 4;
    private readonly int[] cdCounter = {60,180,300 };
    private Dictionary<Upgrades.StoreItems, int[]> ItemPrice;

    public List<GameObject> PriceTextObj;
    public List<GameObject> ArmyLvTextObj;
    public List<GameObject> SkillLvTextObj;

    InGameOperation sceneManager;
    StageManager stageManager;
    ResourceManager resourceManager;

    int[] pendToKart= { 0,0,0,0};
    int[] costToKart = { 0, 0, 0, 0 };

    int[] bonusBossCooldown = { 0, 0, 0};

    readonly int[] PriceForArmy1 = { 20, 50, 100, 200, 500, 800, 1000, 1200, 1500, 2000 };
    readonly int[] PriceForArmy2 = { 20, 50, 100, 200, 500, 800, 1000, 1200, 1500, 2000 };
    readonly int[] PriceForArmy3 =  { 20, 50, 100, 200, 500, 800, 1000, 1200, 1500, 2000 };
    readonly int[] PriceForArmy4 = { 20, 50, 100, 200, 500, 800, 1000, 1200, 1500, 2000 };

    readonly int[] PriceForCastleHP = { 10, 20, 30, 50, 100, 200, 300, 500, 750, 1000 };
    readonly int[] PriceForBonusBoss1 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    readonly int[] PriceForBonusBoss2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    readonly int[] PriceForBonusBoss3 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    readonly int[] PriceForMagicMeteor = { 5, 10, 20, 30, 50, 70, 100, 120, 150, 200 };
    readonly int[] PriceForMagicBlizzard = { 5, 10, 20, 30, 50, 70, 100, 120, 150, 200 };
    readonly int[] PriceForMagicSummon = { 5, 10, 20, 30, 50, 70, 100, 120, 150, 200 };
    readonly int[] PriceForMagicPetrification = { 5, 10, 20, 30, 50, 70, 100, 120, 150, 200 };

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
        ItemPrice.Add(Upgrades.StoreItems.MagicSummon, PriceForMagicSummon);
        ItemPrice.Add(Upgrades.StoreItems.MagicPetrification, PriceForMagicPetrification);

        UpdateAllItemsLvShown();
    }

    private void Update()
    {
        for (int i = 0, s = bonusBossCooldown.Length; i < s; ++i) {
            if (bonusBossCooldown[i] > 0) {
                bonusBossCooldown[i]--;
            }
        }

        if (sceneManager.currScreenShown== (int)InGameOperation.ScreenShownID.SSIDTop)
            for (int i = 0; i < PriceTextObj.Count; ++i)
        {
           if(i % MaxItemPerCategory!=0)
                        PriceTextObj[i].GetComponent<Text>().text = "CD" + bonusBossCooldown[i % MaxItemPerCategory - 1].ToString();
        }
    }

    private void UpdatePrice() {
        if (sceneManager.currScreenShown != 0)
        {
            for (int i = 0; i < PriceTextObj.Count; ++i)
            {
                switch ((i % MaxItemPerCategory) % MaxStoreCategories)
                {
                    case 0:
                        PriceTextObj[i].GetComponent<Text>().text =
                        ItemPrice[Upgrades.StoreItems.Army1 + i][Upgrades.GetLevel(Upgrades.StoreItems.Army1 + i)].ToString() + "G"; break;
                    case 1:
                        if (i % MaxItemPerCategory == 0)
                        {
                            PriceTextObj[i].GetComponent<Text>().text =
                            ItemPrice[Upgrades.StoreItems.CastleHP + i][Upgrades.GetLevel(Upgrades.StoreItems.CastleHP + i)].ToString() + "G";
                        }
                        else
                        {
                            PriceTextObj[i].GetComponent<Text>().text = "CD" + bonusBossCooldown[i % MaxItemPerCategory - 1].ToString();
                        }
                        break;
                    case 2:
                        PriceTextObj[i].GetComponent<Text>().text =
                        ItemPrice[Upgrades.StoreItems.MagicMeteor + i][Upgrades.GetLevel(Upgrades.StoreItems.MagicMeteor + i)].ToString() + "G"; break;
                }
            }
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
                return bonusBossCooldown[0];
            case Upgrades.StoreItems.BonusBoss2:
                return bonusBossCooldown[1];
            case Upgrades.StoreItems.BonusBoss3:
                return bonusBossCooldown[2];
        }

        return ItemPrice[itemID][Upgrades.GetLevel(itemID) + 1];
    }

    public int GetPreviousCost(int itemID) {
        return GetPreviousCost((Upgrades.StoreItems)(itemID + (sceneManager.currScreenShown + 1) * Upgrades.MaxItemPerSlot), pendToKart[itemID]);
    }

    public int GetPreviousCost(Upgrades.StoreItems itemID,int additionLv)
    {
        if (ItemPrice.ContainsKey(itemID) == false) return 0;
        return ItemPrice[itemID][Upgrades.GetLevel(itemID)+ additionLv];
    }


    public void ItemSold(int itemID)
    {
        Upgrades.StoreItems FullitemID = (Upgrades.StoreItems)(itemID + (sceneManager.currScreenShown + 1) * Upgrades.MaxItemPerSlot);
        Upgrades.AddLevel(FullitemID,pendToKart[itemID]);

        resourceManager.ChangeMaterial(-1 * costToKart[itemID]);

        UpdatePrice();
        UpdateAllItemsLvShown();

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
        
        //Checking StockItems
        bool isBonusBoss = false;

        switch (fullitemID) {
            case Upgrades.StoreItems.CastleHP:
                break;
            case Upgrades.StoreItems.BonusBoss1:
                isBonusBoss = GetBossCD(0) == 0; break;
            case Upgrades.StoreItems.BonusBoss2:
                isBonusBoss = GetBossCD(1) == 0; break;
            case Upgrades.StoreItems.BonusBoss3:
                isBonusBoss = GetBossCD(2) == 0; break;
            case Upgrades.StoreItems.MagicMeteor:
                if (GetPreviousCost(Upgrades.StoreItems.MagicMeteor, 0) <= resourceManager.GetCurrMaterial())
                    return SkillStack.AddStock(fullitemID);
                break;
            case Upgrades.StoreItems.MagicBlizzard:
                if (GetPreviousCost(Upgrades.StoreItems.MagicBlizzard, 0) <= resourceManager.GetCurrMaterial())
                    return SkillStack.AddStock(fullitemID);
                break;
            case Upgrades.StoreItems.MagicSummon:
                if (GetPreviousCost(Upgrades.StoreItems.MagicSummon, 0) <= resourceManager.GetCurrMaterial())
                    return SkillStack.AddStock(fullitemID);
                break;
            case Upgrades.StoreItems.MagicPetrification:
                if (GetPreviousCost(Upgrades.StoreItems.MagicPetrification, 0) <= resourceManager.GetCurrMaterial())
                    return SkillStack.AddStock(fullitemID);
                break;
        }

        if (isBonusBoss) {
            if (SkillStack.AddStock(fullitemID)){
                bonusBossCooldown[itemID - 1] = cdCounter[itemID - 1];
                return true;
            }
        }

        //For Non-StockItems
        int price = CheckItemAdd(fullitemID);
        if (price > 0)
        {
            pendToKart[itemID]++;
            costToKart[itemID] += price;
            return true;
        }
        return false;
    }

    public int CheckItemAdd(Upgrades.StoreItems itemID)
    {
        int totalCosttoPurchase = CosttoPurchaseCalculation();
        int price = GetPrice(itemID);

        if (resourceManager.GetCurrMaterial() - totalCosttoPurchase > price)
            return price;
        return -1;
    }

    public bool ItemPendingSubtract(int itemID)
    {
        if (pendToKart[itemID] < 0) return false;

        pendToKart[itemID]--;
        costToKart[itemID] -= GetPreviousCost(itemID);
        return true;
    }

    private int GetBossCD(int bossID) {
        return bonusBossCooldown[bossID];
    }

    public void raycastAction(int itemID, int infoID)
    {
        ItemPendingAdd(itemID);
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

    public void UpdateAllItemsLvShown()
    {
        for (int i = 0; i < ArmyLvTextObj.Count; ++i) {
            switch (i % MaxItemPerCategory) {
                case 0: ArmyLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.Army1).ToString(); break;
                case 1: ArmyLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.Army2).ToString(); break;
                case 2: ArmyLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.Army3).ToString(); break;
                case 3: ArmyLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.Army4).ToString(); break;
            }
        
        }

        for (int i = 0; i < SkillLvTextObj.Count; ++i)
        {
            switch (i % MaxItemPerCategory)
            {
                case 0: SkillLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.MagicMeteor).ToString(); break;
                case 1: SkillLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.MagicBlizzard).ToString(); break;
                case 2: SkillLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.MagicPetrification).ToString(); break;
                case 3: SkillLvTextObj[i].GetComponent<TextMesh>().text = "LV." + Upgrades.GetLevel(Upgrades.StoreItems.MagicSummon).ToString(); break;
            }

        }
    }
}
