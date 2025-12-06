using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;
using System.IO;

namespace RandomTowerDefense.Info
{
    /// <summary>
    /// スキル情報静的クラス - 4種類魔法スキルのデータ定義と管理
    ///
    /// 主な機能:
    /// - 4種類魔法スキル（Meteor、Blizzard、Petrification、Minions）のステータス管理
    /// - スキル属性（範囲、ダメージ、クールタイム、持続時間、減速率、バフ時間）設定
    /// - JSONファイル読み込みとリモート設定統合システム
    /// - スキルバランス調整とゲームプレイ最適化
    /// - 辞書ベースの高速スキルデータアクセス
    /// </summary>
    public static class SkillInfo
    {
        #region Constants

        /// <summary>
        /// 全スキル名配列 - 4種類の魔法スキル
        /// </summary>
        private static readonly string[] AllSkillNames = {
            "SkillMeteor", "SkillBlizzard",
            "SkillPetrification", "SkillMinions"
        };

        /// <summary>
        /// スキル設定ファイルの期待するパラメータ数
        /// </summary>
        private const int EXPECTED_SKILL_PARAMETER_COUNT = 8;

        // Meteorスキル定数
        private const float METEOR_RADIUS = 2.5f;
        private const float METEOR_DAMAGE = 8.0f;
        private const float METEOR_RESPAWN_CYCLE = 2.5f;
        private const float METEOR_WAIT_TO_ACTION = 0.5f;
        private const float METEOR_LIFETIME = 15.0f;
        private const float METEOR_SLOW_RATE = 0.0f;
        private const float METEOR_DEBUFF_TIME = 0.0f;

        // Blizzardスキル定数
        private const float BLIZZARD_RADIUS = 5.0f;
        private const float BLIZZARD_DAMAGE = 0.0f;
        private const float BLIZZARD_RESPAWN_CYCLE = 5.0f;
        private const float BLIZZARD_WAIT_TO_ACTION = 0.0f;
        private const float BLIZZARD_LIFETIME = 15.0f;
        private const float BLIZZARD_SLOW_RATE = 0.01f;
        private const float BLIZZARD_DEBUFF_TIME = 3.0f;

        // Petrificationスキル定数
        private const float PETRIFICATION_RADIUS = 1000.0f;
        private const float PETRIFICATION_DAMAGE = 0.0f;
        private const float PETRIFICATION_RESPAWN_CYCLE = 0.2f;
        private const float PETRIFICATION_WAIT_TO_ACTION = 0.0f;
        private const float PETRIFICATION_LIFETIME = 5.0f;
        private const float PETRIFICATION_SLOW_RATE = 1.0f;
        private const float PETRIFICATION_DEBUFF_TIME = 0.0015f;

        // Minionsスキル定数
        private const float MINIONS_RADIUS = 1.0f;
        private const float MINIONS_DAMAGE = 3.0f;
        private const float MINIONS_RESPAWN_CYCLE = 0.1f;
        private const float MINIONS_WAIT_TO_ACTION = 1.0f;
        private const float MINIONS_LIFETIME = 10.0f;
        private const float MINIONS_SLOW_RATE = 0.0f;
        private const float MINIONS_DEBUFF_TIME = 0.0f;

        #endregion

        #region Private Fields

        /// <summary>
        /// スキル情報辞書
        /// </summary>
        private static Dictionary<string, SkillAttr> skillInfo;

        #endregion

        #region Public API

        /// <summary>
        /// 初期化メソッド - デフォルトのスキル属性を設定
        /// </summary>
        public static void Init()
        {
            skillInfo = new Dictionary<string, SkillAttr>();

            skillInfo.Add("SkillMeteor", new SkillAttr(
                METEOR_RADIUS, METEOR_DAMAGE, // 範囲、ダメージ
                METEOR_RESPAWN_CYCLE, METEOR_WAIT_TO_ACTION, // 再生サイクル、アクション待機時間
                METEOR_LIFETIME, METEOR_SLOW_RATE, // 生存時間、減速率（石化含む）
                METEOR_DEBUFF_TIME)); // デバフ時間

            skillInfo.Add("SkillBlizzard", new SkillAttr(
                BLIZZARD_RADIUS, BLIZZARD_DAMAGE, // 範囲、ダメージ
                BLIZZARD_RESPAWN_CYCLE, BLIZZARD_WAIT_TO_ACTION, // 再生サイクル、アクション待機時間
                BLIZZARD_LIFETIME, BLIZZARD_SLOW_RATE, // 生存時間、減速率（石化含む）
                BLIZZARD_DEBUFF_TIME)); // デバフ時間

            skillInfo.Add("SkillPetrification", new SkillAttr(
                PETRIFICATION_RADIUS, PETRIFICATION_DAMAGE, // 範囲、ダメージ
                PETRIFICATION_RESPAWN_CYCLE, PETRIFICATION_WAIT_TO_ACTION, // 再生サイクル、アクション待機時間
                PETRIFICATION_LIFETIME, PETRIFICATION_SLOW_RATE, // 生存時間、減速率（石化含む）
                PETRIFICATION_DEBUFF_TIME));// デバフ時間

            skillInfo.Add("SkillMinions", new SkillAttr(
                MINIONS_RADIUS, MINIONS_DAMAGE, // 範囲、ダメージ
                MINIONS_RESPAWN_CYCLE, MINIONS_WAIT_TO_ACTION, // 再生サイクル、アクション待機時間
                MINIONS_LIFETIME, MINIONS_SLOW_RATE, // 生存時間、減速率（石化含む）
                METEOR_DEBUFF_TIME)); // デバフ時間
        }

        /// <summary>
        /// ファイルからの初期化メソッド - 指定されたファイルパスからスキル属性を読み込み
        /// </summary>
        /// <param name="filepath">スキル属性データファイルのパス</param>
        public static void InitByFile(string filepath)
        {
            StreamReader inp_stm = new StreamReader(filepath);

            skillInfo = new Dictionary<string, SkillAttr>();

            while (!inp_stm.EndOfStream)
            {
                string inp_ln = inp_stm.ReadLine();
                string[] seperateInfo = inp_ln.Split(':');
                if (seperateInfo.Length == EXPECTED_SKILL_PARAMETER_COUNT)
                {
                    skillInfo.Add(seperateInfo[0], new SkillAttr(
                        float.Parse(seperateInfo[1]), float.Parse(seperateInfo[2]),
                        float.Parse(seperateInfo[3]), float.Parse(seperateInfo[4]),
                        float.Parse(seperateInfo[5]), float.Parse(seperateInfo[6]),
                        float.Parse(seperateInfo[7])));
                }
            }

            inp_stm.Close();
        }

        /// <summary>
        /// リモート設定からの初期化メソッド - リモート設定サービスからスキル属性を取得
        /// </summary>
        /// <param name="response">リモート設定のレスポンスデータ</param>
        public static void InitByRemote(ConfigResponse response)
        {
            skillInfo = new Dictionary<string, SkillAttr>();

            foreach (string name in AllSkillNames)
            {
                var attr = new SkillAttr(
                    ConfigManager.appConfig.GetFloat(name + "Radius"),
                    ConfigManager.appConfig.GetFloat(name + "Damage"),
                    ConfigManager.appConfig.GetFloat(name + "SpawnCycle"),
                    ConfigManager.appConfig.GetFloat(name + "Wait"),
                    ConfigManager.appConfig.GetFloat(name + "Life"),
                    ConfigManager.appConfig.GetFloat(name + "SlowRate"),
                    ConfigManager.appConfig.GetFloat(name + "BuffTime")
                );
                skillInfo.Add(name, attr);
            }
        }

        /// <summary>
        /// スキル情報取得メソッド - 指定されたスキル名に対応するスキル属性を返す
        /// </summary>
        /// <param name="skillName">取得するスキルの名前</param>
        /// <returns>対応するスキル属性オブジェクト、見つからない場合はnull</returns>
        public static SkillAttr GetSkillInfo(string skillName)
        {
            return skillInfo.TryGetValue(skillName, out var attr) ? attr : null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// リソース解放メソッド - スキル情報のクリア
        /// </summary>
        private static void Release()
        {
            skillInfo.Clear();
        }

        #endregion
    }
}