using System;
using Unity.Entities;

/// <summary>
/// 単一のfloat値を格納するECSコンポーネントのベース構造体
/// 簡単な使用のため暗黙的変換演算子を提供
/// </summary>
[Serializable]
public struct BaseValueComponent : IComponentData
{
    public float Value;

    /// <summary>
    /// 値でコンポーネントを初期化
    /// </summary>
    /// <param name="value">初期のfloat値</param>
    public BaseValueComponent(float value)
    {
        Value = value;
    }

    /// <summary>
    /// コンポーネントからfloat値への暗黙的変換
    /// </summary>
    /// <param name="component">変換するコンポーネント</param>
    /// <returns>コンポーネントの値</returns>
    public static implicit operator float(BaseValueComponent component)
    {
        return component.Value;
    }

    /// <summary>
    /// floatからコンポーネントへの暗黙的変換
    /// </summary>
    /// <param name="value">変換する値</param>
    /// <returns>値を含むコンポーネント</returns>
    public static implicit operator BaseValueComponent(float value)
    {
        return new BaseValueComponent(value);
    }
}