using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttr
{
    public int damage;
    public int frameWait;
    public float areaSq;

    public TowerAttr(float areaSq, int damage = 1, int frameWait = 0)
    {
        this.damage = damage;
        this.frameWait = frameWait;
        this.areaSq = areaSq;
    }
}

public static class TowerInfo
{
    static Dictionary<string, TowerAttr> towerInfo;
    public static void Init()
    {
        towerInfo = new Dictionary<string, TowerAttr>();

        towerInfo.Add("TowerNightmare", new TowerAttr(70, 80, 30));
        towerInfo.Add("TowerSoulEater", new TowerAttr(50, 20, 10));
        towerInfo.Add("TowerTerrorBringer", new TowerAttr(100, 150, 45));
        towerInfo.Add("TowerUsurper", new TowerAttr(30, 30, 5));
    }

    static void Release()
    {
        towerInfo.Clear();
    }

    public static TowerAttr GetTowerInfo(string towerName)
    {
        if (towerInfo.ContainsKey(towerName))
            return towerInfo[towerName];
        return null;
    }
}
