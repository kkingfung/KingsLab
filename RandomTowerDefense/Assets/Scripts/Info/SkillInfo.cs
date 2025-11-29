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
        /// <summary>
        /// スキル属性クラス - 各スキルの詳細なパラメータを格納
        /// </summary>
        static Dictionary<string, SkillAttr> skillInfo;

        /// <summary>
        /// 初期化メソッド - デフォルトのスキル属性を設定
        /// </summary>
        public static void Init()
        {
            skillInfo = new Dictionary<string, SkillAttr>();

            skillInfo.Add("SkillMeteor", new SkillAttr(
                2.5f, 8f, // radius,damage
                2.5f, 0.5f, // respawn cycle,wait to action
                15, 0, // lifetime,slowrate(include petrify)
                0)); // debufftime

            skillInfo.Add("SkillBlizzard", new SkillAttr(
                5, 0f, // radius,damage
                5, 0f, // respawn cycle,wait to action
                15, 0.01f, // lifetime,slowrate(include petrify)
                3.0f)); // debufftime

            skillInfo.Add("SkillPetrification", new SkillAttr(
                1000, 0, // radius,damage
                0.2f, 0f, // respawn cycle,wait to action
                5, 1.0f, // lifetime,slowrate(include petrify)
                0.0015f));// debufftime

            skillInfo.Add("SkillMinions", new SkillAttr(
                1, 3f, // radius,damage
                0.1f, 1f, // respawn cycle,wait to action
                10, 0, // lifetime,slowrate(include petrify)
                0)); // debufftime
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
                if (seperateInfo.Length == 8)
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

            string[] allNames =
            {
                "SkillMeteor", "SkillBlizzard",
                "SkillPetrification", "SkillMinions"
            };

            foreach (string name in allNames)
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
        /// リソース解放メソッド - スキル情報のクリア
        /// </summary>
        public static void Release()
        {
            skillInfo.Clear();
        }

        /// <summary>
        /// スキル情報取得メソッド - 指定されたスキル名に対応するスキル属性を返す
        /// </summary>
        /// <param name="skillName">取得するスキルの名前</param>
        /// <returns>対応するスキル属性オブジェクト</returns>
        public static SkillAttr GetSkillInfo(string skillName)
        {
            return skillInfo.TyGetValue(skillName, out var attr) ? attr : null;
        }
    }
}