using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;

namespace RandomTowerDefense.DOTS.Systems.Enemy
{
    /// <summary>
    /// 敵エンティティのバフ/デバフ効果の継続時間と回復を管理するシステム
    /// スロー効果と石化効果の時間経過による減少を処理
    /// </summary>
    public class EnemyBuffCntSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
        }

        /// <summary>
        /// 敵のバフ/デバフ効果の時間減少と回復処理を更新
        /// </summary>
        /// <param name="inputDeps">入力依存関係</param>
        /// <returns>ジョブハンドル</returns>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float recoveryRate = 0.2f;
            float deltaTime = Time.DeltaTime;

            return Entities.WithAll<EnemyTag>().ForEach((Entity entity, ref SlowRate slowRate, ref PetrifyAmt petrifyAmt, ref BuffTime buffTime) =>
            {
                if (buffTime.Value > 0)
                {
                    buffTime.Value -= deltaTime;
                }
                else
                {
                    if (slowRate.Value > 0)
                    {
                        slowRate.Value = Mathf.Max(slowRate.Value - recoveryRate * deltaTime, 0f);
                    }
                    if (petrifyAmt.Value > 0)
                    {
                        petrifyAmt.Value = Mathf.Max(petrifyAmt.Value - recoveryRate * deltaTime, 0f);
                    }
                }

            }).Schedule(inputDeps);
        }
    }
}
