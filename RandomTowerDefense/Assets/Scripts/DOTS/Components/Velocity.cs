using System;
using Unity.Entities;
using Unity.Mathematics;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// エンティティの速度ベクトルを管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct Velocity : IComponentData
    {
        #region Public Fields
        public float3 Value;
        #endregion
    }
}

