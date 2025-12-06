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

        /// <summary>
        /// 減速率の値
        /// </summary>
        public float Value;

        #endregion
    }
}

