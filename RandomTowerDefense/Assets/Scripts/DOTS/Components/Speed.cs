using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// エンティティの移動速度を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct Speed : IComponentData
    {
        #region Public Fields
        public float Value;
        #endregion
    }
}

