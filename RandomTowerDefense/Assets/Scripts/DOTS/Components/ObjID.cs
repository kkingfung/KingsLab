using System;
using UnityEngine;
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

        /// <summary>
        /// オブジェクトID
        /// </summary>
        public int Value;

        #endregion
    }
}
