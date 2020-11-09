using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

[DisableAutoCreation]
public class EnemyPathFollowSystem : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;

        return Entities.WithAll<EnemyTag>().ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuffer,
            ref Translation translation, ref Health health, ref Speed speed, ref SlowRate slow, ref PetrifyAmt petrifyAmt, 
            ref PathFollow pathFollow) => {
            if (health.Value > 0 && pathFollow.pathIndex >= 0) {
                // Has path to follow
                PathPosition pathPosition = pathPositionBuffer[pathFollow.pathIndex];
                float3 targetPosition = new float3(pathPosition.position.x, translation.Value.y, pathPosition.position.y);
                float3 moveDir = math.normalizesafe(targetPosition - translation.Value);
                float moveSpeed = speed.Value*(1-slow.Value)*(1- petrifyAmt.Value);

                translation.Value += moveDir * moveSpeed * deltaTime;
                
                if (math.distance(translation.Value, targetPosition) < .1f) {
                    // Next waypoint
                    pathFollow.pathIndex--;
                }
            }
        }).Schedule(inputDeps);
    }

    private void ValidateGridPosition(ref int x, ref int y) {
        x = math.clamp(x, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetHeight() - 1);
    }

}

[UpdateAfter(typeof(EnemyPathFollowSystem))]
[DisableAutoCreation]
public class PathFollowGetNewPathSystem : JobComponentSystem {
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate() {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        int mapWidth = PathfindingGridSetup.Instance.pathfindingGrid.GetWidth();
        int mapHeight = PathfindingGridSetup.Instance.pathfindingGrid.GetHeight();
        float3 originPosition = float3.zero;
        float cellSize = PathfindingGridSetup.Instance.pathfindingGrid.GetCellSize();
        
        EntityCommandBuffer.ParallelWriter entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        JobHandle jobHandle = Entities.WithNone<PathfindingParams>().ForEach((Entity entity, int entityInQueryIndex, in PathFollow pathFollow, in Translation translation) => { 
            if (pathFollow.pathIndex == -1) {
                
                GetXY(translation.Value + new float3(1, 0, 1) * cellSize * +.5f, originPosition, cellSize, out int startX, out int startY);

                ValidateGridPosition(ref startX, ref startY, mapWidth, mapHeight);

                //Fixed target position to Castle
                int endX = 0;
                int endY = 0;

                entityCommandBuffer.AddComponent(entityInQueryIndex, entity, new PathfindingParams { 
                    startPosition = new int2(startX, startY), endPosition = new int2(endX, endY) 
                });
            }
        }).Schedule(inputDeps);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }

    private static void ValidateGridPosition(ref int x, ref int y, int width, int height) {
        x = math.clamp(x, 0, width - 1);
        y = math.clamp(y, 0, height - 1);
    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y) {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }

}
