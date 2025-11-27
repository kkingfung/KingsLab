using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.RemoteConfig;

using System.IO;

namespace RandomTowerDefense.Info
{
    /// <summary>
    /// タワーの戦闘属性情報を格納するクラス
    /// </summary>
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

        public TowerAttr(float radius, float damage, float waitTime, float lifetime, float attackWaittime,
            float attackRadius, float attackSpd, float attackLifetime)
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

    /// <summary>
    /// タワー情報静的クラス - 4種類タワーの戦闘データ定義と管理
    ///
    /// 主な機能:
    /// - 4種類タワー（Nightmare、SoulEater、TerrorBringer、Usurper）のステータス管理
    /// - ランク別戦闘属性（ダメージ、射程、攻撃速度、持続時間）設定
    /// - JSON設定ファイル読み込みとリモート設定統合
    /// - タワー種別列挙型と属性辞書による効率的データアクセス
    /// - ゲーム内バランス調整とタワー特性定義システム
    /// </summary>
    public static class TowerInfo
    {
        static Dictionary<TowerInfoID, TowerAttr> towerInfo;
        public static bool infoUpdated = false;

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
                3.5f, 25, //radius,damage
                3f, 5f, //wait,lifetime
                0.01f, 0.3f,//atkwait,atkrad
                0,3));//atkspd,atklife
        towerInfo.Add(TowerInfoID.Enum_TowerSoulEater,
            new TowerAttr(
                2.5f, 7,//radius,damage
                0.3f, 5, //wait,lifetime
                0f, 0.8f, //atkwait,atkrad
                5f, 3));//atkspd,atklife
        towerInfo.Add(TowerInfoID.Enum_TowerTerrorBringer,
            new TowerAttr(
                4, 50,//radius,damage
                5f, 5,//wait,lifetime
                0.01f, 0.2f,//atkwait,atkrad
                0,3));//atkspd,atklife
        towerInfo.Add(TowerInfoID.Enum_TowerUsurper,
            new TowerAttr(
                2, 5f,//radius,damage
                0.2f, 5,//wait,lifetime
                0f, 0.5f,//atkwait,atkrad
                3.5f,1));//atkspd,atklife
        infoUpdated = true;
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

        infoUpdated = true;
    }

    public static void InitByRemote(ConfigResponse response)
    {
        towerInfo = new Dictionary<TowerInfoID, TowerAttr>();

        towerInfo.Add(TowerInfoID.Enum_TowerNightmare,
            new TowerAttr(
                ConfigManager.appConfig.GetFloat("TowerNightmareRadius"), ConfigManager.appConfig.GetFloat("TowerNightmareDamage"), //radius,damage
                ConfigManager.appConfig.GetFloat("TowerNightmareWait"), ConfigManager.appConfig.GetFloat("TowerNightmareLife"), //wait,atklife
                ConfigManager.appConfig.GetFloat("TowerNightmareAtkWait"), ConfigManager.appConfig.GetFloat("TowerNightmareAtkRadius"),//atkwait,atkrad
                ConfigManager.appConfig.GetFloat("TowerNightmareAtkSpd"), ConfigManager.appConfig.GetFloat("TowerNightmareAtkLife")));//atkspd,lifetime

        towerInfo.Add(TowerInfoID.Enum_TowerSoulEater,
            new TowerAttr(
                ConfigManager.appConfig.GetFloat("TowerSoulEaterRadius"), ConfigManager.appConfig.GetFloat("TowerSoulEaterDamage"), //radius,damage
                ConfigManager.appConfig.GetFloat("TowerSoulEaterWait"), ConfigManager.appConfig.GetFloat("TowerSoulEaterLife"), //wait,atklife
                ConfigManager.appConfig.GetFloat("TowerSoulEaterAtkWait"), ConfigManager.appConfig.GetFloat("TowerSoulEaterAtkRadius"),//atkwait,atkrad
                ConfigManager.appConfig.GetFloat("TowerSoulEaterAtkSpd"), ConfigManager.appConfig.GetFloat("TowerSoulEaterAtkLife")));//atkspd,lifetime

        towerInfo.Add(TowerInfoID.Enum_TowerTerrorBringer,
            new TowerAttr(
                ConfigManager.appConfig.GetFloat("TowerTerrorBringerRadius"), ConfigManager.appConfig.GetFloat("TowerTerrorBringerDamage"), //radius,damage
                ConfigManager.appConfig.GetFloat("TowerTerrorBringerWait"), ConfigManager.appConfig.GetFloat("TowerTerrorBringerLife"), //wait,atklife
                ConfigManager.appConfig.GetFloat("TowerTerrorBringerAtkWait"), ConfigManager.appConfig.GetFloat("TowerTerrorBringerAtkRadius"),//atkwait,atkrad
                ConfigManager.appConfig.GetFloat("TowerTerrorBringerAtkSpd"), ConfigManager.appConfig.GetFloat("TowerTerrorBringerAtkLife")));//atkspd,lifetime

        towerInfo.Add(TowerInfoID.Enum_TowerUsurper,
            new TowerAttr(
                ConfigManager.appConfig.GetFloat("TowerUsurperRadius"), ConfigManager.appConfig.GetFloat("TowerUsurperDamage"), //radius,damage
                ConfigManager.appConfig.GetFloat("TowerUsurperWait"), ConfigManager.appConfig.GetFloat("TowerUsurperLife"), //wait,atklife
                ConfigManager.appConfig.GetFloat("TowerUsurperAtkWait"), ConfigManager.appConfig.GetFloat("TowerUsurperAtkRadius"),//atkwait,atkrad
                ConfigManager.appConfig.GetFloat("TowerUsurperAtkSpd"), ConfigManager.appConfig.GetFloat("TowerUsurperAtkLife")));//atkspd,lifetime

        infoUpdated = true;
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
}