using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

using System.IO;


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
                0.01f, 1,//atkwait,atkrad
                0,5));//atkspd,lifetime
        towerInfo.Add(TowerInfoID.Enum_TowerUsurper,
            new TowerAttr(
                8, 2f,//radius,damage
                0.2f, 1,//wait,atklife
                0f, 0.5f,//atkwait,atkrad
                1f,5));//atkspd,lifetime
    }

    public static void InitByFile(string filepath)
    {
        StreamReader inp_stm = new StreamReader(filepath);

        towerInfo = new Dictionary<TowerInfoID, TowerAttr>();

        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            string[] seperateInfo = inp_ln.Split(':');
            if(seperateInfo.Length==9)
            towerInfo.Add((TowerInfoID)int.Parse(seperateInfo[0]), new TowerAttr(
                float.Parse(seperateInfo[1]), float.Parse(seperateInfo[2]),
                float.Parse(seperateInfo[3]), float.Parse(seperateInfo[4]),
                float.Parse(seperateInfo[5]), float.Parse(seperateInfo[6]),
                float.Parse(seperateInfo[7]), float.Parse(seperateInfo[8])));
           // Debug.Log(seperateInfo.Length);
        }

        inp_stm.Close();
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
