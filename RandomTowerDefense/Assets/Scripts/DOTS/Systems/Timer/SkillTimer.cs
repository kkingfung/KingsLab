using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;

/// <summary>
/// スキルのタイマーを管理するシステム
/// </summary>
public class SkillTimer : JobComponentSystem
{
	/// <summary>
	/// システムの更新処理
	/// </summary>
	/// <param name="inputDeps">入力情報のジョブハンドル</param>
	/// <returns>ハンドル</returns>
	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
		float deltaTime = Time.DeltaTime;

		return Entities.WithAll<SkillTag>().ForEach((Entity entity, int entityInQueryIndex,
			ref WaitingTime wait, ref ActionTime action) =>
		{
			wait.Value -= deltaTime;
		}).Schedule(inputDeps);
	}
}
