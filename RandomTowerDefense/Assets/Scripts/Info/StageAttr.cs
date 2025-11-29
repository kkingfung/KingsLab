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
    /// ステージ属性クラス - ステージ全体のウェーブ設定と難易度設定
    /// </summary>
    public class StageAttr
    {
        /// <summary>
        /// ウェーブ数
        /// </summary>
        public int waveNum;

        /// <summary>
        /// ウェーブ間待機時間
        /// </summary>
        public float waveWaitTime;

        /// <summary>
        /// 各ウェーブの属性配列
        /// </summary>
        public WaveAttr[] waveAttrs;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waveNum">ウェーブ数</param>
        /// <param name="waveAttrs">各ウェーブの属性配列</param>
        /// <param name="waveWaitTime">ウェーブ間待機時間</param>
        public StageAttr(int waveNum, WaveAttr[] waveAttrs, float waveWaitTime = 10f)
        {
            this.waveNum = waveNum;
            this.waveAttrs = waveAttrs;
            this.waveWaitTime = waveWaitTime;
        }
    }
}
