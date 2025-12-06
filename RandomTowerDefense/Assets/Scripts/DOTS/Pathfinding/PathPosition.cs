using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// [ECS] PathPosition - パス上の位置を表すバッファー要素
/// 経路上の各ウェイポイント座標を格納
/// </summary>
[InternalBufferCapacity(40)]
public struct PathPosition : IBufferElementData {

    /// <summary>
    /// グリッド座標上の位置
    /// </summary>
    public int2 position;

}
