using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RandomTowerDefense.Boids.DOTS
{
    /// <summary>
    /// DOTS用ボイドコンポーネント定義ファイル - ECSアーキテクチャ対応フロッキングシステム
    ///
    /// 主な機能:
    /// - Unity DOTSエンティティ用コンポーネントデータ構造定義
    /// - フロッキング動作計算用データコンポーネント群
    /// - 高性能並列処理対応のfloat3ベクトル使用
    /// - シリアライゼーション対応IComponentData実装
    /// - 球面境界システムとパフォーマンス最適化設定
    /// - DOTS/MonoBehaviour間のデータ変換サポート
    /// </summary>

    #region Core Components

    /// <summary>
    /// ボイドエンティティ識別タグ - フィルタリング用マーカーコンポーネント
    /// </summary>
    [Serializable]
    public struct BoidTag : IComponentData
    {
    }

    /// <summary>
    /// ボイドデータコンポーネント - 位置、方向、フロッキング計算結果
    /// </summary>
    [Serializable]
    public struct BoidData : IComponentData
    {
        [Header("現在状態")]
        public float3 position;   // 現在位置
        public float3 direction;  // 現在方向

        [Header("フロッキング計算結果")]
        public float3 flockHeading;      // 群れ全体の方向
        public float3 flockCentre;       // 群れの中心位置
        public float3 avoidanceHeading;  // 回避方向
        public int numFlockmates;        // 知覚範囲内の仲間数
    }

    /// <summary>
    /// ボイド回転コンポーネント - 3D回転制御
    /// </summary>
    [Serializable]
    public struct BoidRotation : IComponentData
    {
        public Quaternion rotation;
    }

    #endregion

    #region Average Data Components

    /// <summary>
    /// ボイド平均データコンポーネント - フロッキング平均値計算結果
    /// </summary>
    [Serializable]
    public struct BoidDataAvg : IComponentData
    {
        public float3 avgFlockHeading;        // 平均フロック方向
        public float3 avgAvoidanceHeading;    // 平均回避方向
        public float3 centreOfFlockmates;     // フロックメート中心
        public int numPerceivedFlockmates;    // 知覚されたフロックメート数
    }

    #endregion

    #region Settings Components

    /// <summary>
    /// DOTS用ボイド設定コンポーネント - フロッキングパラメーター
    /// </summary>
    [Serializable]
    public struct BoidSettingDots : IComponentData
    {
        [Header("移動設定")]
        public float minSpeed;           // 最小速度
        public float maxSpeed;           // 最大速度
        public float perceptionRadius;   // 知覚範囲
        public float avoidanceRadius;    // 回避範囲
        public float maxSteerForce;      // 最大操舵力

        [Header("フロッキング重み")]
        public float alignWeight;        // 整列重み
        public float cohesionWeight;     // 結束重み
        public float seperateWeight;     // 分離重み

        [Header("ターゲット追従")]
        public float targetWeight;       // ターゲット重み

        [Header("衝突設定")]
        public float boundsRadius;           // 境界半径
        public float avoidCollisionWeight;   // 衝突回避重み
        public float collisionAvoidDst;      // 衝突回避距離
    }

    #endregion

    #region Utility Components

    /// <summary>
    /// 原点位置コンポーネント - 球面境界システム用
    /// GameObjectバウンディングボックスの代替として球面境界を使用
    /// </summary>
    [Serializable]
    public struct OriPos : IComponentData
    {
        public float3 Value;           // 原点位置
        public float BoundingRadius;   // 境界半径
    }

    #endregion
}