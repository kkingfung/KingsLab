using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// [ECS] Health - エンティティのヘルス値を表すECSコンポーネント
    ///
    /// 主な機能:
    /// - エンティティのヘルス値管理
    /// - Burst互換の高性能データ構造
    /// - 暗黙的型変換によるfloatとの相互変換
    /// - メモリ効率的なBlittable型実装
    /// </summary>
    [Serializable]
    public struct Health : IComponentData
    {
        #region IComponentData Implementation

        /// <summary>
        /// ヘルス値
        /// </summary>
        public float Value;

        #endregion

        #region Public Properties

        /// <summary>
        /// ヘルス値が0以下かどうか（死亡状態）
        /// </summary>
        public bool IsDead => Value <= 0f;

        /// <summary>
        /// ヘルス値の正規化された値（0.0-1.0の範囲、最大値必要）
        /// </summary>
        /// <param name="maxHealth">最大ヘルス値</param>
        /// <returns>正規化されたヘルス値</returns>
        public float GetNormalized(float maxHealth) => maxHealth > 0f ? Value / maxHealth : 0f;

        #endregion

        #region Constructors

        /// <summary>
        /// 指定した値でHealthコンポーネントを初期化
        /// </summary>
        /// <param name="value">初期ヘルス値</param>
        public Health(float value)
        {
            Value = value;
        }

        #endregion

        #region Implicit Operators

        /// <summary>
        /// Healthコンポーネントからfloat値への暗黙的変換
        /// </summary>
        /// <param name="health">変換するHealthコンポーネント</param>
        /// <returns>ヘルス値</returns>
        public static implicit operator float(Health health)
        {
            return health.Value;
        }

        /// <summary>
        /// float値からHealthコンポーネントへの暗黙的変換
        /// </summary>
        /// <param name="value">変換する値</param>
        /// <returns>指定した値を持つHealthコンポーネント</returns>
        public static implicit operator Health(float value)
        {
            return new Health(value);
        }

        #endregion
    }
}

