using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// エンティティのレベルを管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct Level : IComponentData
    {
        #region Public Fields
        public int Value;
        #endregion
    }
}

