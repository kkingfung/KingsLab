using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// エンティティのタイプを管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct Type : IComponentData
    {
        #region Public Fields
        public int Value;
        #endregion
    }
}

