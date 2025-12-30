using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;

/// <summary>
/// 攻撃タイマーシステム - 攻撃エンティティのタイマー管理
///
/// 主な機能:
/// - 攻撃の待機時間（WaitingTime）をデルタタイムで減少
/// - 攻撃のアクション時間（ActionTime）をデルタタイムで減少
/// - AttackTagを持つエンティティのみを対象に処理
/// - Burst対応の並列ジョブで高性能なタイマー更新
/// - ECS Jobシステムによるマルチスレッド処理
/// </summary>
public class AttackTimer : JobComponentSystem
{
	/// <summary>
	/// 毎フレーム実行 - 全攻撃エンティティのタイマーを更新
	/// </summary>
	/// <param name="inputDeps">入力依存関係</param>
	/// <returns>ジョブハンドル</returns>
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
