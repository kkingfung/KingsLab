using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// エンティティの減速率を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct SlowRate : IComponentData
    {
        #region Public Fields
        public float Value;
        #endregion
    }
}

