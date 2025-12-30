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
        /// <summary>
        /// マップサイズ
        /// </summary>
        [Header("マップサイズ")]
        public FilledMapCoord mapSize;

        /// <summary>
        /// 障害物出現率（0.0～1.0）
        /// </summary>
        [Header("障害物設定")]
        [Range(0, 1)] public float obstaclePercent;

        /// <summary>
        /// 障害物の最小高さ
        /// </summary>
        public float minObstacleHeight;

        /// <summary>
        /// 障害物の最大高さ
        /// </summary>
        public float maxObstacleHeight;

        /// <summary>
        /// ランダム生成用シード値
        /// </summary>
        [Header("生成設定")]
        public int seed;

        /// <summary>
        /// 前景カラー
        /// </summary>
        [Header("カラー設定")]
        public Color foregroundColour;

        /// <summary>
        /// 背景カラー
        /// </summary>
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
