using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;

public class AttackUpdateSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        EntityManager entityManager = World.EntityManager;
        Entities.WithAll<AttackTag>().ForEach((Entity unitEntity, ref Translation transform, ref Velocity velocity) =>
        {
            transform.Value += velocity.Value*Time.DeltaTime;
            Debug.DrawLine(transform.Value, transform.Value+new float3(0,1,0),Color.magenta);
        });
    }
}
