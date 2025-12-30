using System;
using Unity.Entities;
using Unity.Mathematics;

namespace RandomTowerDefense.DOTS.Components
{
    /// <summary>
    /// [ECS] Target - ターゲッティング情報を管理するコンポーネント
    ///
    /// 主な機能:
    /// - エンティティのターゲット情報管理
    /// - ターゲットエンティティの追跡
    /// - ターゲット位置とヘルス情報の保持
    /// - Burst互換の高性能データ構造
    /// </summary>
    [Serializable]
    public struct Target : IComponentData
    {
        #region IComponentData Implementation

        /// <summary>
        /// ターゲットエンティティ
        /// </summary>
        public Entity targetEntity;

        /// <summary>
        /// ターゲットの位置
        /// </summary>
        public float3 targetPos;

        /// <summary>
        /// ターゲットのヘルス値
        /// </summary>
        public float targetHealth;

        #endregion

        #region Public Properties

        /// <summary>
        /// 有効なターゲットが設定されているかどうか
        /// </summary>
        public bool HasValidTarget => targetEntity != Entity.Null;

        /// <summary>
        /// ターゲットが生きているかどうか（ヘルス > 0）
        /// </summary>
        public bool IsTargetAlive => targetHealth > 0f;

        /// <summary>
        /// ターゲットとの距離を計算
        /// </summary>
        /// <param name="sourcePos">発射元の位置</param>
        /// <returns>ターゲットとの距離</returns>
        public float GetDistanceToTarget(float3 sourcePos) => math.distance(sourcePos, targetPos);

        /// <summary>
        /// ターゲットへの方向ベクトルを計算
        /// </summary>
        /// <param name="sourcePos">発射元の位置</param>
        /// <returns>正規化された方向ベクトル</returns>
        public float3 GetDirectionToTarget(float3 sourcePos)
        {
            float3 direction = targetPos - sourcePos;
            return math.normalizesafe(direction);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// ターゲット情報でTargetコンポーネントを初期化
        /// </summary>
        /// <param name="entity">ターゲットエンティティ</param>
        /// <param name="position">ターゲット位置</param>
        /// <param name="health">ターゲットヘルス</param>
        public Target(Entity entity, float3 position, float health)
        {
            targetEntity = entity;
            targetPos = position;
            targetHealth = health;
        }

        #endregion
    }
}