using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class RemoveDeadSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
    protected override void OnCreate()
    {
        endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }


	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		EntityManager entityManager = EntityManager;
		EntityCommandBuffer ecb = endSimulationEcbSystem.CreateCommandBuffer();
		EntityCommandBuffer.ParallelWriter ecbc = ecb.AsParallelWriter();

		Entities.WithAll<EnemyTag>().ForEach((Entity entity, int entityInQueryIndex, ref Health health) =>
		{
			if (health.Value <= 0)
			{
				ecbc.RemoveComponent<EnemyTag>(entityInQueryIndex, entity);
				//ecbc.DestroyEntity(entityInQueryIndex, entity);
			}
		}).Run();

		return default;
	}
}
