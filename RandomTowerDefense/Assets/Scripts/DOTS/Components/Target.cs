using System;
using Unity.Entities;
using Unity.Mathematics;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// ターゲッティング情報を管理するコンポーネント
    /// </summary>
    [Serializable]
    public struct Target : IComponentData
    {
        #region Public Fields
        public Entity targetEntity;
        public float3 targetPos;
        public float targetHealth;
        #endregion
    }
}