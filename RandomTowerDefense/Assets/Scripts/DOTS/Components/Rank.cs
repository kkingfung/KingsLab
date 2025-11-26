using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// エンティティのランク情報を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct Rank : IComponentData
    {
        #region Public Fields
        public int Value;
        #endregion
    }
}

