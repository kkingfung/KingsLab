using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class TimerSystem : JobComponentSystem
{
	private EntityManager entityManager;
	protected override void OnCreate()
	{
		entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{

		float deltaTime = Time.DeltaTime;

		JobHandle job = Entities.WithAll<AttackTag>().ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait, ref ActionTime action) =>
		{
			if (action.Value > 0) action.Value -= deltaTime;
			wait.Value -= deltaTime;
		}).Schedule(inputDeps);

		job = Entities.WithAll<SkillTag>().ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait, ref ActionTime action) =>
		{
			if (action.Value > 0) action.Value -= deltaTime;
			if (wait.Value < 0) wait.Value = 0.2f;
			wait.Value -= deltaTime;
		}).Schedule(inputDeps);

		job = Entities.WithAll<PlayerTag>().ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait) =>
		{
			wait.Value -= deltaTime;
		}).Schedule(inputDeps);

		return job;
	}
}
