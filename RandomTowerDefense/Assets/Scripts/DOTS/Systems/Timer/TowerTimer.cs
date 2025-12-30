using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;

/// <summary>
/// タワータイマーシステム - タワーエンティティの攻撃クールダウンタイマー管理
///
/// 主な機能:
/// - タワーの待機時間（WaitingTime）をデルタタイムで減少
/// - PlayerTagを持つエンティティのみを対象に処理
/// - Burst対応の並列ジョブで高性能なタイマー更新
/// - ECS Jobシステムによるマルチスレッド処理
/// </summary>
public class TowerTimer : JobComponentSystem
{
	/// <summary>
	/// 毎フレーム実行 - 全タワーエンティティの待機時間を更新
	/// </summary>
	/// <param name="inputDeps">入力依存関係</param>
	/// <returns>ジョブハンドル</returns>
	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{

		float deltaTime = Time.DeltaTime;

		return Entities.WithAll<PlayerTag>().ForEach((Entity entity, int entityInQueryIndex, ref WaitingTime wait) =>
		{
			wait.Value -= deltaTime;
		}).Schedule(inputDeps);
	}
}
