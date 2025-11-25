using System;
using Unity.Entities;

/// <summary>
/// エンティティのヘルス値を表すECSコンポーネント
/// </summary>
[Serializable]
public struct Health : IComponentData
{
	public float Value;

	/// <summary>
	/// 指定した値でHealthコンポーネントを初期化
	/// </summary>
	/// <param name="value">初期ヘルス値</param>
	public Health(float value)
	{
		Value = value;
	}

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
}

