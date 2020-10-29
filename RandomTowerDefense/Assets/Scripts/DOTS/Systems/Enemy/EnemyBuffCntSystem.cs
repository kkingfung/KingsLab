using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public class EnemyBuffCntSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        return Entities.WithAll<EnemyTag>().ForEach((Entity entity, ref SlowRate slowRate, ref BuffTime buffTime) =>
        {
            if(buffTime.Value > 0) buffTime.Value -= deltaTime;

            if (slowRate.Value < 1) slowRate.Value += 0.2f* deltaTime;

        }).Schedule(inputDeps);
    }
}
