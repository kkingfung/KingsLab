using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// 石化効果の量を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct PetrifyAmt : IComponentData
    {
        #region Public Fields
        public float Value;
        #endregion
    }
}


