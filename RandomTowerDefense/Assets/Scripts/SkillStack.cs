using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class SkillStack
{
    static public readonly int maxStackNum = 4;

    static private int currStackNum;
    static private int[] stackDetail = { 0, 0, 0, 0 };

    static private Material[] stackMaterials;

    static public void init()
    {
        stackMaterials = new Material[4];
        for(int i =0,s = stackDetail.Length; i<s; i++){
            stackDetail[i] = 0;
            stackMaterials[i] = Resources.Load("Materials/StocksMaterial/IconNullMat.mat", typeof(Material)) as Material;
        }
    }

    static public bool AddStock(Upgrades.StoreItems itemID) {
        if (CheckFullStocks())
            return false;

        int emptySlot = -1;
        for (int i = 0; i < maxStackNum; ++i)
        {
            if (GetStock(i) == 0) {
                emptySlot = i;
                break;
            }
        }

        stackDetail[emptySlot] =(int)itemID;
        switch (itemID) {
            case Upgrades.StoreItems.BonusBoss1:
                stackMaterials[emptySlot] = Resources.Load("Materials/StocksMaterial/IconBoss1Mat.mat", typeof(Material)) as Material;
                break;
            case Upgrades.StoreItems.BonusBoss2:
                stackMaterials[emptySlot] = Resources.Load("Materials/StocksMaterial/IconBoss2Mat.mat", typeof(Material)) as Material;
                break;
            case Upgrades.StoreItems.BonusBoss3:
                stackMaterials[emptySlot] = Resources.Load("Materials/StocksMaterial/IconBoss3Mat.mat", typeof(Material)) as Material;
                break;
            case Upgrades.StoreItems.MagicMeteor:
                stackMaterials[emptySlot] = Resources.Load("Materials/StocksMaterial/IconSkillMeteorMat.mat", typeof(Material)) as Material;
                break;
            case Upgrades.StoreItems.MagicBlizzard:
                stackMaterials[emptySlot] = Resources.Load("Materials/StocksMaterial/IconSkillBlizzardMat.mat", typeof(Material)) as Material;
                break;
            case Upgrades.StoreItems.MagicSummon:
                stackMaterials[emptySlot] = Resources.Load("Materials/StocksMaterial/IconSkillMinionsMat.mat", typeof(Material)) as Material;
                break;
            case Upgrades.StoreItems.MagicPetrification:
                stackMaterials[emptySlot] = Resources.Load("Materials/StocksMaterial/IconSkillPetrificationMat.mat", typeof(Material)) as Material;
                break;
        }

        currStackNum++;
        return true;
    }

    static public int UseStock(int StockID)
    {
        if (stackDetail[StockID]==0)
            return -1;

        int selectedItem = stackDetail[StockID];
        stackDetail[StockID] = 0;
        stackMaterials[StockID] = Resources.Load("Materials/StocksMaterial/IconNullMat.mat", typeof(Material)) as Material;
        return selectedItem;
    }

    static public int GetStock(int StockID)
    {
        return stackDetail[StockID];
    }

    static public bool CheckFullStocks()
    {
        return currStackNum>=maxStackNum;
    }
}
