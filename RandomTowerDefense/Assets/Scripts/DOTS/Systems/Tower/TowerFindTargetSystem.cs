using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

/// <summary>
/// 空間分割を使用してタワーの最適ターゲットを探すECSシステム
/// 効率的な近働クエリのためにクアドラントベースの空間ハッシュを使用
/// 戦略的ターゲッティングのため城に最も近い敵を優先
/// 新しい空間データを確保するためQuadrantSystemの後に更新
/// </summary>
[UpdateAfter(typeof(QuadrantSystem))]
public class TowerFindTargetSystem : JobComponentSystem
{
    [RequireComponentTag(typeof(PlayerTag), typeof(Translation), typeof(Radius), typeof(QuadrantEntity), typeof(CastlePos))]
    [ExcludeComponent(typeof(Target))]
    [BurstCompile]
    private struct FindTargetQuadrantSystemJob : IJobChunk
    {
        [ReadOnly] public EntityTypeHandle entityType;
        [ReadOnly] public ComponentTypeHandle<Translation> translationType;
        [ReadOnly] public ComponentTypeHandle<Radius> radiusType;
        [ReadOnly] public ComponentTypeHandle<QuadrantEntity> quadrantEntityType;

        [ReadOnly] public ComponentTypeHandle<CastlePos> castlePosType;

        [ReadOnly] public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;

        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslation = chunk.GetNativeArray(translationType);
            var chunkRadius = chunk.GetNativeArray(radiusType);
            var chunkQuadrantEntity = chunk.GetNativeArray(quadrantEntityType);
            var chunkCastlePos = chunk.GetNativeArray(castlePosType);
            var chunkEntity = chunk.GetNativeArray(entityType);

            for (int i = 0; i < chunk.Count; ++i)
            {
                float3 unitPosition = chunkTranslation[i].Value;
                Entity closestTargetEntity = Entity.Null;
                float closestTargetDistance = float.MaxValue;
                float3 closestTargetPosition = chunkTranslation[i].Value;
                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(chunkTranslation[i].Value);

                FindTarget(hashMapKey - 1 - QuadrantSystem.quadrantYMultiplier, unitPosition, chunkRadius[i].Value, chunkQuadrantEntity[i], chunkCastlePos[i].Value, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                if (closestTargetEntity == Entity.Null)
                {
                    FindTarget(hashMapKey - 1, unitPosition, chunkRadius[i].Value, chunkQuadrantEntity[i], chunkCastlePos[i].Value, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                    FindTarget(hashMapKey - QuadrantSystem.quadrantYMultiplier, unitPosition, chunkRadius[i].Value, chunkQuadrantEntity[i], chunkCastlePos[i].Value, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                }
                if (closestTargetEntity == Entity.Null)
                {
                    FindTarget(hashMapKey, unitPosition, chunkRadius[i].Value, chunkQuadrantEntity[i], chunkCastlePos[i].Value, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                    FindTarget(hashMapKey - 1 + QuadrantSystem.quadrantYMultiplier, unitPosition, chunkRadius[i].Value, chunkQuadrantEntity[i], chunkCastlePos[i].Value, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                    FindTarget(hashMapKey + 1 - QuadrantSystem.quadrantYMultiplier, unitPosition, chunkRadius[i].Value, chunkQuadrantEntity[i], chunkCastlePos[i].Value, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                }
                if (closestTargetEntity == Entity.Null)
                {
                    FindTarget(hashMapKey + 1, unitPosition, chunkRadius[i].Value, chunkQuadrantEntity[i], chunkCastlePos[i].Value, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                    FindTarget(hashMapKey + QuadrantSystem.quadrantYMultiplier, unitPosition, chunkRadius[i].Value, chunkQuadrantEntity[i], chunkCastlePos[i].Value, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                }
                if (closestTargetEntity == Entity.Null)
                {
                    FindTarget(hashMapKey + 1 + QuadrantSystem.quadrantYMultiplier, unitPosition, chunkRadius[i].Value, chunkQuadrantEntity[i], chunkCastlePos[i].Value, ref closestTargetEntity, ref closestTargetDistance, ref closestTargetPosition);
                }

                entityCommandBuffer.AddComponent(i, chunkEntity[i], new Target
                {
                    targetEntity = closestTargetEntity,
                    targetPos = closestTargetPosition,
                    targetHealth = 1,
                });
            }
        }

        private void FindTarget(int hashMapKey, float3 unitPosition, float maxdist, QuadrantEntity quadrantEntity, float3 castlePos, ref Entity closestTargetEntity, ref float closestTargetDistance, ref float3 closestTargetPosition)
        {
            QuadrantData quadrantData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
            {
                //Debug.Log("findtarget");
                do
                {
                    if (quadrantData.quadrantEntity.typeEnum == QuadrantEntity.TypeEnum.EnemyTag)
                    {
                        float distCastleSq = math.distancesq(castlePos, quadrantData.position);
                        if (distCastleSq < closestTargetDistance &&
                            CheckCollision(unitPosition, quadrantData.position, maxdist * maxdist / 1.21f))
                        {
                            // 現在の最適ターゲットよりも近いターゲットを発見
                            closestTargetEntity = quadrantData.entity;
                            closestTargetDistance = distCastleSq;
                            closestTargetPosition = quadrantData.position;
                        }
                    }
                } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
            }
        }


    }

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (GetEntityQuery(typeof(EnemyTag)).CalculateEntityCount() == 0) return inputDeps;

        EntityQuery unitQuery = GetEntityQuery(typeof(PlayerTag), typeof(Radius)/*, ComponentType.Exclude<Target>()*/);

        var entityType = GetEntityTypeHandle();
        var translationType = GetComponentTypeHandle<Translation>(true);
        var radiusType = GetComponentTypeHandle<Radius>(true);
        var quadrantEntityType = GetComponentTypeHandle<QuadrantEntity>(true);
        var castlePosType = GetComponentTypeHandle<CastlePos>(true);

        var findTargetQuadrantSystemJob = new FindTargetQuadrantSystemJob
        {
            entityType = entityType,
            translationType = translationType,
            radiusType = radiusType,
            quadrantEntityType = quadrantEntityType,
            castlePosType = castlePosType,
            quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap,
            entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
        };
        JobHandle jobHandle = findTargetQuadrantSystemJob.Schedule(unitQuery, inputDeps);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }

    /// <summary>
    /// パフォーマンスのためで2つの位置間の距離の二乗を計算
    /// </summary>
    /// <param name="posA">最初の位置</param>
    /// <param name="posB">2番目の位置</param>
    /// <returns>位置間の距離の二乗</returns>
    static private float GetDistance(float3 posA, float3 posB)
    {
        float3 delta = posA - posB;
        return delta.x * delta.x + delta.z * delta.z;
    }

    /// <summary>
    /// 距離の二乗を使用して2つの位置が衝突範囲内にあるかチェック
    /// </summary>
    /// <param name="posA">最初の位置</param>
    /// <param name="posB">2番目の位置</param>
    /// <param name="radiusSqr">衝突半径の二乗</param>
    /// <returns>位置が衝突範囲内にある場合true</returns>
    static private bool CheckCollision(float3 posA, float3 posB, float radiusSqr)
    {
        return GetDistance(posA, posB) <= radiusSqr;
    }
}

