using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class TowerAttr
{
    public float damage;
    public float waitTime;
    public float lifetime;
    public float radius;
    public float attackLifetime;
    public float attackWaittime;
    public float attackRadius;
    public float attackSpd;

    public TowerAttr(float radius, float damage, float waitTime, float attackLifetime, float attackWaittime,
        float attackRadius, float attackSpd, float lifetime =0.5f)
    {
        this.damage = damage;
        this.waitTime = waitTime;
        this.radius = radius;
        this.lifetime = lifetime;
        this.attackLifetime = attackLifetime;
        this.attackWaittime = attackWaittime;
        this.attackRadius = attackRadius;
        this.attackSpd = attackSpd;
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

        towerInfo.Add(TowerInfoID.Enum_TowerNightmare, 
            new TowerAttr(
                20, 1, //radius,damage
                2.0f, 3, //wait,atklife
                0.01f, 1,//atkwait,atkrad
                0,5));//atkspd,lifetime
        towerInfo.Add(TowerInfoID.Enum_TowerSoulEater,
            new TowerAttr(
                13, 8,//radius,damage
                0.5f, 3, //wait,atklife
                0f, 0.5f, //atkwait,atkrad
                1f, 5));//atkspd,lifetime
        towerInfo.Add(TowerInfoID.Enum_TowerTerrorBringer,
            new TowerAttr(
                40, 20,//radius,damage
                3.0f, 3,//wait,atklife
                0.01f, 2,//atkwait,atkrad
                0,5));//atkspd,lifetime
        towerInfo.Add(TowerInfoID.Enum_TowerUsurper,
            new TowerAttr(
                8, 0.3f,//radius,damage
                0.2f, 1,//wait,atklife
                0f, 0.5f,//atkwait,atkrad
                1f,5));//atkspd,lifetime
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
