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

        /// <summary>
        /// 移動速度の値
        /// </summary>
        public float Value;

        #endregion
    }
}

