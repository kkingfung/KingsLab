using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;

public class TowerFaceTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<PlayerTag>().ForEach((Entity unitEntity, ref TargetFound hasTarget, ref TargetPos targetPos, ref CustomTransform transform, ref WaitingTime wait, ref Radius radius) => {
            if (hasTarget.Value)
            {
                if (wait.Value <= 0)
                {
                    float3 moveDir = math.normalizesafe(targetPos.Value - transform.translation);

                    float2 dirXZ = math.normalizesafe(new float2(moveDir.x, moveDir.z));
                    if (moveDir.x == 0) moveDir.x = 0.0001f;
                    bool isFront = Mathf.Acos(Vector2.Dot(new float2(0, 1), dirXZ)) > 0;
                    transform.angle = 90f+Mathf.Acos(Vector2.Dot(new float2(1, 0), dirXZ)) * Mathf.Rad2Deg;
                    if (isFront == false)
                        transform.angle *= -1f;
                    //Mathf.Atan(-moveDir.z / moveDir.x) * Mathf.Rad2Deg;

                    if (math.distancesq(transform.translation, targetPos.Value) > radius.Value*radius.Value)
                    {
                        // far to target, destroy it
                        //PostUpdateCommands.DestroyEntity(hasTarget.targetEntity);
                        //PostUpdateCommands.RemoveComponent(unitEntity, typeof(Target));
                        hasTarget.Value = false;
                    }
                }
            }
        });
    }
}
