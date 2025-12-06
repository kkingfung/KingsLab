using UnityEngine;

namespace RandomTowerDefense.Info
{
    /// <summary>
    /// 敵詳細情報クラス - ウェーブ内の敵スポーン設定データ
    ///
    /// 主な機能:
    /// - ウェーブ内の個別敵ユニットのスポーン設定管理
    /// - スポーンポート、敵タイプ、数量の設定データ保持
    /// - ステージ構成における敵配置とタイミング制御のサポート
    /// - ウェーブアトリビュートシステムとの連携データ構造
    /// </summary>
    public class WaveDetail
    {
        #region Public Properties

        /// <summary>
        /// ウェーブID
        /// </summary>
        public int WaveID { get; private set; }

        /// <summary>
        /// 敵の数
        /// </summary>
        public int EnemyNumber { get; private set; }

        /// <summary>
        /// スポーンポートID
        /// </summary>
        public int EnemyPort { get; private set; }

        /// <summary>
        /// 敵タイプ
        /// </summary>
        public string EnemyType { get; private set; }

        #endregion

        #region Public API

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waveID">ウェーブID</param>
        /// <param name="enemyNumber">敵の数</param>
        /// <param name="enemyPort">スポーンポートID</param>
        /// <param name="enemyType">敵タイプ</param>
        public WaveDetail(int waveID, int enemyNumber, int enemyPort, string enemyType)
        {
            WaveID = waveID;
            EnemyNumber = enemyNumber;
            EnemyPort = enemyPort;
            EnemyType = enemyType ?? string.Empty;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="other">コピー元のWaveDetailオブジェクト</param>
        public WaveDetail(WaveDetail other)
        {
            if (other == null)
            {
                WaveID = 0;
                EnemyNumber = 0;
                EnemyPort = 0;
                EnemyType = string.Empty;
                return;
            }

            WaveID = other.WaveID;
            EnemyNumber = other.EnemyNumber;
            EnemyPort = other.EnemyPort;
            EnemyType = other.EnemyType;
        }

        /// <summary>
        /// オブジェクトの文字列表現を取得
        /// </summary>
        /// <returns>ウェーブ詳細情報の文字列</returns>
        public override string ToString()
        {
            return $"WaveDetail[ID:{WaveID}, Type:{EnemyType}, Count:{EnemyNumber}, Port:{EnemyPort}]";
        }

        #endregion
    }
}
