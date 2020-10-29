using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public class SkillSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        JobHandle job;

        job = Entities.WithAll<BlizzardTag>().ForEach((Entity entity, ref ActiveTime activeTime) =>
        {
            //Move with Gyro

        }).Schedule(inputDeps);

        job = Entities.WithAll<MinionsTag>().ForEach((Entity entity, ref ActiveTime activeTime) =>
        {
            //FindTarget

        }).Schedule(inputDeps);

        return job;
    }
}

