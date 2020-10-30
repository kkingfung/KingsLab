using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttr
{
    public float damage;
    public float waitTime;
    public float lifetime;
    public float radius;

    public TowerAttr(float radius, float damage, float waitTime, float lifetime=5)
    {
        this.damage = damage;
        this.waitTime = waitTime;
        this.radius = radius;
        this.lifetime = lifetime;
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

        towerInfo.Add(TowerInfoID.Enum_TowerNightmare, new TowerAttr(300, 20, 2.0f, 5));
        towerInfo.Add(TowerInfoID.Enum_TowerSoulEater, new TowerAttr(150, 8, 0.5f, 5));
        towerInfo.Add(TowerInfoID.Enum_TowerTerrorBringer, new TowerAttr(500, 20, 3.0f, 5));
        towerInfo.Add(TowerInfoID.Enum_TowerUsurper, new TowerAttr(100, 5, 0.2f, 5));
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
