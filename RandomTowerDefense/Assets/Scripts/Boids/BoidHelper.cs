using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTowerDefense.Boids
{
    /// <summary>
    /// ボイドヘルパーユーティリティ - フロッキング計算用の数学的サポート機能
    ///
    /// 主な機能:
    /// - 黄金比ベースの球面均等分布方向ベクトル生成（300方向）
    /// - Fibonacci螺旋アルゴリズムによる最適化された3D方向配布
    /// - 障害物回避用のレイキャスティング方向プリセット提供
    /// - 静的初期化による高性能アクセス保証
    /// - 数学的に均等な視野方向分散システム
    /// - ボイドシステム全体での共有リソース管理
    /// </summary>
    public static class BoidHelper
    {
        #region Constants

        private const int NUM_VIEW_DIRECTIONS = 300;

        #endregion

        #region Public Static Fields

        /// <summary>
        /// 事前計算された均等分布3D方向ベクトル配列
        /// </summary>
        public static readonly Vector3[] directions;

        #endregion

        #region Static Constructor

        /// <summary>
        /// 静的初期化 - 黄金比を使用した球面上の均等分布方向ベクトル生成
        /// </summary>
        static BoidHelper()
        {
            directions = new Vector3[NUM_VIEW_DIRECTIONS];

            // 黄金比による最適な角度分散
            float goldenRatio = (1 + Mathf.Sqrt(5)) / 2f;
            float angleIncrement = Mathf.PI * 2f * goldenRatio;

            // Fibonacci螺旋アルゴリズムで球面均等分布
            for (int i = 0; i < NUM_VIEW_DIRECTIONS; ++i)
            {
                float t = (float)i / NUM_VIEW_DIRECTIONS;
                float inclination = Mathf.Acos(1 - 2 * t);
                float azimuth = angleIncrement * i;

                float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                float z = Mathf.Cos(inclination);

                directions[i] = new Vector3(x, y, z);
            }
        }

        #endregion
    }
}