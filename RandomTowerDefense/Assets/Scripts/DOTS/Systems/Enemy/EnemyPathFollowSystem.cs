using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;
using RandomTowerDefense.DOTS.Pathfinding;

//[DisableAutoCreation]
/// <summary>
/// A*パスフィンディングに基づいて敵エンティティの移動を処理するシステム
/// 敵の速度、スロー効果、石化効果を考慮した移動計算を行う
/// </summary>
public class EnemyPathFollowSystem : JobComponentSystem
{
    /// <summary>
    /// 敵エンティティのパス追従処理を更新
    /// </summary>
    /// <param name="inputDeps">入力依存関係</param>
    /// <returns>ジョブハンドル</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        return Entities.WithAll<EnemyTag>().ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuffer,
            ref Translation transform, ref Health health, ref Speed speed, ref SlowRate slow, ref PetrifyAmt petrifyAmt,
            ref PathFollow pathFollow) =>
        {
            if (health.Value > 0 && pathFollow.pathIndex >= 0 && pathFollow.pathIndex < pathPositionBuffer.Length)
            {
                // 追従するパスが存在
                PathPosition pathPosition = pathPositionBuffer[pathFollow.pathIndex];
                float3 pathActualPos = PathfindingGridSetup.Instance.pathfindingGrid.GetWorldPosition((int)pathPosition.position.x, (int)pathPosition.position.y);

                float3 targetPosition = new float3(pathActualPos.x, transform.Value.y, pathActualPos.z);
                // Debug.DrawLine(targetPosition, targetPosition + new float3(0,1,0), Color.green);

                float3 moveDir = math.normalizesafe(targetPosition - transform.Value);
                float moveSpeed = speed.Value * (1 - slow.Value) * (1 - petrifyAmt.Value);
                //Debug.DrawLine(transform.translation, targetPosition);

                transform.Value += moveDir * moveSpeed * deltaTime;

                //float2 dirXZ = math.normalizesafe(new float2(moveDir.x, moveDir.z));
                //if (moveDir.x == 0) moveDir.x = 0.0001f;
                //bool isFront = Mathf.Acos(Vector2.Dot(new float2(0, 1),dirXZ))>0;
                //transform.angle = 90f + Mathf.Acos(Vector2.Dot(new float2(1, 0),dirXZ)) * Mathf.Rad2Deg;
                //if (isFront == false)
                //    transform.angle *= -1f;
                //Mathf.Atan(-moveDir.z / moveDir.x) * Mathf.Rad2Deg;

                if (math.distance(transform.Value, targetPosition) < .1f)
                {
                    // 次のウェイポイントへ
                    pathFollow.pathIndex--;
                }
            }
        }).WithoutBurst().Schedule(inputDeps);
    }

    /// <summary>
    /// グリッド位置を有効な範囲内に制限
    /// </summary>
    /// <param name="x">X座標（参照渡し）</param>
    /// <param name="y">Y座標（参照渡し）</param>
    private void ValidateGridPosition(ref int x, ref int y)
    {
        x = math.clamp(x, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetHeight() - 1);
    }

}

[UpdateAfter(typeof(EnemyPathFollowSystem))]
[DisableAutoCreation]
/// <summary>
/// 新しいパスが必要な敵エンティティにパスフィンディングパラメータを追加するシステム
/// 城に向かう経路計算の開始点を設定
/// </summary>
public class PathFollowGetNewPathSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    /// <summary>
    /// 新しいパスが必要なエンティティにパスフィンディングパラメータを追加
    /// </summary>
    /// <param name="inputDeps">入力依存関係</param>
    /// <returns>ジョブハンドル</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int mapWidth = PathfindingGridSetup.Instance.pathfindingGrid.GetWidth();
        int mapHeight = PathfindingGridSetup.Instance.pathfindingGrid.GetHeight();
        float3 originPosition = PathfindingGridSetup.Instance.pathfindingGrid.GetWorldPosition(0, 0);
        float cellSize = PathfindingGridSetup.Instance.pathfindingGrid.GetCellSize();

        EntityCommandBuffer.ParallelWriter entityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        JobHandle jobHandle = Entities.WithNone<PathfindingParams>().ForEach((Entity entity, int entityInQueryIndex, in PathFollow pathFollow, in Translation transform) =>
        {
            if (pathFollow.pathIndex == -1)
            {

                GetXY(transform.Value + new float3(1, 0, 1) * cellSize * +.5f, originPosition, cellSize, out int startX, out int startY);
                ValidateGridPosition(ref startX, ref startY, mapWidth, mapHeight);

                // 城への固定ターゲット位置
                int endX = 0;
                int endY = 0;

                entityCommandBuffer.AddComponent(entityInQueryIndex, entity, new PathfindingParams
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(endX, endY)
                });
            }
        }).Schedule(inputDeps);

        _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }

    /// <summary>
    /// グリッド位置を指定した範囲内に制限
    /// </summary>
    /// <param name="x">X座標（参照渡し）</param>
    /// <param name="y">Y座標（参照渡し）</param>
    /// <param name="width">グリッドの幅</param>
    /// <param name="height">グリッドの高さ</param>
    private static void ValidateGridPosition(ref int x, ref int y, int width, int height)
    {
        x = math.clamp(x, 0, width - 1);
        y = math.clamp(y, 0, height - 1);
    }

    /// <summary>
    /// ワールド座標をグリッド座標に変換
    /// </summary>
    /// <param name="worldPosition">ワールド位置</param>
    /// <param name="originPosition">原点位置</param>
    /// <param name="cellSize">セルサイズ</param>
    /// <param name="x">出力X座標</param>
    /// <param name="y">出力Y座標</param>
    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y)
    {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }

}
