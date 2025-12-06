using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Tags
{
    /// <summary>
    /// [ECS Tag] CastleTag - 城エンティティを識別するためのタグ
    ///
    /// 主な機能:
    /// - 城エンティティのタイプ識別
    /// - 防衛拠点としてのクエリ検索
    /// - Burst互換のゼロコストタグ
    /// - ゲームオーバー判定でのターゲット識別
    /// </summary>
    [Serializable]
    public struct CastleTag : IComponentData { }
}