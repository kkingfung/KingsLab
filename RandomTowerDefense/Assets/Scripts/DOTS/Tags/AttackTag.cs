using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Tags
{
    /// <summary>
    /// [ECS Tag] AttackTag - アタックエンティティを識別するためのタグ
    ///
    /// 主な機能:
    /// - アタックエンティティのタイプ識別
    /// - 弾丸や攻撃オブジェクトのクエリ検索
    /// - Burst互換のゼロコストタグ
    /// - 衝突処理でのタイプ判定
    /// </summary>
    [Serializable]
    public struct AttackTag : IComponentData { }
}