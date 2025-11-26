using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Tags
{
    /// <summary>
    /// アタックエンティティを識別するためのタグ
    /// </summary>
    [Serializable]
    public struct AttackTag : IComponentData { }
}