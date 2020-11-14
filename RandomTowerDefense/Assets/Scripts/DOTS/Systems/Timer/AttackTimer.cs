using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class AttackTimer : JobComponentSystem
{
	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{

		float deltaTime = Time.DeltaTime;

		return Entities.WithAll<AttackTag>().ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait, ref ActionTime action) =>
		{
			if (action.Value > 0)
				action.Value -= deltaTime;
			wait.Value -= deltaTime;
		}).Schedule(inputDeps);
	}
}
