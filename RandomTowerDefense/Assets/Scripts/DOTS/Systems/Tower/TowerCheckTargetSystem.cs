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

            if (entityManager.Exists(target.targetEntity) && target.targetEntity != Entity.Null)
            {
                if (entityManager.HasComponent<EnemyTag>(target.targetEntity))
                {
                    Translation targetpos = entityManager.GetComponentData<Translation>(target.targetEntity);
                    Health targetHealth = entityManager.GetComponentData<Health>(target.targetEntity);
                    target.targetPos = targetpos.Value;
                    targetpos.Value.y = transform.Value.y;
                    target.targetHealth = targetHealth.Value;
                    if (CheckCollision(transform.Value, targetpos.Value, radius.Value * radius.Value)==false)
                    {
                        // far to target, destroy it
                        //PostUpdateCommands.DestroyEntity(hasTarget.targetEntity);
                        //PostUpdateCommands.RemoveComponent(unitEntity, typeof(Target));
                        target.targetEntity = Entity.Null;
                        target.targetHealth = 0;
                        entityManager.RemoveComponent<Target>(unitEntity);
                    }
                }
                else
                {
                    target.targetEntity = Entity.Null;
                    target.targetHealth = 0;
                    target.targetPos = new Vector3();
                    entityManager.RemoveComponent<Target>(unitEntity);
                }
            }
        });
    }

    //Common Function
    static private float GetDistance(float3 posA, float3 posB)
    {
        float3 delta = posA - posB;
        return delta.x * delta.x + delta.z * delta.z;
    }

    static private bool CheckCollision(float3 posA, float3 posB, float radiusSqr)
    {
        return GetDistance(posA, posB) <= radiusSqr;
    }
}
