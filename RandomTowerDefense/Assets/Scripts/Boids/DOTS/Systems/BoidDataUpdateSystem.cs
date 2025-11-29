using Unity;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;
using RandomTowerDefense.Boids.DOTS;

namespace RandomTowerDefense.Boids.DOTS.Systems
{
    /// <summary>
    /// ボイドデータ更新システム - ECS並列処理によるフロッキング近隣検索システム
    ///
    /// 主な機能:
    /// - IJobChunkによるマルチスレッド並列処理フロッキング計算
    /// - Burst Compilerによる高性能数学演算最適化
    /// - 近隣ボイド検索とフロッキングベクトル計算の統合処理
    /// - 知覚範囲内ボイド数カウントと群れ中心位置計算
    /// - 回避範囲内での反発力計算と累積処理
    /// - NativeArray安全性制約回避による高速メモリアクセス
    /// </summary>
    public class BoidDataUpdateSystem : ComponentSystem
    {
        #region Private Fields

        private EntityQuery _boidGroup;

        #endregion

        #region System Lifecycle

        /// <summary>
        /// システム作成時初期化 - エンティティクエリ設定
        /// </summary>
        protected override void OnCreate()
        {
            _boidGroup = GetEntityQuery(typeof(BoidData),
                ComponentType.ReadOnly<BoidTag>());
        }

        /// <summary>
        /// システム更新処理 - 並列ジョブスケジューリング実行
        /// </summary>
        protected override void OnUpdate()
        {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            NativeArray<BoidData> targetDataArray = _boidGroup.ToComponentDataArray<BoidData>(Allocator.Temp);

            if (_boidGroup.CalculateEntityCount() > 0)
            {
                Entities.WithAll<BoidTag>().ForEach((Entity entity) =>
                {
                    NativeArray<BoidData> targetData = new NativeArray<BoidData>(targetDataArray, Allocator.TempJob);

                    var dataType = GetComponentTypeHandle<BoidData>(false);
                    var settingType = GetComponentTypeHandle<BoidSettingDots>(true);

                    var jobData = new UpdateBoidData()
                    {
                        dataType = dataType,
                        settingType = settingType,
                        targetData = targetData
                    };

                    jobHandleList.Add(jobData.Schedule(_boidGroup));
                });
            }

            JobHandle.CompleteAll(jobHandleList);
        }

        #endregion

        #region Job Structs

        /// <summary>
        /// ボイドデータ更新ジョブ - Burstコンパイル対応並列フロッキング計算
        /// </summary>
        [BurstCompile]
        struct UpdateBoidData : IJobChunk
        {
            #region Job Fields

            [NativeDisableContainerSafetyRestriction]
            public ComponentTypeHandle<BoidData> dataType;

            [ReadOnly]
            public ComponentTypeHandle<BoidSettingDots> settingType;

            [DeallocateOnJobCompletion]
            public NativeArray<BoidData> targetData;

            #endregion

            #region Job Execution

            /// <summary>
            /// チャンク実行処理 - 各ボイドエンティティの近隣検索とフロッキング計算
            /// </summary>
            /// <param name="chunk">処理対象チャンク</param>
            /// <param name="chunkIndex">チャンクインデックス</param>
            /// <param name="firstEntityIndex">最初のエンティティインデックス</param>
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkData = chunk.GetNativeArray(dataType);
                var chunkSetting = chunk.GetNativeArray(settingType);

                // チャンク内の各エンティティ処理
                for (int i = 0; i < chunk.Count; ++i)
                {
                    BoidData data = chunkData[i];
                    BoidSettingDots setting = chunkSetting[i];

                    // 近隣ボイド検索とフロッキング計算
                    for (int j = 0; j < targetData.Length; j++)
                    {
                        if (i == j) continue;

                        float3 offset = targetData[j].position - data.position;
                        float sqrDst = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                        // 知覚範囲内のボイド処理
                        if (sqrDst < setting.perceptionRadius * setting.perceptionRadius)
                        {
                            data.numFlockmates += 1;
                            data.flockHeading += targetData[j].direction;
                            data.flockCentre += targetData[j].position;

                            // 回避範囲内での反発力計算
                            if (sqrDst < setting.avoidanceRadius * setting.avoidanceRadius)
                            {
                                data.avoidanceHeading -= offset / sqrDst;
                            }
                        }
                    }

                    chunkData[i] = data;
                }
            }

            #endregion
        }

        #endregion
    }
}
