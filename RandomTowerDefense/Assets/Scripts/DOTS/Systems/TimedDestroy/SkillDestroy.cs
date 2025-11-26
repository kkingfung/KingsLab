using System.Security.Cryptography;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// スキルエンティティの生存時間管理と破棄を処理するシステム
/// 生存時間が終了したスキルエンティティからSkillTagを削除
/// </summary>
public class SkillDestroy : JobComponentSystem
{
	//private EntityManager entityManager;
	private EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
	protected override void OnCreate()
	{
		//entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	/// <summary>
	/// スキルエンティティの生存時間更新と破棄処理
	/// </summary>
	/// <param name="inputDeps">入力依存関係</param>
	/// <returns>ジョブハンドル</returns>
	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		EntityCommandBuffer ecb = endSimulationEcbSystem.CreateCommandBuffer();
		EntityCommandBuffer.ParallelWriter ecbc = ecb.AsParallelWriter();

		float deltaTime = Time.DeltaTime;

		JobHandle jobHandle = Entities.WithAll<SkillTag>().ForEach((Entity entity, int entityInQueryIndex, ref Lifetime activeTime) =>
		{
			activeTime.Value -= deltaTime;
			if (activeTime.Value <= 0f)
			{
				ecbc.RemoveComponent<SkillTag>(entityInQueryIndex, entity);
				//ecbc.DestroyEntity(entityInQueryIndex, entity);
			}
		}).Schedule(inputDeps);

		endSimulationEcbSystem.AddJobHandleForProducer(jobHandle);

		return jobHandle;
	}
}
