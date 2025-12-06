using System;
using Unity.Entities;
using Unity.Mathematics;

namespace RandomTowerDefense.DOTS.Pathfinding
{
    /// <summary>
    /// [ECS] PathfindingParams - A*パスフィンディングパラメータを管理するコンポーネント
    ///
    /// 主な機能:
    /// - 開始・終了位置の座標管理
    /// - エンティティ毎の経路計算リクエスト
    /// - Burst互換の高性能データ構造
    /// - グリッドベース座標システムとの統合
    /// </summary>
    [Serializable]
    public struct PathfindingParams : IComponentData
    {
        #region IComponentData Implementation

        /// <summary>
        /// パスフィンディング開始位置（グリッド座標）
        /// </summary>
        public int2 startPosition;

        /// <summary>
        /// パスフィンディング終了位置（グリッド座標）
        /// </summary>
        public int2 endPosition;

        #endregion

        #region Public Properties

        /// <summary>
        /// パラメータが有効かどうか（開始・終了位置が異なる）
        /// </summary>
        public bool IsValid => !startPosition.Equals(endPosition);

        /// <summary>
        /// 開始位置から終了位置までの直線距離
        /// </summary>
        public float StraightLineDistance => math.distance(startPosition, endPosition);

        /// <summary>
        /// マンハッタン距離（グリッド移動距離）
        /// </summary>
        public int ManhattanDistance => math.abs(endPosition.x - startPosition.x) + math.abs(endPosition.y - startPosition.y);

        #endregion

        #region Constructors

        /// <summary>
        /// 開始・終了位置を指定してPathfindingParamsを初期化
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="end">終了位置</param>
        public PathfindingParams(int2 start, int2 end)
        {
            startPosition = start;
            endPosition = end;
        }

        /// <summary>
        /// 個別座標を指定してPathfindingParamsを初期化
        /// </summary>
        /// <param name="startX">開始X座標</param>
        /// <param name="startY">開始Y座標</param>
        /// <param name="endX">終了X座標</param>
        /// <param name="endY">終了Y座標</param>
        public PathfindingParams(int startX, int startY, int endX, int endY)
        {
            startPosition = new int2(startX, startY);
            endPosition = new int2(endX, endY);
        }

        #endregion
    }
}
