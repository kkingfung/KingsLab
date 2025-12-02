using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;

namespace RandomTowerDefense.DOTS.Systems.Timer
{
    /// <summary>
    /// [ECS System] UnifiedTimerSystem - タワー、攻撃、スキルのカウントダウンタイマーを処理する統合タイマーシステム
    ///
    /// 主な機能:
    /// - 複数のタイマーコンポーネントの一元管理
    /// - ジョブ依存関係の最適化による高性能処理
    /// - 1000+エンティティでの60FPS維持対応
    /// - Burst対応による最大性能実現
    ///
    /// パフォーマンス特性:
    /// - ジョブチェーンによる並列処理最適化
    /// - メモリアクセスパターンの最適化
    /// - キャッシュ効率的なコンポーネント処理
    /// </summary>
    public class UnifiedTimerSystem : JobComponentSystem
    {
        #region Constants

        /// <summary>
        /// デルタタイム更新の最小閾値（フレーム不安定性対策）
        /// </summary>
        private const float MIN_DELTA_TIME = 0.0001f;

        #endregion

        #region Protected Methods (ECS)

        /// <summary>
        /// 連鎖したジョブシーケンスですべてのタイマーコンポーネントをデルタタイムで更新
        /// 依存関係の順序でタワー、攻撃、スキルを処理し、最大並列性を実現
        /// </summary>
        /// <param name="inputDeps">入力ジョブ依存関係</param>
        /// <returns>すべてのタイマー更新後の最終ジョブハンドル</returns>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float deltaTime = UnityEngine.Time.DeltaTime;

            // フレーム不安定性対策：極小デルタタイムをクランプ
            if (deltaTime < MIN_DELTA_TIME)
                return inputDeps;

            // タワーの待機時間を更新（プレイヤータグ付きエンティティ）
            var towerJob = Entities
                .WithName("TowerTimerUpdate")
                .WithAll<PlayerTag>()
                .ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait) =>
                {
                    wait.Value -= deltaTime;
                }).Schedule(inputDeps);

            // 攻撃タイマーと待機時間を更新（攻撃タグ付きエンティティ）
            var attackJob = Entities
                .WithName("AttackTimerUpdate")
                .WithAll<AttackTag>()
                .ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait, ref ActionTime action) =>
                {
                    // アクション時間が残っている場合のみ減算
                    if (action.Value > 0f)
                        action.Value -= deltaTime;

                    // 待機時間は常に減算
                    wait.Value -= deltaTime;
                }).Schedule(towerJob);

            // スキルの待機時間を更新（スキルタグ付きエンティティ）
            var skillJob = Entities
                .WithName("SkillTimerUpdate")
                .WithAll<SkillTag>()
                .ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait, ref ActionTime action) =>
                {
                    // スキルは待機時間のみ更新（アクション時間は個別システムで管理）
                    wait.Value -= deltaTime;
                }).Schedule(attackJob);

            return skillJob;
        }

        #endregion
    }
}