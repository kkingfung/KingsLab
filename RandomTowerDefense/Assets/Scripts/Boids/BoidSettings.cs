using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTowerDefense.Boids
{
    /// <summary>
    /// ボイド設定管理 - フロッキングシミュレーション用パラメーター設定
    ///
    /// 主な機能:
    /// - フロッキング動作パラメーター設定（速度、知覚範囲、回避距離等）
    /// - 3つの基本フロッキングルール重み調整（整列、結束、分離）
    /// - 衝突回避システム設定（障害物マスク、境界半径、回避重み）
    /// - ターゲット追従動作パラメーター制御
    /// - ScriptableObjectベースの設定保存と再利用性
    /// - Unity Inspectorでのリアルタイム調整対応
    /// </summary>
    [CreateAssetMenu(fileName = "New Boid Settings", menuName = "Boids/Boid Settings")]
    public class BoidSettings : ScriptableObject
    {
        #region Movement Settings

        [Header("移動設定")]
        [Tooltip("ボイドの最小移動速度")]
        [SerializeField] public float minSpeed = 2f;

        [Tooltip("ボイドの最大移動速度")]
        [SerializeField] public float maxSpeed = 5f;

        [Tooltip("他のボイドを知覚できる範囲")]
        [SerializeField] public float perceptionRadius = 2.5f;

        [Tooltip("回避動作を開始する距離")]
        [SerializeField] public float avoidanceRadius = 1f;

        [Tooltip("操舵力の最大値")]
        [SerializeField] public float maxSteerForce = 3f;

        #endregion

        #region Flocking Behavior Weights

        [Header("フロッキング動作重み")]
        [Tooltip("整列動作の重み（近隣ボイドとの速度同期）")]
        [SerializeField] public float alignWeight = 1f;

        [Tooltip("結束動作の重み（群れの中心に向かう力）")]
        [SerializeField] public float cohesionWeight = 1f;

        [Tooltip("分離動作の重み（他のボイドから離れる力）")]
        [SerializeField] public float seperateWeight = 1f;

        [Tooltip("ターゲット追従動作の重み")]
        [SerializeField] public float targetWeight = 1f;

        #endregion

        #region Collision Settings

        [Header("衝突設定")]
        [Tooltip("障害物として認識するレイヤーマスク")]
        [SerializeField] public LayerMask obstacleMask;

        [Tooltip("ボイドの境界判定半径")]
        [SerializeField] public float boundsRadius = 0.27f;

        [Tooltip("衝突回避動作の重み")]
        [SerializeField] public float avoidCollisionWeight = 10f;

        [Tooltip("衝突回避を開始する距離")]
        [SerializeField] public float collisionAvoidDst = 5f;

        #endregion
    }
}