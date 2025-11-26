using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// エンティティのオブジェクトIDを管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct ObjID : IComponentData
    {
        #region Public Fields
        public int Value;
        #endregion
    }
}
