using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

public struct UnitSelected : IComponentData
{
}
public class UnityControl : ComponentSystem
{
    private Vector3 startPosition;

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Mouse Pressed
            startPosition =Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            // Mouse Held Down
            float3 selectionAreaSize = Input.mousePosition - startPosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Mouse Released
            float3 endPosition = Input.mousePosition;

            float3 lowerLeftPosition = new float3(math.min(startPosition.x, endPosition.x), math.min(startPosition.y, endPosition.y), 0);
            float3 upperRightPosition = new float3(math.max(startPosition.x, endPosition.x), math.max(startPosition.y, endPosition.y), 0);

            bool selectOnlyOneEntity = false;
            float selectionAreaMinSize = 10f;
            float selectionAreaSize = math.distance(lowerLeftPosition, upperRightPosition);
            if (selectionAreaSize < selectionAreaMinSize)
            {
                // Selection area too small
                lowerLeftPosition += new float3(-1, -1, 0) * (selectionAreaMinSize - selectionAreaSize) * .5f;
                upperRightPosition += new float3(+1, +1, 0) * (selectionAreaMinSize - selectionAreaSize) * .5f;
                selectOnlyOneEntity = true;
            }

            // Deselect all selected Entities
            Entities.WithAll<UnitSelected>().ForEach((Entity entity) => {
                PostUpdateCommands.RemoveComponent<UnitSelected>(entity);
            });

            // Select Entities inside selection area
            int selectedEntityCount = 0;
            Entities.ForEach((Entity entity, ref Translation translation) => {
                if (selectOnlyOneEntity == false || selectedEntityCount < 1)
                {
                    float3 entityPosition = translation.Value;
                    if (entityPosition.x >= lowerLeftPosition.x &&
                        entityPosition.y >= lowerLeftPosition.y &&
                        entityPosition.x <= upperRightPosition.x &&
                        entityPosition.y <= upperRightPosition.y)
                    {
                        // Entity inside selection area
                        PostUpdateCommands.AddComponent(entity, new UnitSelected());
                        selectedEntityCount++;
                    }
                }
            });
        }

        if (Input.GetMouseButtonDown(1))
        {
            // Right mouse button down
            float3 targetPosition = Input.mousePosition;
            List<float3> movePositionList = GetPositionListAround(targetPosition, new float[] { 10f, 20f, 30f }, new int[] { 5, 10, 20 });
            int positionIndex = 0;
            Entities.WithAll<UnitSelected>().ForEach((Entity entity, ref MoveTo moveTo) => {
                moveTo.position = movePositionList[positionIndex];
                positionIndex = (positionIndex + 1) % movePositionList.Count;
                moveTo.move = true;
            });
        }
    }

    private List<float3> GetPositionListAround(float3 startPosition, float[] ringDistance, int[] ringPositionCount)
    {
        List<float3> positionList = new List<float3>();
        positionList.Add(startPosition);
        for (int ring = 0; ring < ringPositionCount.Length; ring++)
        {
            List<float3> ringPositionList = GetPositionListAround(startPosition, ringDistance[ring], ringPositionCount[ring]);
            positionList.AddRange(ringPositionList);
        }
        return positionList;
    }

    private List<float3> GetPositionListAround(float3 startPosition, float distance, int positionCount)
    {
        List<float3> positionList = new List<float3>();
        for (int i = 0; i < positionCount; i++)
        {
            int angle = i * (360 / positionCount);
            float3 dir = ApplyRotationToVector(new float3(0, 1, 0), angle);
            float3 position = startPosition + dir * distance;
            positionList.Add(position);
        }
        return positionList;
    }

    private float3 ApplyRotationToVector(float3 vec, float angle)
    {
        return Quaternion.Euler(0, 0, angle) * vec;
    }

}

public class UnitSelectedRenderer : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<UnitSelected>().ForEach((ref Translation translation) => {
            float3 position = translation.Value + new float3(0, -3f, +5f);
            //Graphics.DrawMesh();
        });
    }

}

public struct MoveTo : IComponentData
{
    public bool move;
    public float3 position;
    public float3 lastMoveDir;
    public float moveSpeed;
}


// Unit go to Move Position
public class UnitMoveSystem : JobComponentSystem
{

    private struct Job : IJobForEachWithEntity<MoveTo, Translation>
    {

        public float deltaTime;

        public void Execute(Entity entity, int index, ref MoveTo moveTo, ref Translation translation)
        {
            if (moveTo.move)
            {
                float reachedPositionDistance = 1f;
                if (math.distance(translation.Value, moveTo.position) > reachedPositionDistance)
                {
                    // Far from target position, Move to position
                    float3 moveDir = math.normalize(moveTo.position - translation.Value);
                    moveTo.lastMoveDir = moveDir;
                    translation.Value += moveDir * moveTo.moveSpeed * deltaTime;
                }
                else
                {
                    // Already there
                    moveTo.move = false;
                }
            }
        }

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Job job = new Job
        {
            deltaTime = Time.DeltaTime,
        };
        return job.Schedule(this, inputDeps);
    }

}