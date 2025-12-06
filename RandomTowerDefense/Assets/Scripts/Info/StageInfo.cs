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
    /// ステージ情報静的クラス
    /// </summary>
    public class StageInfo
    {
        /// <summary>
        /// モンスターカテゴリ別出現タイプリスト
        /// </summary>
        public string[] MonsterName;

        /// <summary>
        /// ステージサイズ情報調整用係数
        /// </summary>
        public int StageSizeFactor;

        /// <summary>
        /// ウェブ数情報調整用係数
        /// </summary>
        public int WaveNumFactor;

        /// <summary>
        /// 敵数情報調整用係数
        /// </summary>
        public float EnemyNumFactor;

        /// <summary>
        /// 敵能力の調整値情報調整用係数
        /// </summary>
        public float EnemyAttributeFactor;

        /// <summary>
        /// ユーザーのHP情報調整用係数
        /// </summary>
        public int HpMaxFactor;

        /// <summary>
        /// 障害物出現率情報調整用係数
        /// </summary>
        public float ObstacleFactor;

        /// <summary>
        /// スポーン速度情報調整用係数
        /// </summary>
        public float SpawnSpeedFactor;

        /// <summary>
        /// 資源獲得量情報調整用係数
        /// </summary>
        public float ResourceFactor;
    }
}
