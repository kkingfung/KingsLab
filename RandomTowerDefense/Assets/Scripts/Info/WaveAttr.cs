using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomTowerDefense.Info
{
    /// <summary>
    /// ウェーブ属性クラス - 敵ウェーブのタイミングとスポーン設定
    ///
    /// 主な機能:
    /// - ウェーブ全体のタイミング制御（開始時間、スポーン間隔）
    /// - ウェーブ内敵詳細情報の管理とアクセス提供
    /// - ステージ構成における各ウェーブの設定データ保持
    /// - 敵スポーンシーケンスの時間管理とコントロール
    /// </summary>
    public class WaveAttr
    {
        #region Public Properties

        /// <summary>
        /// ウェーブ開始時間（秒）
        /// </summary>
        public float EnemyStartTime { get; private set; }

        /// <summary>
        /// 敵スポーン間隔時間（秒）
        /// </summary>
        public float EnemySpawnPeriod { get; private set; }

        /// <summary>
        /// ウェーブ内の敵詳細リスト
        /// </summary>
        public List<WaveDetail> WaveDetails { get; private set; }

        /// <summary>
        /// ウェーブ内の敵総数
        /// </summary>
        public int TotalEnemyCount => WaveDetails?.Sum(detail => detail.EnemyNumber) ?? 0;

        /// <summary>
        /// ウェーブの推定総持続時間（秒）
        /// </summary>
        public float EstimatedDuration => TotalEnemyCount > 0 ? EnemyStartTime + (TotalEnemyCount - 1) * EnemySpawnPeriod : EnemyStartTime;

        #endregion

        #region Public Methods

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waveDetails">ウェーブ内の敵詳細リスト</param>
        /// <param name="enemyStartTime">ウェーブ開始時間（デフォルト: 1.5秒）</param>
        /// <param name="enemySpawnPeriod">敵スポーン間隔時間（デフォルト: 0.5秒）</param>
        public WaveAttr(List<WaveDetail> waveDetails,
            float enemyStartTime = 1.5f, float enemySpawnPeriod = 0.5f)
        {
            EnemyStartTime = enemyStartTime;
            EnemySpawnPeriod = enemySpawnPeriod;
            WaveDetails = waveDetails ?? new List<WaveDetail>();
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="other">コピー元のWaveAttrオブジェクト</param>
        public WaveAttr(WaveAttr other)
        {
            if (other == null)
            {
                EnemyStartTime = 1.5f;
                EnemySpawnPeriod = 0.5f;
                WaveDetails = new List<WaveDetail>();
                return;
            }

            EnemyStartTime = other.EnemyStartTime;
            EnemySpawnPeriod = other.EnemySpawnPeriod;
            WaveDetails = other.WaveDetails?.Select(detail => new WaveDetail(detail)).ToList() ?? new List<WaveDetail>();
        }

        /// <summary>
        /// 敵詳細を追加
        /// </summary>
        /// <param name="waveDetail">追加する敵詳細</param>
        public void AddWaveDetail(WaveDetail waveDetail)
        {
            if (waveDetail != null)
            {
                WaveDetails.Add(waveDetail);
            }
        }

        /// <summary>
        /// 指定したポートIDの敵詳細を取得
        /// </summary>
        /// <param name="portID">スポーンポートID</param>
        /// <returns>該当する敵詳細のリスト</returns>
        public List<WaveDetail> GetWaveDetailsByPort(int portID)
        {
            return WaveDetails.Where(detail => detail.EnemyPort == portID).ToList();
        }

        /// <summary>
        /// 指定した敵タイプの詳細を取得
        /// </summary>
        /// <param name="enemyType">敵タイプ</param>
        /// <returns>該当する敵詳細のリスト</returns>
        public List<WaveDetail> GetWaveDetailsByType(string enemyType)
        {
            if (string.IsNullOrEmpty(enemyType))
                return new List<WaveDetail>();

            return WaveDetails.Where(detail => detail.EnemyType == enemyType).ToList();
        }

        /// <summary>
        /// オブジェクトの文字列表現を取得
        /// </summary>
        /// <returns>ウェーブ属性情報の文字列</returns>
        public override string ToString()
        {
            return $"WaveAttr[Start:{EnemyStartTime}s, Period:{EnemySpawnPeriod}s, Enemies:{TotalEnemyCount}, Duration:{EstimatedDuration:F1}s]";
        }

        #endregion
    }
}
