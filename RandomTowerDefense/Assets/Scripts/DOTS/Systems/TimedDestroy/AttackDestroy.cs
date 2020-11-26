using System.Security.Cryptography;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class AttackDestroy : JobComponentSystem
{
	//private EntityManager entityManager;
	private EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
	protected override void OnCreate()
	{
		//entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		EntityCommandBuffer ecb = endSimulationEcbSystem.CreateCommandBuffer();
		EntityCommandBuffer.ParallelWriter ecbc = ecb.AsParallelWriter();

		float deltaTime = Time.DeltaTime;

		JobHandle jobHandle = Entities.WithAll<AttackTag>().ForEach((Entity entity, int entityInQueryIndex, ref Lifetime activeTime) =>
		{
			activeTime.Value -= deltaTime;
			if (activeTime.Value <= 0f)
			{
				ecbc.RemoveComponent<AttackTag>(entityInQueryIndex, entity);
				//ecbc.DestroyEntity(entityInQueryIndex, entity);
			}

		}).Schedule(inputDeps);
		endSimulationEcbSystem.AddJobHandleForProducer(jobHandle);
		return jobHandle;
	}
}
