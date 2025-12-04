using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using System.Linq;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;

namespace RandomTowerDefense.DOTS.Systems
{
    /// <summary>
    /// ヘルスが0以下になった敵エンティティを処理するシステム
    /// エンティティから敵タグとクアドラントコンポーネントを削除
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class RemoveDeadSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
        protected override void OnCreate()
        {
            _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        /// <summary>
        /// 死亡した敵エンティティの削除処理を更新
        /// </summary>
        /// <param name="inputDeps">入力依存関係</param>
        /// <returns>ジョブハンドル</returns>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer ecb = _endSimulationEcbSystem.CreateCommandBuffer();
            EntityCommandBuffer.ParallelWriter ecbc = ecb.AsParallelWriter();

            return Entities.WithAll<EnemyTag>().ForEach((Entity entity, int entityInQueryIndex, ref Health health) =>
            {
                if (health.Value < 0)
                {
                    ecbc.RemoveComponent<QuadrantEntity>(entityInQueryIndex, entity);
                    ecbc.RemoveComponent<EnemyTag>(entityInQueryIndex, entity);
                    //ecbc.DestroyEntity(entityInQueryIndex, entity);
                }

            }).Schedule(inputDeps);
        }

        //Alternative using Parallel Jobs
        //protected override JobHandle OnUpdate(JobHandle inputDeps)
        //{
        //    EntityCommandBuffer ecb = endSimulationEcbSystem.CreateCommandBuffer();

        //    NativeArray<Entity> entityArray = new NativeArray<Entity>();
        //    Entities.WithAll<EnemyTag>().ForEach((Entity entity, int entityInQueryIndex, ref Health health) =>
        //    {
        //        if (health.Value <= 0)
        //        {
        //            entityArray.Append(entity);
        //            //ecbc.DestroyEntity(entityInQueryIndex, entity);
        //        }
        //    }).WithoutBurst().Run();

        //    if (entityArray.Count() == 0) return inputDeps;

        //    KillJob killTag = new KillJob
        //    {
        //        ecb = ecb.AsParallelWriter(),
        //        entityBuffer = entityArray,
        //    };

        //    if (entityArray.Count() > 0)
        //    {
        //        inputDeps = killTag.Schedule(entityArray.Count(), 5);
        //        inputDeps.Complete();
        //    }
        //    return inputDeps;
        //}

        //[BurstCompile]
        //[RequireComponentTag(typeof(EnemyTag))]
        //private struct KillJob : IJobParallelFor
        //{
        //    [ReadOnly] public EntityCommandBuffer.ParallelWriter ecb;
        //    [DeallocateOnJobCompletion]
        //    public NativeArray<Entity> entityBuffer;

        //    public void Execute(int i)
        //    {
        //            ecb.RemoveComponent<EnemyTag>(i, entityBuffer[i]);
        //    }
        //}
    }
}
