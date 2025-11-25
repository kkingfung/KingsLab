using System;
using Unity.Entities;

/// <summary>
/// エンティティのダメージ値を表すECSコンポーネント
/// </summary>
[Serializable]
public struct Damage : IComponentData
{
    public float Value;

    /// <summary>
    /// 指定した値でDamageコンポーネントを初期化
    /// </summary>
    /// <param name="value">初期ダメージ値</param>
    public Damage(float value)
    {
        Value = value;
    }

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
}

