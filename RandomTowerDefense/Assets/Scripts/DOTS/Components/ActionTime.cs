using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// [ECS] ActionTime - 最初の衝突チェックまでの待機時間を管理するコンポーネント
    ///
    /// 主な機能:
    /// - アクション実行までの遅延時間管理
    /// - タイミング制御による戦術的要素
    /// - Burst互換の高性能データ構造
    /// - メモリ効率的なBlittable型実装
    /// </summary>
    [Serializable]
    public struct ActionTime : IComponentData
    {
        #region IComponentData Implementation

        /// <summary>
        /// アクション実行までの残り時間（秒）
        /// </summary>
        public float Value;

        #endregion

        #region Public Properties

        /// <summary>
        /// アクション実行可能かどうか（時間が0以下）
        /// </summary>
        public bool IsReady => Value <= 0f;

        /// <summary>
        /// アクションまでの待機が必要かどうか
        /// </summary>
        public bool IsWaiting => Value > 0f;

        #endregion

        #region Constructors

        /// <summary>
        /// 指定した時間でActionTimeコンポーネントを初期化
        /// </summary>
        /// <param name="value">待機時間（秒）</param>
        public ActionTime(float value)
        {
            Value = value;
        }

        #endregion

        #region Implicit Operators

        /// <summary>
        /// ActionTimeコンポーネントからfloat値への暗黙的変換
        /// </summary>
        /// <param name="actionTime">変換するActionTimeコンポーネント</param>
        /// <returns>時間値</returns>
        public static implicit operator float(ActionTime actionTime)
        {
            return actionTime.Value;
        }

        /// <summary>
        /// float値からActionTimeコンポーネントへの暗黙的変換
        /// </summary>
        /// <param name="value">変換する値</param>
        /// <returns>指定した値を持つActionTimeコンポーネント</returns>
        public static implicit operator ActionTime(float value)
        {
            return new ActionTime(value);
        }

        #endregion
    }
}

