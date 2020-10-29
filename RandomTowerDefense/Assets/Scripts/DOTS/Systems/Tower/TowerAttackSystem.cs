using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

public class TowerAttackSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        return Entities.WithAll<PlayerTag>().ForEach((Entity unitEntity, ref Target hasTarget, ref Translation translation) => {
            if (entityManager.Exists(hasTarget.targetEntity))
            {

            }
            else
            {

            }
        }).Schedule(inputDependencies);
    }
}

