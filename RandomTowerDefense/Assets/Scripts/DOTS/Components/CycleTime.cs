using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// サイクルタイムを管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct CycleTime : IComponentData
    {
        #region Public Fields
        public float Value;
        #endregion
    }
}

