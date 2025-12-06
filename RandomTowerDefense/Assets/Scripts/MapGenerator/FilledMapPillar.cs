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
    /// 柱情報
    /// </summary>
    public class Pillar
    {
        /// <summary>
        /// 柱のオブジェクト
        /// </summary>
        public GameObject obj;

        /// <summary>
        /// マップ上のサイズ
        /// </summary>
        public FilledMapCoord mapSize;

        /// <summary>
        /// 状態
        /// </summary>
        public int state;//0: Empty 1: Occupied

        /// <summary>
        /// 高さ
        /// </summary>
        public float height;

        /// <summary>
        /// 周囲の空きスペース
        /// </summary>
        public int surroundSpace;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="obj">柱のオブジェクト</param>
        /// <param name="x">マップ上のX座標</param>
        /// <param name="y">マップ上のY座標</param>
        /// <param name="height">高さ</param>
        /// <param name="state">状態</param>
        public Pillar(GameObject obj, int x, int y, float height, int state = 0)
        {
            this.obj = obj;
            mapSize.x = x;
            mapSize.y = y;
            this.state = state;
            this.height = height;
        }
    }
}
