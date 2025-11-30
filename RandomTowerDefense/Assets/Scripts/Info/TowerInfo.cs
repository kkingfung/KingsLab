using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.RemoteConfig;
using System.IO;

namespace RandomTowerDefense.Info
{
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
        /// <summary>
        /// タワー情報ID列挙型
        /// </summary>
        public enum TowerInfoID
        {
            EnumTowerNightmare = 0,
            EnumTowerSoulEater,
            EnumTowerTerrorBringer,
            EnumTowerUsurper
        }

        /// <summary>
        /// タワー属性クラス
        /// </summary>
        public static Dictionary<TowerInfoID, TowerAttr> TowerInfos => towerInfos;
        /// <summary>
        /// 情報更新フラグ
        /// </summary>
        public static bool InfoUpdated = false;

        /// <summary>
        /// タワー属性辞書
        /// </summary>
        static Dictionary<TowerInfoID, TowerAttr> towerInfos;

        /// <summary>
        /// 初期化メソッド
        /// </summary>
        public static void Init()
        {
            towerInfos = new Dictionary<TowerInfoID, TowerAttr>();

            towerInfos.Add(TowerInfoID.EnumTowerNightmare,
                new TowerAttr(
                    3.5f, 25, // radius, damage
                    3f, 5f, // wait, lifetime
                    0.01f, 0.3f, // atkwait, atkrad
                    0, 3)); // atkspd, atklife
            towerInfos.Add(TowerInfoID.EnumTowerSoulEater,
                new TowerAttr(
                    2.5f, 7, // radius, damage
                    0.3f, 5, // wait, lifetime
                    0f, 0.8f, // atkwait, atkrad
                    5f, 3)); // atkspd, atklife
            towerInfos.Add(TowerInfoID.EnumTowerTerrorBringer,
                new TowerAttr(
                    4, 50, // radius, damage
                    5f, 5, // wait, lifetime
                    0.01f, 0.2f, // atkwait, atkrad
                    0, 3)); // atkspd, atklife
            towerInfos.Add(TowerInfoID.EnumTowerUsurper,
                new TowerAttr(
                    2, 5f, // radius, damage
                    0.2f, 5, // wait, lifetime
                    0f, 0.5f, // atkwait, atkrad
                    3.5f, 1)); // atkspd, atklife
            infoUpdated = true;
        }

        /// <summary>
        /// ファイルから初期化メソッド
        /// </summary>
        /// <param name="filepath">ファイルパス</param>
        public static void InitByFile(string filepath)
        {
            StreamReader inp_stm = new StreamReader(filepath);

            towerInfos = new Dictionary<TowerInfoID, TowerAttr>();

            while (!inp_stm.EndOfStream)
            {
                string inp_ln = inp_stm.ReadLine();
                string[] seperateInfo = inp_ln.Split(':');
                if (seperateInfo.Length == 9)
                {
                    towerInfos.Add((TowerInfoID)int.Parse(seperateInfo[0]), new TowerAttr(
                        float.Parse(seperateInfo[1]), float.Parse(seperateInfo[2]),
                        float.Parse(seperateInfo[3]), float.Parse(seperateInfo[4]),
                        float.Parse(seperateInfo[5]), float.Parse(seperateInfo[6]),
                        float.Parse(seperateInfo[7]), float.Parse(seperateInfo[8])));
                }
            }

            inp_stm.Close();

            infoUpdated = true;
        }

        /// <summary>
        /// リモート設定から初期化メソッド
        /// </summary>
        /// <param name="response">設定レスポンス</param>
        public static void InitByRemote(ConfigResponse response)
        {
            towerInfos = new Dictionary<TowerInfoID, TowerAttr>();
            var _appConfig = ConfigManager.appConfig;
            towerInfos.Add(TowerInfoID.EnumTowerNightmare,
                new TowerAttr(
                    _appConfig.GetFloat("TowerNightmareRadius"), _appConfig.GetFloat("TowerNightmareDamage"), //radius,damage
                    _appConfig.GetFloat("TowerNightmareWait"), _appConfig.GetFloat("TowerNightmareLife"), // wait,atklife
                    _appConfig.GetFloat("TowerNightmareAtkWait"), _appConfig.GetFloat("TowerNightmareAtkRadius"),//atkwait,atkrad
                    _appConfig.GetFloat("TowerNightmareAtkSpd"), _appConfig.GetFloat("TowerNightmareAtkLife")));//atkspd,lifetime

            towerInfos.Add(TowerInfoID.EnumTowerSoulEater,
                new TowerAttr(
                    _appConfig.GetFloat("TowerSoulEaterRadius"), _appConfig.GetFloat("TowerSoulEaterDamage"), //radius,damage
                    _appConfig.GetFloat("TowerSoulEaterWait"), _appConfig.GetFloat("TowerSoulEaterLife"), //wait,atklife
                    _appConfig.GetFloat("TowerSoulEaterAtkWait"), _appConfig.GetFloat("TowerSoulEaterAtkRadius"),//atkwait,atkrad
                    _appConfig.GetFloat("TowerSoulEaterAtkSpd"), _appConfig.GetFloat("TowerSoulEaterAtkLife")));//atkspd,lifetime

            towerInfos.Add(TowerInfoID.EnumTowerTerrorBringer,
                new TowerAttr(
                    _appConfig.GetFloat("TowerTerrorBringerRadius"), _appConfig.GetFloat("TowerTerrorBringerDamage"), //radius,damage
                    _appConfig.GetFloat("TowerTerrorBringerWait"), _appConfig.GetFloat("TowerTerrorBringerLife"), //wait,atklife
                    _appConfig.GetFloat("TowerTerrorBringerAtkWait"), _appConfig.GetFloat("TowerTerrorBringerAtkRadius"),//atkwait,atkrad
                    _appConfig.GetFloat("TowerTerrorBringerAtkSpd"), _appConfig.GetFloat("TowerTerrorBringerAtkLife")));//atkspd,lifetime

            towerInfos.Add(TowerInfoID.EnumTowerUsurper,
                new TowerAttr(
                    _appConfig.GetFloat("TowerUsurperRadius"), _appConfig.GetFloat("TowerUsurperDamage"), //radius,damage
                    _appConfig.GetFloat("TowerUsurperWait"), _appConfig.GetFloat("TowerUsurperLife"), //wait,atklife
                    _appConfig.GetFloat("TowerUsurperAtkWait"), _appConfig.GetFloat("TowerUsurperAtkRadius"),//atkwait,atkrad
                    _appConfig.GetFloat("TowerUsurperAtkSpd"), _appConfig.GetFloat("TowerUsurperAtkLife")));//atkspd,lifetime

            infoUpdated = true;
        }

        /// <summary>
        /// タワー情報取得メソッド
        /// </summary>
        /// <param name="towerType"></param>
        /// <returns></returns>
        public static TowerAttr GetTowerInfo(TowerInfoID towerType)
        {
            return TowerInfos.TryGetValue(towerType, out TowerAttr value) ? value : null;
        }

        /// <summary>
        /// 解放メソッド
        /// </summary>
        static void Release()
        {
            towerInfos.Clear();
        }
    }
}