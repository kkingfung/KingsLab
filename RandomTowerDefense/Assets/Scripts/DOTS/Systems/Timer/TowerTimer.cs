using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class TowerTimer : JobComponentSystem
{
	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{

		float deltaTime = Time.DeltaTime;

		return Entities.WithAll<PlayerTag>().ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait) =>
		{
			wait.Value -= deltaTime;
		}).Schedule(inputDeps);
	}
}
