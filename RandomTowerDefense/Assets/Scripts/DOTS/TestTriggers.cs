﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Mathematics;
using Unity.Burst;

public class TestTriggers : JobComponentSystem
{

    [BurstCompile]
    private struct TriggerJob : ITriggerEventsJob
    {

        public ComponentDataFromEntity<PhysicsVelocity> physicsVelocityEntities;

        public void Execute(TriggerEvent triggerEvent)
        {
            if (physicsVelocityEntities.HasComponent(triggerEvent.EntityA))
            {
                PhysicsVelocity physicsVelocity = physicsVelocityEntities[triggerEvent.EntityA];
                physicsVelocity.Linear.y = 5f;
                physicsVelocityEntities[triggerEvent.EntityA] = physicsVelocity;
            }

            if (physicsVelocityEntities.HasComponent(triggerEvent.EntityB))
            {
                PhysicsVelocity physicsVelocity = physicsVelocityEntities[triggerEvent.EntityB];
                physicsVelocity.Linear.y = 5f;
                physicsVelocityEntities[triggerEvent.EntityB] = physicsVelocity;
            }
        }

    }


    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        TriggerJob triggerJob = new TriggerJob
        {
            physicsVelocityEntities = GetComponentDataFromEntity<PhysicsVelocity>()
        };
        return triggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
    }

}
