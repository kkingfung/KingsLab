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

       CastleHP = 30,
       BonusBoss1,//Green Metalon
       BonusBoss2,//Purple Metalon
       BonusBoss3,//Red Metalon

        MagicMeteor = 40,//Fire
        MagicBlizzard,//Ice
        MagicPetrification,//Mind
        MagicMinions,//Metal
    }

    static public Dictionary<StoreItems, int> StoreLevel;
    static private CastleSpawner castleSpawner;
    static private StoreManager StoreManager;
    // Start is called before the first frame update
    static public void init()
    {
        StoreLevel = new Dictionary<StoreItems, int>();

        StoreLevel.Add(StoreItems.Army1, 0);
        StoreLevel.Add(StoreItems.Army2, 0);
        StoreLevel.Add(StoreItems.Army3, 0);
        StoreLevel.Add(StoreItems.Army4, 0);

        StoreLevel.Add(StoreItems.CastleHP, 0);
        StoreLevel.Add(StoreItems.BonusBoss1, 0);
        StoreLevel.Add(StoreItems.BonusBoss2, 0);
        StoreLevel.Add(StoreItems.BonusBoss3, 0);

        StoreLevel.Add(StoreItems.MagicMeteor, 0);
        StoreLevel.Add(StoreItems.MagicBlizzard, 0);
        StoreLevel.Add(StoreItems.MagicMinions, 0);
        StoreLevel.Add(StoreItems.MagicPetrification, 0);

        castleSpawner=GameObject.FindObjectOfType<CastleSpawner>();
        StoreManager = GameObject.FindObjectOfType<StoreManager>();
    }

    static public int GetLevel(StoreItems itemID) {
        if (StoreLevel.ContainsKey(itemID) == false) return -1;
        return StoreLevel[itemID];
    }

    static public bool AddSkillLevel(StoreItems itemID, int lvUP)
    {
        if (StoreLevel.ContainsKey(itemID) == false) return false;
        if (StoreLevel[itemID] + lvUP > MaxLevel) return false;
        StoreLevel[itemID] += lvUP;
        return true;
    }

        static public bool StoreUpgrade(StoreItems itemID,int lvUP)
    {
        if (StoreLevel.ContainsKey(itemID) == false) return false;
        if (StoreLevel[itemID] + lvUP > MaxLevel) return false;

        //Checking StockItems
        switch (itemID)
        {
            case StoreItems.CastleHP:
                castleSpawner.castle.AddedHealth();
                break;
            case StoreItems.BonusBoss1: 
            case StoreItems.BonusBoss2:
            case StoreItems.BonusBoss3:
                SkillStack.AddStock(itemID);
                StoreManager.SetBossCD((int)(itemID- Upgrades.StoreItems.BonusBoss1));
                break;
            case Upgrades.StoreItems.Army1:
            case Upgrades.StoreItems.Army2:
            case Upgrades.StoreItems.Army3:
            case Upgrades.StoreItems.Army4:
                StoreLevel[itemID] += lvUP;
                break;
            case StoreItems.MagicMeteor:
            case StoreItems.MagicBlizzard:
            case StoreItems.MagicMinions:
            case StoreItems.MagicPetrification:
                SkillStack.AddStock(itemID);
                break;
        }
        return true;
    }

    static public bool CheckTopLevel(StoreItems itemID)
    {
        if (itemID == StoreItems.Army1 || itemID == StoreItems.Army2
            || itemID == StoreItems.Army3 || itemID == StoreItems.Army4)
            return StoreLevel[itemID] < MaxLevel;
        return true;
    }

    static public int allLevel() 
    { 
    int totalLv = 0;
        totalLv += GetLevel(StoreItems.Army1) + GetLevel(StoreItems.Army2) + GetLevel(StoreItems.Army3) + GetLevel(StoreItems.Army4);
        totalLv += GetLevel(StoreItems.MagicMeteor) + GetLevel(StoreItems.MagicBlizzard) + GetLevel(StoreItems.MagicMinions) + GetLevel(StoreItems.MagicPetrification);
        return totalLv;
    }
}
