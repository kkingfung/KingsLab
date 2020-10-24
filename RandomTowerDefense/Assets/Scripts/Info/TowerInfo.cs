using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttr
{
    public float damage;
    public int frameWait;
    public float areaSq;

    public TowerAttr(float areaSq, float damage = 1, int frameWait = 0)
    {
        this.damage = damage;
        this.frameWait = frameWait;
        this.areaSq = areaSq;
    }
}

public static class TowerInfo
{
    static Dictionary<TowerInfoID, TowerAttr> towerInfo;

    public enum TowerInfoID
    {
        Enum_TowerNightmare = 0,
        Enum_TowerSoulEater,
        Enum_TowerTerrorBringer,
        Enum_TowerUsurper
    }

    public static void Init()
    {
        towerInfo = new Dictionary<TowerInfoID, TowerAttr>();

        towerInfo.Add(TowerInfoID.Enum_TowerNightmare, new TowerAttr(80, 80, 150));
        towerInfo.Add(TowerInfoID.Enum_TowerSoulEater, new TowerAttr(60, 20, 50));
        towerInfo.Add(TowerInfoID.Enum_TowerTerrorBringer, new TowerAttr(120, 150, 220));
        towerInfo.Add(TowerInfoID.Enum_TowerUsurper, new TowerAttr(40, 30, 25));
    }

    static void Release()
    {
        towerInfo.Clear();
    }

    public static TowerAttr GetTowerInfo(TowerInfoID towerType)
    {
        if (towerInfo.ContainsKey(towerType))
        {
            return towerInfo[towerType];
        }
        return null;
    }
}
