using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public class BlizzardSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        return Entities.WithAll<SkillTag>().ForEach((Entity entity, ref ActiveTime activeTime) =>
        {


        }).Schedule(inputDeps);
    }
}

