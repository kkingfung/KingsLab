using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;

public class TowerCheckTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        EntityManager entityManager = World.EntityManager;
        Entities.WithAll<PlayerTag>().ForEach((Entity unitEntity, ref Target target, ref Translation transform, ref WaitingTime wait, ref Radius radius) => 
        {
            if (entityManager.Exists(target.targetEntity))
            {
                Translation targetpos = entityManager.GetComponentData<Translation>(target.targetEntity);
                if (math.distancesq(transform.Value, targetpos.Value) > radius.Value * radius.Value)
                {
                    // far to target, destroy it
                    //PostUpdateCommands.DestroyEntity(hasTarget.targetEntity);
                    //PostUpdateCommands.RemoveComponent(unitEntity, typeof(Target));
                    entityManager.RemoveComponent<Target>(unitEntity);
                }
            }
            else {
                entityManager.RemoveComponent<Target>(unitEntity);
            }
        });
    }
}
