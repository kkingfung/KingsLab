using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// 経験値情報を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct Exp : IComponentData
    {
        #region Public Fields
        public int Value;
        #endregion
    }
}

