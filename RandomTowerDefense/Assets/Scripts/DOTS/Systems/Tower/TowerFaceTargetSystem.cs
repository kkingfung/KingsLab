﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;

public class TowerFaceTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        EntityManager entityManager = World.EntityManager;
        Entities.WithAll<PlayerTag>().ForEach((Entity unitEntity, ref Target target, ref CustomTransform transform, ref WaitingTime wait, ref Radius radius) => 
        {
            if (entityManager.Exists(target.targetEntity))
            {
                if (math.distancesq(transform.translation, target.targetPos) > radius.Value * radius.Value)
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
