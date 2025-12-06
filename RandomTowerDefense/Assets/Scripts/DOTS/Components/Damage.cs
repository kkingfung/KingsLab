using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// [ECS] Damage - エンティティのダメージ値を表すECSコンポーネント
    ///
    /// 主な機能:
    /// - エンティティのダメージ値管理
    /// - Burst互換の高性能データ構造
    /// - 暗黙的型変換によるfloatとの相互変換
    /// - メモリ効率的なBlittable型実装
    /// </summary>
    [Serializable]
    public struct Damage : IComponentData
    {
        #region IComponentData Implementation

        /// <summary>
        /// ダメージ値
        /// </summary>
        public float Value;

        #endregion

        #region Public Properties

        /// <summary>
        /// ダメージ値が有効かどうか（0より大きい）
        /// </summary>
        public bool IsValid => Value > 0f;

        /// <summary>
        /// クリティカルダメージを計算（指定した倍率を適用）
        /// </summary>
        /// <param name="criticalMultiplier">クリティカル倍率</param>
        /// <returns>クリティカルダメージ値</returns>
        public float GetCriticalDamage(float criticalMultiplier) => Value * criticalMultiplier;

        #endregion

        #region Constructors

        /// <summary>
        /// 指定した値でDamageコンポーネントを初期化
        /// </summary>
        /// <param name="value">初期ダメージ値</param>
        public Damage(float value)
        {
            Value = value;
        }

        #endregion

        #region Implicit Operators

        /// <summary>
        /// Damageコンポーネントからfloat値への暗黙的変換
        /// </summary>
        /// <param name="damage">変換するDamageコンポーネント</param>
        /// <returns>ダメージ値</returns>
        public static implicit operator float(Damage damage)
        {
            return damage.Value;
        }

        /// <summary>
        /// float値からDamageコンポーネントへの暗黙的変換
        /// </summary>
        /// <param name="value">変換する値</param>
        /// <returns>指定した値を持つDamageコンポーネント</returns>
        public static implicit operator Damage(float value)
        {
            return new Damage(value);
        }

        #endregion
    }
}

