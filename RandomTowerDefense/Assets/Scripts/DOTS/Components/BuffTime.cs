using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// バフの持続時間を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct BuffTime : IComponentData
    {
        #region Public Fields
        public float Value;
        #endregion
    }
}

