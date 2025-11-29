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
    /// 敵詳細情報クラス - ウェーブ内の敵スポーン設定データ
    /// </summary>
    public class WaveDetail
    {
        /// <summary>
        /// ウェーブID
        /// </summary>
        public int waveID;

        /// <summary>
        /// 敵の数
        /// </summary>
        public int enemyNumber;

        /// <summary>
        /// スポーンポートID
        /// </summary>
        public int enemyPort;

        /// <summary>
        /// 敵タイプ
        /// </summary>
        public string enemyType;

        /// <remarks>
        /// コンストラクタ
        /// </remarks>
        /// <param name="waveID">ウェーブID</param>
        /// <param name="enemyNumber">敵の数</param>
        /// <param name="enemyPort">スポーンポートID</param>
        /// <param name="enemyType">敵タイプ</param>
        public WaveDetail(int waveID, int enemyNumber, int enemyPort, string enemyType)
        {
            this.waveID = waveID;
            this.enemyNumber = enemyNumber;
            this.enemyPort = enemyPort;
            this.enemyType = enemyType;
        }
    }
}
