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

        return Entities.WithAll<EnemyTag>().ForEach((Entity entity, ref SlowRate slowRate, ref PetrifyAmt petrifyAmt, ref BuffTime buffTime) =>
        {
            if (buffTime.Value > 0) {
                buffTime.Value -= deltaTime;
            }
            else
            {
                if (slowRate.Value > 0) {
                    slowRate.Value = Mathf.Max(slowRate.Value - 0.2f * deltaTime, 0f); 
                }
                if (petrifyAmt.Value > 0) { 
                    petrifyAmt.Value = Mathf.Max(petrifyAmt.Value - 0.2f * deltaTime, 0f);
                }
            }

        }).Schedule(inputDeps);
    }
}
