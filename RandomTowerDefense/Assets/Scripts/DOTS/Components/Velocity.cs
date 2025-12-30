using System;
using Unity.Entities;
using Unity.Mathematics;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// [ECS] Velocity - エンティティの速度ベクトルを管理するコンポーネント
    ///
    /// 主な機能:
    /// - エンティティの3D速度ベクトル管理
    /// - 物理計算との統合
    /// - Burst互換の高性能データ構造
    /// - メモリ効率的なBlittable型実装
    /// </summary>
    [Serializable]
    public struct Velocity : IComponentData
    {
        #region IComponentData Implementation

        /// <summary>
        /// 3D速度ベクトル（Unity units/秒）
        /// </summary>
        public float3 Value;

        #endregion

        #region Public Properties

        /// <summary>
        /// 速度の大きさ（スカラー値）
        /// </summary>
        public float Magnitude => math.length(Value);

        /// <summary>
        /// 正規化された方向ベクトル
        /// </summary>
        public float3 Direction => math.normalizesafe(Value);

        /// <summary>
        /// 速度が0かどうか
        /// </summary>
        public bool IsZero => math.lengthsq(Value) < math.EPSILON;

        /// <summary>
        /// 水平速度（XZ平面）
        /// </summary>
        public float3 HorizontalVelocity => new float3(Value.x, 0f, Value.z);

        #endregion

        #region Constructors

        /// <summary>
        /// 指定したベクトルでVelocityコンポーネントを初期化
        /// </summary>
        /// <param name="value">速度ベクトル</param>
        public Velocity(float3 value)
        {
            Value = value;
        }

        /// <summary>
        /// 指定した成分でVelocityコンポーネントを初期化
        /// </summary>
        /// <param name="x">X成分</param>
        /// <param name="y">Y成分</param>
        /// <param name="z">Z成分</param>
        public Velocity(float x, float y, float z)
        {
            Value = new float3(x, y, z);
        }

        #endregion

        #region Implicit Operators

        /// <summary>
        /// Velocityコンポーネントからfloat3値への暗黙的変換
        /// </summary>
        /// <param name="velocity">変換するVelocityコンポーネント</param>
        /// <returns>速度ベクトル</returns>
        public static implicit operator float3(Velocity velocity)
        {
            return velocity.Value;
        }

        /// <summary>
        /// float3値からVelocityコンポーネントへの暗黙的変換
        /// </summary>
        /// <param name="value">変換するベクトル</param>
        /// <returns>指定したベクトルを持つVelocityコンポーネント</returns>
        public static implicit operator Velocity(float3 value)
        {
            return new Velocity(value);
        }

        #endregion
    }
}

