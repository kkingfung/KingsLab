using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Tags
{
    /// <summary>
    /// [ECS Tag] PlayerTag - プレイヤーエンティティを識別するためのタグ
    ///
    /// 主な機能:
    /// - プレイヤー所有エンティティのタイプ識別
    /// - プレイヤー側ユニットのクエリ検索
    /// - Burst互換のゼロコストタグ
    /// - AIとプレイヤーの判別処理
    /// </summary>
    [Serializable]
    public struct PlayerTag : IComponentData { }
}