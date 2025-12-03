using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;

using System.IO;

namespace RandomTowerDefense.Info
{
    /// <summary>
    /// エネミーの属性情報を格納するクラス - 敵キャラクターの基本ステータス定義
    ///
    /// 主なプロパティ:
    /// - 体力（Health）：敵の耐久力
    /// - 移動速度（Speed）：移動スピード
    /// - ダメージ（Damage）：城への攻撃力
    /// - 半径（Radius）：当たり判定サイズ
    /// - 持続時間（Time）：特殊効果の持続時間
    /// - 報酬金額（Money）：撃破時の獲得資源
    /// </summary>
    public class EnemyAttr
    {
        #region Constants

        /// <summary>
        /// 体力計算用基準値
        /// </summary>
        private const float HealthBase = 1;

        #endregion

        #region Public Properties

        /// <summary>
        /// 体力
        /// </summary>
        public float Health;

        /// <summary>
        /// 移動速度
        /// </summary>
        public float Speed;

        /// <summary>
        /// 攻撃力
        /// </summary>
        public float Damage;

        /// <summary>
        /// 半径
        /// </summary>
        public float Radius;

        /// <summary>
        /// 持続時間
        /// </summary>
        public float Time;

        /// <summary>
        /// 報酬金額
        /// </summary>
        public int Money;

        #endregion

        #region Constructor

        /// <summary>
        /// エネミー属性コンストラクタ
        /// </summary>
        /// <param name="money">報酬金額</param>
        /// <param name="health">体力</param>
        /// <param name="speed">移動速度</param>
        /// <param name="time">持続時間</param>
        /// <param name="damage">攻撃力</param>
        /// <param name="radius">半径</param>
        public EnemyAttr(int money, float health, float speed, float time = 10,
            int damage = 1, float radius = 1)
        {
            this.Health = HealthBase * health;
            this.Speed = speed;
            this.Radius = radius;
            this.Damage = damage;
            this.Time = time;
            this.Money = money;
        }

        #endregion
    }

    /// <summary>
    /// 敵情報静的クラス - 25種類敵キャラクターのステータスデータ管理
    ///
    /// 主な機能:
    /// - 25種類敵タイプ（ボーナス敵、ロボット、ドラゴン、アンデッド、人型、生物系）のステータス管理
    /// - 敵属性（体力、移動速度、ダメージ、サイズ、持続時間、報酬金額）設定
    /// - JSON設定読み込みとリモート設定統合システム
    /// - ステージ別敵強化倍率とバランス調整機能
    /// - 敵名前配列とハッシュマップによる効率的データアクセス
    /// </summary>
    public static class EnemyInfo
    {
        #region Constants

        /// <summary>
        /// 全敵タイプ名配列 - ボーナス敵、ロボット系、ボス系、アンデッド系、人型系、生物系
        /// </summary>
        /// <remarks>報酬金額はボス・ボーナス敵以外はウェーブIDで調整される</remarks>
        public static readonly string[] AllName = {
            "MetalonGreen", "MetalonPurple", "MetalonRed", "AttackBot" , "RobotSphere",
            "Dragon", "Bull", "StoneMonster", "FreeLichS" , "FreeLich",
            "GolemS", "Golem", "SkeletonArmed", "SpiderGhost" , "Skeleton",
            "GruntS", "FootmanS", "Grunt", "Footman" , "TurtleShell",
            "Mushroom" , "Slime", "PigChef", "PhoenixChick" , "RockCritter"
        };

        #endregion

        #region Private Fields

        /// <summary>
        /// 敵情報辞書
        /// </summary>
        private static Dictionary<string, EnemyAttr> enemyInfo;

        #endregion

        #region Public Methods

        /// <summary>
        /// 敵情報初期化メソッド - デフォルト値でステータス設定
        /// </summary>
        public static void Init()
        {
            enemyInfo = new Dictionary<string, EnemyAttr>();

            //Bonus
            enemyInfo.Add("MetalonGreen", new EnemyAttr(200, 2000, 1.2f, 0.5f, 5, 0.5f));
            enemyInfo.Add("MetalonPurple", new EnemyAttr(350, 5000, 1.2f, 0.5f, 5, 0.5f));
            enemyInfo.Add("MetalonRed", new EnemyAttr(500, 10000, 1.2f, 0.5f, 5, 0.5f));

            //Stage 4
            enemyInfo.Add("AttackBot", new EnemyAttr(1000, 5000, 0.05f, 0.5f, 5, 0.5f));
            enemyInfo.Add("RobotSphere", new EnemyAttr(500, 2500, 0.2f, 0.5f, 5, 0.5f));

            //Bosses
            enemyInfo.Add("Dragon", new EnemyAttr(500, 3000, 0.15f, 0.5f, 5, 0.5f));
            enemyInfo.Add("Bull", new EnemyAttr(100, 2000, 0.1f, 0.5f, 5, 0.5f));
            enemyInfo.Add("StoneMonster", new EnemyAttr(20, 600, 0.4f, 0.5f, 5, 0.5f));

            //Stage 3
            enemyInfo.Add("FreeLichS", new EnemyAttr(18, 660, 0.75f, 0.5f, 1, 0.5f));
            enemyInfo.Add("FreeLich", new EnemyAttr(15, 480, 0.7f, 0.5f, 1, 0.5f));
            enemyInfo.Add("GolemS", new EnemyAttr(16, 1000, 0.4f, 0.5f, 1, 0.5f));
            enemyInfo.Add("Golem", new EnemyAttr(13, 600, 0.35f, 0.5f, 1, 0.5f));
            enemyInfo.Add("SkeletonArmed", new EnemyAttr(11, 320, 0.75f, 0.5f, 1, 0.5f));
            enemyInfo.Add("SpiderGhost", new EnemyAttr(7, 220, 1f, 0.5f, 1, 0.5f));

            //Stage 2
            enemyInfo.Add("Skeleton", new EnemyAttr(8, 280, 0.85f, 0.5f, 1, 0.5f));
            enemyInfo.Add("GruntS", new EnemyAttr(10, 380, 0.8f, 0.5f, 1, 0.5f));
            enemyInfo.Add("FootmanS", new EnemyAttr(8, 350, 0.8f, 0.5f, 1, 0.5f));
            enemyInfo.Add("Grunt", new EnemyAttr(8, 320, 0.75f, 0.5f, 1, 0.5f));
            enemyInfo.Add("Footman", new EnemyAttr(6, 280, 0.75f, 0.5f, 1, 0.5f));

            //Stage 1
            enemyInfo.Add("TurtleShell", new EnemyAttr(4, 300, 0.4f, 0.5f, 1, 0.5f));
            enemyInfo.Add("Mushroom", new EnemyAttr(3, 150, 0.7f, 1.0f, 1, 0.5f));
            enemyInfo.Add("Slime", new EnemyAttr(2, 120, 0.6f, 0.08f, 1, 0.5f));

            //Addition
            enemyInfo.Add("PigChef", new EnemyAttr(5, 250, 0.65f, 0.5f, 1, 0.5f));
            enemyInfo.Add("PhoenixChick", new EnemyAttr(4, 100, 1.1f, 1.0f, 1, 0.5f));
            enemyInfo.Add("RockCritter", new EnemyAttr(6, 320, 0.75f, 0.08f, 1, 0.5f));
        }

        /// <summary>
        /// ファイルから敵情報を初期化
        /// </summary>
        /// <param name="filepath">設定ファイルパス</param>
        public static void InitByFile(string filepath)
        {
            StreamReader inp_stm = new StreamReader(filepath);

            enemyInfo = new Dictionary<string, EnemyAttr>();

            while (!inp_stm.EndOfStream)
            {
                string inp_ln = inp_stm.ReadLine();
                string[] seperateInfo = inp_ln.Split(':');
                if (seperateInfo.Length == 7)
                {
                    enemyInfo.Add(seperateInfo[0], new EnemyAttr(
                        int.Parse(seperateInfo[1]), float.Parse(seperateInfo[2]),
                        float.Parse(seperateInfo[3]), float.Parse(seperateInfo[4]),
                        int.Parse(seperateInfo[5]), float.Parse(seperateInfo[6])));
                }
                // Debug.Log(seperateInfo.Length);
            }

            inp_stm.Close();
        }
        /// <summary>
        /// リモート設定から敵情報を初期化
        /// </summary>
        /// <param name="response">設定レスポンス</param>
        public static void InitByRemote(ConfigResponse response)
        {
            enemyInfo = new Dictionary<string, EnemyAttr>();

            foreach (string name in AllName)
            {
                enemyInfo.Add(name, new EnemyAttr(
                          ConfigManager.appConfig.GetInt("Enemy" + name + "Money"), ConfigManager.appConfig.GetFloat("Enemy" + name + "Health"),
                          ConfigManager.appConfig.GetFloat("Enemy" + name + "Speed"), ConfigManager.appConfig.GetFloat("Enemy" + name + "Time"),
                           ConfigManager.appConfig.GetInt("Enemy" + name + "Damage"), ConfigManager.appConfig.GetFloat("Enemy" + name + "Radius")));
            }
        }

        /// <summary>
        /// 敵情報取得メソッド
        /// </summary>
        /// <param name="enemyName">敵名</param>
        /// <returns>敵属性情報、見つからない場合はnull</returns>
        public static EnemyAttr GetEnemyInfo(string enemyName)
        {
            return enemyInfo.TryGetValue(enemyName, out EnemyAttr value) ? value : null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// リソース解放メソッド
        /// </summary>
        private static void Release()
        {
            enemyInfo.Clear();
        }

        #endregion
    }
}
