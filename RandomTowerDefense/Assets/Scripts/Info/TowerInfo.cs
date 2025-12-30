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
        #region Constants

        // Nightmare タワー用定数
        private const float NIGHTMARE_RADIUS = 3.5f;
        private const float NIGHTMARE_DAMAGE = 25f;
        private const float NIGHTMARE_WAIT = 3f;
        private const float NIGHTMARE_LIFETIME = 5f;
        private const float NIGHTMARE_ATK_WAIT = 0.01f;
        private const float NIGHTMARE_ATK_RADIUS = 0.3f;
        private const float NIGHTMARE_ATK_SPEED = 0f;
        private const float NIGHTMARE_ATK_LIFETIME = 3f;

        // SoulEater タワー用定数
        private const float SOULEATER_RADIUS = 2.5f;
        private const float SOULEATER_DAMAGE = 7f;
        private const float SOULEATER_WAIT = 0.3f;
        private const float SOULEATER_LIFETIME = 5f;
        private const float SOULEATER_ATK_WAIT = 0f;
        private const float SOULEATER_ATK_RADIUS = 0.8f;
        private const float SOULEATER_ATK_SPEED = 5f;
        private const float SOULEATER_ATK_LIFETIME = 3f;

        // TerrorBringer タワー用定数
        private const float TERRORBRINGER_RADIUS = 4f;
        private const float TERRORBRINGER_DAMAGE = 50f;
        private const float TERRORBRINGER_WAIT = 5f;
        private const float TERRORBRINGER_LIFETIME = 5f;
        private const float TERRORBRINGER_ATK_WAIT = 0.01f;
        private const float TERRORBRINGER_ATK_RADIUS = 0.2f;
        private const float TERRORBRINGER_ATK_SPEED = 0f;
        private const float TERRORBRINGER_ATK_LIFETIME = 3f;

        // Usurper タワー用定数
        private const float USURPER_RADIUS = 2f;
        private const float USURPER_DAMAGE = 5f;
        private const float USURPER_WAIT = 0.2f;
        private const float USURPER_LIFETIME = 5f;
        private const float USURPER_ATK_WAIT = 0f;
        private const float USURPER_ATK_RADIUS = 0.5f;
        private const float USURPER_ATK_SPEED = 3.5f;
        private const float USURPER_ATK_LIFETIME = 1f;

        // ファイル解析用定数
        private const int EXPECTED_PARAMETER_COUNT = 9;

        #endregion

        #region Enums

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

        #endregion

        #region Public Properties

        /// <summary>
        /// タワー属性辞書への読み取り専用アクセス
        /// </summary>
        public static Dictionary<TowerInfoID, TowerAttr> TowerInfos => towerInfos;

        /// <summary>
        /// 情報更新フラグ
        /// </summary>
        public static bool InfoUpdated = false;

        #endregion

        #region Private Fields

        /// <summary>
        /// タワー属性辞書
        /// </summary>
        private static Dictionary<TowerInfoID, TowerAttr> towerInfos;

        #endregion

        #region Public API

        /// <summary>
        /// 初期化メソッド
        /// </summary>
        public static void Init()
        {
            towerInfos = new Dictionary<TowerInfoID, TowerAttr>();

            towerInfos.Add(TowerInfoID.EnumTowerNightmare,
                new TowerAttr(
                    NIGHTMARE_RADIUS, NIGHTMARE_DAMAGE, // 攻撃半径, ダメージ
                    NIGHTMARE_WAIT, NIGHTMARE_LIFETIME, // 待機時間, 持続時間
                    NIGHTMARE_ATK_WAIT, NIGHTMARE_ATK_RADIUS, // 攻撃待機時間, 攻撃範囲
                    NIGHTMARE_ATK_SPEED, NIGHTMARE_ATK_LIFETIME)); // 攻撃速度, 攻撃持続時間
            towerInfos.Add(TowerInfoID.EnumTowerSoulEater,
                new TowerAttr(
                    SOULEATER_RADIUS, SOULEATER_DAMAGE, // 攻撃半径, ダメージ
                    SOULEATER_WAIT, SOULEATER_LIFETIME, // 待機時間, 持続時間
                    SOULEATER_ATK_WAIT, SOULEATER_ATK_RADIUS, // 攻撃待機時間, 攻撃範囲
                    SOULEATER_ATK_SPEED, SOULEATER_ATK_LIFETIME)); // 攻撃速度, 攻撃持続時間
            towerInfos.Add(TowerInfoID.EnumTowerTerrorBringer,
                new TowerAttr(
                    TERRORBRINGER_RADIUS, TERRORBRINGER_DAMAGE, // 攻撃半径, ダメージ
                    TERRORBRINGER_WAIT, TERRORBRINGER_LIFETIME, // 待機時間, 持続時間
                    TERRORBRINGER_ATK_WAIT, TERRORBRINGER_ATK_RADIUS, // 攻撃待機時間, 攻撃範囲
                    TERRORBRINGER_ATK_SPEED, TERRORBRINGER_ATK_LIFETIME)); // 攻撃速度, 攻撃持続時間
            towerInfos.Add(TowerInfoID.EnumTowerUsurper,
                new TowerAttr(
                    USURPER_RADIUS, USURPER_DAMAGE, // 攻撃半径, ダメージ
                    USURPER_WAIT, USURPER_LIFETIME, // 待機時間, 持続時間
                    USURPER_ATK_WAIT, USURPER_ATK_RADIUS, // 攻撃待機時間, 攻撃範囲
                    USURPER_ATK_SPEED, USURPER_ATK_LIFETIME)); // 攻撃速度, 攻撃持続時間
            InfoUpdated = true;
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
                if (seperateInfo.Length == EXPECTED_PARAMETER_COUNT)
                {
                    towerInfos.Add((TowerInfoID)int.Parse(seperateInfo[0]), new TowerAttr(
                        float.Parse(seperateInfo[1]), float.Parse(seperateInfo[2]),
                        float.Parse(seperateInfo[3]), float.Parse(seperateInfo[4]),
                        float.Parse(seperateInfo[5]), float.Parse(seperateInfo[6]),
                        float.Parse(seperateInfo[7]), float.Parse(seperateInfo[8])));
                }
            }

            inp_stm.Close();

            InfoUpdated = true;
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

            InfoUpdated = true;
        }

        /// <summary>
        /// タワー情報取得メソッド
        /// </summary>
        /// <param name="towerType">取得するタワーのタイプ</param>
        /// <returns>対応するタワー属性データ、見つからない場合はnull</returns>
        public static TowerAttr GetTowerInfo(TowerInfoID towerType)
        {
            return TowerInfos.TryGetValue(towerType, out TowerAttr value) ? value : null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// リソース解放メソッド
        /// </summary>
        private static void Release()
        {
            towerInfos.Clear();
        }

        #endregion
    }
}