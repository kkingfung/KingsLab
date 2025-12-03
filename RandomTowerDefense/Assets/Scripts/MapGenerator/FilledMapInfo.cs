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
    /// マップ設定データクラス - SerializableObject対応マップパラメーター
    /// </summary>
    [System.Serializable]
    public class FilledMapInfo
    {
        [Header("マップサイズ")]
        public FilledMapCoord mapSize;

        [Header("障害物設定")]
        [Range(0, 1)] public float obstaclePercent;
        public float minObstacleHeight;
        public float maxObstacleHeight;

        [Header("生成設定")]
        public int seed;

        [Header("カラー設定")]
        public Color foregroundColour;
        public Color backgroundColour;

        /// <summary>
        /// マップ中心座標の自動計算
        /// </summary>
        public FilledMapCoord MapCentre
        {
            get
            {
                return new FilledMapCoord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
