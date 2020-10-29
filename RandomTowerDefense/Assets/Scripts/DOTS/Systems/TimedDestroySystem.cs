using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class TimedDestroySystem : JobComponentSystem
{
	private EntityManager entityManager;
	private EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
	protected override void OnCreate()
	{
		entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		EntityCommandBuffer ecb = endSimulationEcbSystem.CreateCommandBuffer();
		EntityCommandBuffer.ParallelWriter ecbc = ecb.AsParallelWriter();
		
		float deltaTime = Time.DeltaTime;

		JobHandle job = Entities.WithAll<AttackTag>().ForEach((Entity entity, int entityInQueryIndex, ref ActionTime actionTime) =>
		{
			actionTime.Value -= deltaTime;
			if (actionTime.Value <= 0f)
				ecbc.DestroyEntity(entityInQueryIndex, entity);

		}).Schedule(inputDeps);

		job = Entities.WithAll<SkillTag>().ForEach((Entity entity, int entityInQueryIndex, ref ActionTime actionTime) =>
		{
			actionTime.Value -= deltaTime;
			if (actionTime.Value <= 0f)
				ecbc.DestroyEntity(entityInQueryIndex,entity);

		}).Schedule(inputDeps);

		job = Entities.WithAll<PlayerTag>().ForEach((Entity entity, int entityInQueryIndex, ref ActiveTime activeTime) =>
		{
			if (activeTime.Value <= 0f)
				ecbc.DestroyEntity(entityInQueryIndex, entity);

		}).Schedule(inputDeps);

		return job;
	}
}

