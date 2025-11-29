using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RandomTowerDefense.DOTS.Systems.Timer
{
    /// <summary>
    /// タワー、攻撃、スキルのカウントダウンタイマーを処理する統合タイマーシステム
    /// ジョブ依存関係の連鎖でパフォーマンスを向上させるため別々のタイマーシステムを統合
    /// </summary>
    public class UnifiedTimerSystem : JobComponentSystem
    {
        /// <summary>
        /// 連鎖したジョブシーケンスですべてのタイマーコンポーネントをデルタタイムで更新
        /// 依存関係の順序でタワー、攻撃、スキルを処理
        /// </summary>
        /// <param name="inputDeps">入力ジョブ依存関係</param>
        /// <returns>すべてのタイマー更新後の最終ジョブハンドル</returns>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float deltaTime = Time.DeltaTime;

            // タワーの待機時間を更新
            var towerJob = Entities.WithAll<PlayerTag>().ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait) =>
            {
                wait.Value -= deltaTime;
            }).Schedule(inputDeps);

            // 攻撃タイマーと待機時間を更新
            var attackJob = Entities.WithAll<AttackTag>().ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait, ref ActionTime action) =>
            {
                if (action.Value > 0)
                    action.Value -= deltaTime;
                wait.Value -= deltaTime;
            }).Schedule(towerJob);

            // スキルの待機時間を更新
            var skillJob = Entities.WithAll<SkillTag>().ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait, ref ActionTime action) =>
            {
                wait.Value -= deltaTime;
            }).Schedule(attackJob);

            return skillJob;
        }
    }
}