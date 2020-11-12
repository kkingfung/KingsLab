using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

//[DisableAutoCreation]
public class EnemyPathFollowSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        return Entities.WithAll<EnemyTag>().ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuffer,
            ref CustomTransform transform, ref Health health, ref Speed speed, ref SlowRate slow, ref PetrifyAmt petrifyAmt,
            ref PathFollow pathFollow) => {
                if (health.Value > 0 && pathFollow.pathIndex >= 0)
                {
                    // Has path to follow
                    PathPosition pathPosition = pathPositionBuffer[pathFollow.pathIndex];
                    float3 pathActualPos = PathfindingGridSetup.Instance.pathfindingGrid.GetWorldPosition((int)pathPosition.position.x, (int)pathPosition.position.y);

                    float3 targetPosition = new float3(pathActualPos.x, transform.translation.y, pathActualPos.z);
                    Debug.DrawLine(targetPosition, targetPosition + new float3(0,1,0), Color.green);

                    float3 moveDir = math.normalizesafe(targetPosition - transform.translation);
                    float moveSpeed = speed.Value * (1 - slow.Value) * (1 - petrifyAmt.Value);
                    Debug.DrawLine(transform.translation, targetPosition);

                    transform.translation += moveDir * moveSpeed * deltaTime;

                    float2 dirXZ = math.normalizesafe(new float2(moveDir.x, moveDir.z));
                    if (moveDir.x == 0) moveDir.x = 0.0001f;
                    bool isFront = Mathf.Acos(Vector2.Dot(new float2(0, 1),dirXZ))>0;
                    transform.angle = 90f + Mathf.Acos(Vector2.Dot(new float2(1, 0),dirXZ)) * Mathf.Rad2Deg;
                    if (isFront == false)
                        transform.angle *= -1f;
                    //Mathf.Atan(-moveDir.z / moveDir.x) * Mathf.Rad2Deg;

                    if (math.distance(transform.translation, targetPosition) < .1f)
                    {
                        // Next waypoint
                        pathFollow.pathIndex--;
                    }
                }
            }).WithoutBurst().Schedule(inputDeps);
    }

    private void ValidateGridPosition(ref int x, ref int y)
    {
        x = math.clamp(x, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetHeight() - 1);
    }

}

[UpdateAfter(typeof(EnemyPathFollowSystem))]
[DisableAutoCreation]
public class PathFollowGetNewPathSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int mapWidth = PathfindingGridSetup.Instance.pathfindingGrid.GetWidth();
        int mapHeight = PathfindingGridSetup.Instance.pathfindingGrid.GetHeight();
        float3 originPosition = PathfindingGridSetup.Instance.pathfindingGrid.GetWorldPosition(0,0);
        float cellSize = PathfindingGridSetup.Instance.pathfindingGrid.GetCellSize();

        EntityCommandBuffer.ParallelWriter entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        JobHandle jobHandle = Entities.WithNone<PathfindingParams>().ForEach((Entity entity, int entityInQueryIndex, in PathFollow pathFollow, in CustomTransform transform) =>
        {
            if (pathFollow.pathIndex == -1)
            {

                GetXY(transform.translation + new float3(1, 0, 1) * cellSize * +.5f, originPosition, cellSize, out int startX, out int startY);
                ValidateGridPosition(ref startX, ref startY, mapWidth, mapHeight);

                //Fixed target position to Castle
                int endX = 0;
                int endY = 0;

                entityCommandBuffer.AddComponent(entityInQueryIndex, entity, new PathfindingParams
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(endX, endY)
                });
            }
        }).Schedule(inputDeps);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }

    private static void ValidateGridPosition(ref int x, ref int y, int width, int height)
    {
        x = math.clamp(x, 0, width - 1);
        y = math.clamp(y, 0, height - 1);
    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y)
    {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }

}
