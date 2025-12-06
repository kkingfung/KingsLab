using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// エンティティの生存時間を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct Lifetime : IComponentData
    {
        #region Public Fields
        public float Value;
        #endregion
    }
}

