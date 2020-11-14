using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class SkillTimer : JobComponentSystem
{
	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{

		float deltaTime = Time.DeltaTime;

		return Entities.WithAll<SkillTag>().ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait, ref ActionTime action) =>
		{
			wait.Value -= deltaTime;
		}).Schedule(inputDeps);
	}
}
