using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class SkillStack
{
    static public readonly int maxStackNum = 4;

    static private int currStackNum;
    static private int[] stackDetail = { 0, 0, 0, 0 };

    static public void init()
    {
        currStackNum = 0;
        for (int i = 0; i < stackDetail.Length; ++i)
            stackDetail[i] = 0;
    }

    static public bool AddStock(Upgrades.StoreItems itemID)
    {
        if (CheckFullStocks())
            return false;
        int emptySlot = -1;
        for (int i = 0; i < maxStackNum; ++i)
        {
            if (GetStock(i) == 0)
            {
                emptySlot = i;
                break;
            }
        }

        stackDetail[emptySlot] = (int)itemID;
        currStackNum++;
        return true;
    }

    static public int UseStock(int StockID)
    {
        if (stackDetail[StockID] == 0)
            return -1;

        int selectedItem = stackDetail[StockID];
        stackDetail[StockID] = 0;
        currStackNum--;
        return selectedItem;
    }

    static public int GetStock(int StockID)
    {
        return stackDetail[StockID];
    }

    static public bool CheckFullStocks()
    {
        return currStackNum >= maxStackNum;
    }
}
