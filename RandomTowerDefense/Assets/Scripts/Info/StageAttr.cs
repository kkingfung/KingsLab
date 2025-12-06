using System.Linq;
using UnityEngine;

namespace RandomTowerDefense.Info
{
    /// <summary>
    /// ステージ属性クラス - ステージ全体のウェーブ設定と難易度設定
    ///
    /// 主な機能:
    /// - ステージ全体の構成とウェーブ管理
    /// - ウェーブ間のタイミング制御と待機時間設定
    /// - ステージ難易度調整とプレイ体験の設計
    /// - 全ウェーブの統合情報とステージ統計の提供
    /// </summary>
    public class StageAttr
    {
        #region Public Properties

        /// <summary>
        /// ウェーブ数
        /// </summary>
        public int WaveNum { get; private set; }

        /// <summary>
        /// ウェーブ間待機時間（秒）
        /// </summary>
        public float WaveWaitTime { get; private set; }

        /// <summary>
        /// 各ウェーブの属性配列
        /// </summary>
        public WaveAttr[] WaveAttrs { get; private set; }

        /// <summary>
        /// ステージ内の総敵数
        /// </summary>
        public int TotalEnemyCount => WaveAttrs?.Sum(wave => wave.TotalEnemyCount) ?? 0;

        /// <summary>
        /// ステージの推定総持続時間（秒）
        /// </summary>
        public float EstimatedDuration
        {
            get
            {
                if (WaveAttrs == null || WaveAttrs.Length == 0)
                    return 0f;

                float totalDuration = 0f;
                for (int i = 0; i < WaveAttrs.Length; i++)
                {
                    totalDuration += WaveAttrs[i].EstimatedDuration;
                    if (i < WaveAttrs.Length - 1) // 最後のウェーブ以外は待機時間を追加
                    {
                        totalDuration += WaveWaitTime;
                    }
                }
                return totalDuration;
            }
        }

        /// <summary>
        /// ステージが有効かどうか
        /// </summary>
        public bool IsValid => WaveAttrs != null && WaveAttrs.Length > 0 && WaveNum == WaveAttrs.Length;

        #endregion

        #region Public API

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waveNum">ウェーブ数</param>
        /// <param name="waveAttrs">各ウェーブの属性配列</param>
        /// <param name="waveWaitTime">ウェーブ間待機時間（デフォルト: 10秒）</param>
        public StageAttr(int waveNum, WaveAttr[] waveAttrs, float waveWaitTime = 10f)
        {
            WaveNum = waveNum;
            WaveAttrs = waveAttrs ?? new WaveAttr[0];
            WaveWaitTime = Mathf.Max(0f, waveWaitTime);

            // ウェーブ数と配列長の整合性チェック
            if (WaveAttrs.Length != waveNum)
            {
                Debug.LogWarning($"StageAttr: ウェーブ数({waveNum})と配列長({WaveAttrs.Length})が一致しません。");
            }
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="other">コピー元のStageAttrオブジェクト</param>
        public StageAttr(StageAttr other)
        {
            if (other == null)
            {
                WaveNum = 0;
                WaveWaitTime = 10f;
                WaveAttrs = new WaveAttr[0];
                return;
            }

            WaveNum = other.WaveNum;
            WaveWaitTime = other.WaveWaitTime;
            WaveAttrs = other.WaveAttrs?.Select(wave => new WaveAttr(wave)).ToArray() ?? new WaveAttr[0];
        }

        /// <summary>
        /// 指定したインデックスのウェーブ属性を取得
        /// </summary>
        /// <param name="waveIndex">ウェーブインデックス（0ベース）</param>
        /// <returns>ウェーブ属性、範囲外の場合はnull</returns>
        public WaveAttr GetWaveAttr(int waveIndex)
        {
            if (WaveAttrs == null || waveIndex < 0 || waveIndex >= WaveAttrs.Length)
                return null;

            return WaveAttrs[waveIndex];
        }

        /// <summary>
        /// ウェーブ属性の安全な更新
        /// </summary>
        /// <param name="waveIndex">ウェーブインデックス</param>
        /// <param name="newWaveAttr">新しいウェーブ属性</param>
        /// <returns>更新成功かどうか</returns>
        public bool UpdateWaveAttr(int waveIndex, WaveAttr newWaveAttr)
        {
            if (WaveAttrs == null || waveIndex < 0 || waveIndex >= WaveAttrs.Length || newWaveAttr == null)
                return false;

            WaveAttrs[waveIndex] = newWaveAttr;
            return true;
        }

        /// <summary>
        /// 指定した時刻に実行中のウェーブインデックスを取得
        /// </summary>
        /// <param name="elapsedTime">経過時間（秒）</param>
        /// <returns>ウェーブインデックス、該当なしの場合は-1</returns>
        public int GetActiveWaveIndex(float elapsedTime)
        {
            if (WaveAttrs == null || elapsedTime < 0)
                return -1;

            float currentTime = 0f;
            for (int i = 0; i < WaveAttrs.Length; i++)
            {
                float waveEndTime = currentTime + WaveAttrs[i].EstimatedDuration;
                if (elapsedTime >= currentTime && elapsedTime < waveEndTime)
                {
                    return i;
                }
                currentTime = waveEndTime + WaveWaitTime;
            }

            return -1; // すべてのウェーブが終了
        }

        /// <summary>
        /// オブジェクトの文字列表現を取得
        /// </summary>
        /// <returns>ステージ属性情報の文字列</returns>
        public override string ToString()
        {
            return $"StageAttr[Waves:{WaveNum}, Wait:{WaveWaitTime}s, Enemies:{TotalEnemyCount}, Duration:{EstimatedDuration:F1}s]";
        }

        #endregion
    }
}
