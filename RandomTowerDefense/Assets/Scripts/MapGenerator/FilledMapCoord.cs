using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;
using RandomTowerDefense.Scene;
using RandomTowerDefense.Managers.Macro;
using RandomTowerDefense.Info;
using RandomTowerDefense.Utilities;

namespace RandomTowerDefense.MapGenerator
{
    /// <summary>
    /// マップ上の座標
    /// </summary>
    [System.Serializable]
    public struct FilledMapCoord
    {
        /// <summary>
        /// X座標\
        /// </summary>
        public int x;

        /// <summary>
        /// Y座標
        /// </summary>
        public int y;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_x">X座標</param>
        /// <param name="_y">Y座標</param>
        public FilledMapCoord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        /// <summary>
        /// 等価判定
        /// </summary>
        /// <param name="obj">比較対象オブジェクト</param>
        /// <returns>等価ならtrue</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// ハッシュコード取得
        /// </summary>
        /// <returns>ハッシュコード</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 等価演算子オーバーロード
        /// </summary>
        /// <param name="c1">座標1</param>
        /// <param name="c2">座標2</param>
        /// <returns>等価ならtrue</returns>
        public static bool operator ==(FilledMapCoord c1, FilledMapCoord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        /// <summary>
        /// 不等価演算子オーバーロード
        /// </summary>
        /// <param name="c1">座標1</param>
        /// <param name="c2">座標2</param>
        /// <returns>不等価ならtrue</returns>
        public static bool operator !=(FilledMapCoord c1, FilledMapCoord c2)
        {
            return !(c1 == c2);
        }
    }
}
