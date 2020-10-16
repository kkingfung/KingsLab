using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class SkillStack
{
    static public readonly int maxStackNum = 4;

    static int currStackNum;
    static int[] stackDetail = { 0, 0, 0, 0 };


    static public void init()
    {
        for(int i =0,s = stackDetail.Length; i<s; i++){
            stackDetail[i] = 0;
        }
    }

    static public bool AddStock(Upgrades.StoreItems itemID) {
        if (currStackNum == maxStackNum)
            return false;

        stackDetail[currStackNum]=(int)itemID;
        currStackNum++;
        return true;
    }

    static public int UseStock(int StockID)
    {
        if (stackDetail[StockID]==0)
            return -1;

        int selectedItem = stackDetail[StockID];
        stackDetail[StockID] = 0;
        return selectedItem;
    }

    static public int GetStock(int StockID)
    {
        return stackDetail[StockID];
    }
}
