using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttr
{
    public float damage;
    public float waitTime;
    public float lifetime;
    public float radius;
    public float attackLifetime;

    public TowerAttr(float radius, float damage, float waitTime, float atkLifetime, float lifetime)
    {
        this.damage = damage;
        this.waitTime = waitTime;
        this.radius = radius;
        this.lifetime = lifetime;
        this.attackLifetime = atkLifetime;
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

        towerInfo.Add(TowerInfoID.Enum_TowerNightmare, new TowerAttr(20, 20, 2.0f,3, 5));
        towerInfo.Add(TowerInfoID.Enum_TowerSoulEater, new TowerAttr(10, 8, 0.5f,3, 5));
        towerInfo.Add(TowerInfoID.Enum_TowerTerrorBringer, new TowerAttr(25, 20, 3.0f,3, 5));
        towerInfo.Add(TowerInfoID.Enum_TowerUsurper, new TowerAttr(8, 5, 0.2f,1, 5));
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
