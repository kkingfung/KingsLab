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
        Entities.WithAll<PlayerTag>().ForEach((Entity unitEntity, ref Target hasTarget, ref Translation translation, ref Rotation rotation, ref WaitingTime wait, ref Radius radius) => {
            if (World.DefaultGameObjectInjectionWorld.EntityManager.Exists(hasTarget.targetEntity))
            {
                if (wait.Value <= 0)
                {
                    Translation targetTranslation = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Translation>(hasTarget.targetEntity);

                    rotation.Value = Quaternion.Euler(0, 90f + Mathf.Rad2Deg * Mathf.Atan2(translation.Value.z - targetTranslation.Value.z,
                        targetTranslation.Value.x - translation.Value.x), 0);

                    if (math.distance(translation.Value, targetTranslation.Value) > radius.Value)
                    {
                        // far to target, destroy it
                        //PostUpdateCommands.DestroyEntity(hasTarget.targetEntity);
                        PostUpdateCommands.RemoveComponent(unitEntity, typeof(Target));
                    }
                }
            }
            else
            {
                // Target Entity already destroyed
                PostUpdateCommands.RemoveComponent(unitEntity, typeof(Target));
            }
        });
    }
}
