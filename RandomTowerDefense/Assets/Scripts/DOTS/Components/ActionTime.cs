using System;
using Unity.Entities;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// 最初の衝突チェックまでの待機時間を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct ActionTime : IComponentData
    {
        #region Public Fields
        public float Value;
        #endregion
    }
}

