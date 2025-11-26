using System;
using Unity.Entities;
using Unity.Mathematics;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// 城の位置情報を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct CastlePos : IComponentData
    {
        #region Public Fields
        public float3 Value;
        #endregion
    }
}

