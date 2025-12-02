using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Tags
{
    /// <summary>
    /// [ECS Tag] EnemyTag - エネミーエンティティを識別するためのタグ
    ///
    /// 主な機能:
    /// - エネミーエンティティのタイプ識別
    /// - クエリによる高速なエネミー検索
    /// - Burst互換のゼロコストタグ
    /// - システム間でのエネミー判定
    /// </summary>
    [Serializable]
    public struct EnemyTag : IComponentData { }
}