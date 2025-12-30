using UnityEngine;

namespace RandomTowerDefense.Utility.Math
{
    /// <summary>
    /// 拡張メソッドライブラリ - Vector3とVector2変換ユーティリティ
    ///
    /// 主な機能:
    /// - Vector3のXZ成分をVector2に変換
    /// - 3D座標から2D座標への効率的な変換
    /// - パフォーマンス最適化された型変換
    /// </summary>
    public static class ExtensionMethods
    {
        #region Vector Conversion Methods

        /// <summary>
        /// Vector3のX、Z成分をVector2に変換します
        /// </summary>
        /// <param name="v3">変換元のVector3</param>
        /// <returns>X、Z成分を含むVector2</returns>
        public static Vector2 ToXZ(this Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }

        #endregion
    }
}
