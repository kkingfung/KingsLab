using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// エンティティの半径を表すECSコンポーネント
    /// </summary>
    [Serializable]
    public struct Radius : IComponentData
{
    public float Value;

    /// <summary>
    /// 指定した値でRadiusコンポーネントを初期化
    /// </summary>
    /// <param name="value">初期半径値</param>
    public Radius(float value)
    {
        Value = value;
    }

    /// <summary>
    /// Radiusコンポーネントからfloat値への暗黙的変換
    /// </summary>
    /// <param name="radius">変換するRadiusコンポーネント</param>
    /// <returns>半径値</returns>
    public static implicit operator float(Radius radius)
    {
        return radius.Value;
    }

    /// <summary>
    /// float値からRadiusコンポーネントへの暗黙的変換
    /// </summary>
    /// <param name="value">変換する値</param>
    /// <returns>指定した値を持つRadiusコンポーネント</returns>
    public static implicit operator Radius(float value)
    {
        return new Radius(value);
    }
    }
}

