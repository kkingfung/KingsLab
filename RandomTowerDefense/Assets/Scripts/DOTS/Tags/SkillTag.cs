using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Tags
{
    /// <summary>
    /// [ECS Tag] SkillTag - スキルエンティティを識別するためのタグ
    ///
    /// 主な機能:
    /// - スキルエンティティのタイプ識別
    /// - 特殊攻撃や強化エフェクトのクエリ
    /// - Burst互換のゼロコストタグ
    /// - スキルシステムでの管理対象識別
    /// </summary>
    [Serializable]
    public struct SkillTag : IComponentData { }
}