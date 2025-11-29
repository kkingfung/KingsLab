using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Physics;
using UnityEditor;
using UnityEngine;
using Unity.RemoteConfig;
using System.IO;

namespace RandomTowerDefense.Info
{
    /// <summary>
    /// ウェーブ属性クラス - 敵ウェーブのタイミングとスポーン設定
    /// </summary>
    public class WaveAttr
    {
        /// <summary>
        /// ウェーブ開始時間
        /// </summary>
        public float enemyStartTime;

        /// <summary>
        /// 敵スポーン間隔時間
        /// </summary>
        public float enemySpawnPeriod;

        /// <summary>
        /// ウェーブ内の敵詳細リスト
        /// </summary>
        public List<WaveDetail> waveDetails;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waveDetails">ウェーブ内の敵詳細リスト</param>
        /// <param name="enemyStartTime">ウェーブ開始時間</param>
        /// <param name="enemySpawnPeriod"> 敵スポーン間隔時間</param>
        public WaveAttr(List<WaveDetail> waveDetails,
            float enemyStartTime = 1.5f, float enemySpawnPeriod = 0.5f)
        {
            this.enemyStartTime = enemyStartTime;
            this.enemySpawnPeriod = enemySpawnPeriod;
            this.waveDetail = waveDetails;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="wave">ウェーブ属性</param>
        public WaveAttr(WaveAttr wave)
        {
            enemyStartTime = wave.enemyStartTime;
            enemySpawnPeriod = wave.enemySpawnPeriod;
            waveDetail = wave.waveDetail;
        }
    }
}
