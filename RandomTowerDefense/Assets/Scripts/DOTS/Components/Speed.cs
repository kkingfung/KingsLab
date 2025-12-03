using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// [ECS] Speed - エンティティの移動速度を管理するコンポーネント
    ///
    /// 主な機能:
    /// - エンティティの移動速度管理
    /// - Burst互換の高性能データ構造
    /// - 速度ベースの計算サポート
    /// - メモリ効率的なBlittable型実装
    /// </summary>
    [Serializable]
    public struct Speed : IComponentData
    {
        #region IComponentData Implementation

        /// <summary>
        /// 移動速度の値（単位: Unity units/秒）
        /// </summary>
        public float Value;

        #endregion

        #region Public Properties

        /// <summary>
        /// 速度が有効かどうか（0以上）
        /// </summary>
        public bool IsValid => Value >= 0f;

        /// <summary>
        /// 指定した時間での移動距離を計算
        /// </summary>
        /// <param name="deltaTime">経過時間</param>
        /// <returns>移動距離</returns>
        public float GetDistance(float deltaTime) => Value * deltaTime;

        #endregion

        #region Constructors

        /// <summary>
        /// 指定した値でSpeedコンポーネントを初期化
        /// </summary>
        /// <param name="value">移動速度値</param>
        public Speed(float value)
        {
            Value = value;
        }

        #endregion

        #region Implicit Operators

        /// <summary>
        /// Speedコンポーネントからfloat値への暗黙的変換
        /// </summary>
        /// <param name="speed">変換するSpeedコンポーネント</param>
        /// <returns>速度値</returns>
        public static implicit operator float(Speed speed)
        {
            return speed.Value;
        }

        /// <summary>
        /// float値からSpeedコンポーネントへの暗黙的変換
        /// </summary>
        /// <param name="value">変換する値</param>
        /// <returns>指定した値を持つSpeedコンポーネント</returns>
        public static implicit operator Speed(float value)
        {
            return new Speed(value);
        }

        #endregion
    }
}

