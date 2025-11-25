using Unity.Mathematics;

/// <summary>
/// DOTSシステム用の共有衝突検出ユーティリティ
/// 最適化された距離と衝突チェックメソッドを提供
/// </summary>
public static class CollisionUtilities
{
    /// <summary>
    /// 2つの3D位置間の距離の二乗を計算
    /// コストの高い平方根演算を避けるため距離の二乗を使用
    /// </summary>
    /// <param name="posA">最初の位置</param>
    /// <param name="posB">2番目の位置</param>
    /// <returns>位置間の距離の二乗</returns>
    public static float GetDistance(float3 posA, float3 posB)
    {
        float3 delta = posA - posB;
        return delta.x * delta.x + delta.z * delta.z;
    }

    /// <summary>
    /// 2つの位置が衝突範囲内にあるかチェック
    /// パフォーマンス最適化のため距離の二乗を比較
    /// </summary>
    /// <param name="posA">最初の位置</param>
    /// <param name="posB">2番目の位置</param>
    /// <param name="radiusSqr">衝突半径の二乗</param>
    /// <returns>位置が衝突範囲内にある場合true</returns>
    public static bool CheckCollision(float3 posA, float3 posB, float radiusSqr)
    {
        return GetDistance(posA, posB) <= radiusSqr;
    }
}