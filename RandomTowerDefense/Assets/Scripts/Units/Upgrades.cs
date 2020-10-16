using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Upgrades
{
    static public readonly int MaxLevel = 10;
    static public readonly int MaxItemPerSlot = 10;

    public enum StoreItems {
       Army1 = 20, //SoulEater
       Army2,//Nightmare
       Army3,//TerrorBringer
       Army4,//Usurper

       MoneyCollector = 30,
       BonusBoss1,//Green Metalon
       BonusBoss2,//Purple Metalon
       BonusBoss3,//Red Metalon

       MagicMeteor = 40,//Fire
       MagicBlizzard,//Ice
       MagicSummon,//Metal
       MagicPetrification,//Mind
    }

    static public Dictionary<StoreItems, int> StoreLevel;

    // Start is called before the first frame update
    static public void init()
    {
        StoreLevel = new Dictionary<StoreItems, int>();

        StoreLevel.Add(StoreItems.Army1, 0);
        StoreLevel.Add(StoreItems.Army2, 0);
        StoreLevel.Add(StoreItems.Army3, 0);
        StoreLevel.Add(StoreItems.Army4, 0);

        StoreLevel.Add(StoreItems.MoneyCollector, 0);
        //StoreLevel.Add(StoreItems.BonusBoss1, 0);
        //StoreLevel.Add(StoreItems.BonusBoss2, 0);
        //StoreLevel.Add(StoreItems.BonusBoss3, 0);

        StoreLevel.Add(StoreItems.MagicMeteor, 0);
        StoreLevel.Add(StoreItems.MagicBlizzard, 0);
        StoreLevel.Add(StoreItems.MagicSummon, 0);
        StoreLevel.Add(StoreItems.MagicPetrification, 0);
    }

    static public int GetLevel(StoreItems itemID) {
        if (StoreLevel.ContainsKey(itemID) == false) return -1;
        return StoreLevel[itemID];
    }

    static public bool AddLevel(StoreItems itemID,int lvUP)
    {
        if (StoreLevel.ContainsKey(itemID) == false) return false;
        if (StoreLevel[itemID] + lvUP > MaxLevel) return false;

        StoreLevel[itemID] += lvUP;
        return true;
    }
}
