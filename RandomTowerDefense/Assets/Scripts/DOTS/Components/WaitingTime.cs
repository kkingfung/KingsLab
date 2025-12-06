using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// 次の繰り返し衝突チェックまでの待機時間を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct WaitingTime : IComponentData
    {
        #region Public Fields
        public float Value;
        #endregion
    }
}

