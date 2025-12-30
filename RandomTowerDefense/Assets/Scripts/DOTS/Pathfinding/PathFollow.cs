using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Pathfinding
{
    /// <summary>
    /// [ECS] PathFollow - エンティティのパス追従状態を管理するコンポーネント
    ///
    /// 主な機能:
    /// - 現在のパス進行状況追跡
    /// - パスフィンディング完了後の経路追従制御
    /// - Burst互換の高性能データ構造
    /// - パスバッファとの統合管理
    /// </summary>
    [Serializable]
    [GenerateAuthoringComponent]
    public struct PathFollow : IComponentData
    {
        #region IComponentData Implementation

        /// <summary>
        /// 現在追従中のパスインデックス
        /// -1の場合はパス追従なし
        /// </summary>
        public int pathIndex;

        #endregion

        #region Public Properties

        /// <summary>
        /// 有効なパスを追従中かどうか
        /// </summary>
        public bool IsFollowingPath => pathIndex >= 0;

        /// <summary>
        /// パス追従が完了しているかどうか
        /// </summary>
        public bool IsPathComplete => pathIndex < 0;

        #endregion

        #region Constructors

        /// <summary>
        /// 指定したパスインデックスでPathFollowを初期化
        /// </summary>
        /// <param name="index">パスインデックス</param>
        public PathFollow(int index)
        {
            pathIndex = index;
        }

        /// <summary>
        /// パス追従なし状態でPathFollowを初期化
        /// </summary>
        /// <returns>パス追従なしのPathFollow</returns>
        public static PathFollow NoPath => new PathFollow(-1);

        #endregion
    }
}
