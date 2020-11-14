using Unity;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;


public class CastleToEnemy : JobComponentSystem
{
    EntityQuery castleGroup;

    EntityQuery enemyGroup;

    protected override void OnCreate()
    {
    }

	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		castleGroup = GetEntityQuery(typeof(Health), typeof(Radius),
		ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<CastleTag>());

		enemyGroup = GetEntityQuery(typeof(Health), typeof(Radius), typeof(Damage),
			ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<EnemyTag>());

		var transformType = GetComponentTypeHandle<Translation>(true);
		var healthType = GetComponentTypeHandle<Health>(false);
		var radiusType = GetComponentTypeHandle<Radius>(true);

		JobHandle jobHandle = inputDependencies;

		if (enemyGroup.CalculateEntityCount() > 0)
		{
			//castle by enemy
			var jobCvE = new CollisionJobCvE()
			{
				healthType = healthType,
				translationType = transformType,
				radius = radiusType,
				targetDamage = enemyGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
				targetRadius = enemyGroup.ToComponentDataArray<Radius>(Allocator.TempJob),
				targetTrans = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
				targetHealth = enemyGroup.ToComponentDataArray<Health>(Allocator.TempJob)
			};
			jobHandle = jobCvE.Schedule(castleGroup, inputDependencies);
			jobHandle.Complete();

			//enemy by castle

			var jobEvC = new CollisionJobEvC()
			{
				healthType = healthType,
				translationType = transformType,
				radius = radiusType,
				targetRadius = castleGroup.ToComponentDataArray<Radius>(Allocator.TempJob),
				targetTrans = castleGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
			};
			jobHandle = jobEvC.Schedule(enemyGroup, inputDependencies);
			jobHandle.Complete();
		}

		//For GameOver
		//if (Settings.IsPlayerDead())
		//	return jobHandle;
		return jobHandle;
	}

	//Common Function
	static float GetDistance(float3 posA, float3 posB)
	{
		float3 delta = posA - posB;
		return delta.x * delta.x + delta.z * delta.z;
	}

	static bool CheckCollision(float3 posA, float3 posB, float radiusSqr)
	{
		return GetDistance(posA, posB) <= radiusSqr;
	}

	//Collision Job
	#region JobEvC
	[BurstCompile]
	struct CollisionJobEvC : IJobChunk
	{
		[ReadOnly] public ComponentTypeHandle<Radius> radius;
		public ComponentTypeHandle<Health> healthType;
		[ReadOnly] public ComponentTypeHandle<Translation> translationType;

		[DeallocateOnJobCompletion]
		public NativeArray<Radius> targetRadius;
		[DeallocateOnJobCompletion]
		public NativeArray<Translation> targetTrans;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkHealths = chunk.GetNativeArray(healthType);
			var chunkTranslations = chunk.GetNativeArray(translationType);
			var chunkRadius = chunk.GetNativeArray(radius);

			for (int i = 0; i < chunk.Count; i++)
			{
				int damage = 0;
				Health health = chunkHealths[i];
				if (health.Value <= 0) continue;
				Radius radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				for (int j = 0; j < targetTrans.Length && damage <= 0; j++)
				{
					Translation pos2 = targetTrans[j];

					if (CheckCollision(pos.Value, pos2.Value, targetRadius[j].Value + radius.Value))
					{
						damage += 1;
					}

				}

				if (damage > 0)
				{
					health.Value = 0;
					chunkHealths[i] = health;
				}
			}
		}
	}
	#endregion

	#region JobCvE
	[BurstCompile]
	struct CollisionJobCvE : IJobChunk
	{
		[ReadOnly] public ComponentTypeHandle<Radius> radius;
		public ComponentTypeHandle<Health> healthType;
		[ReadOnly] public ComponentTypeHandle<Translation> translationType;

		[DeallocateOnJobCompletion]
		public NativeArray<Damage> targetDamage;
		[DeallocateOnJobCompletion]
		public NativeArray<Radius> targetRadius;
		[DeallocateOnJobCompletion]
		public NativeArray<Translation> targetTrans;
		[DeallocateOnJobCompletion]
		public NativeArray<Health> targetHealth;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkHealths = chunk.GetNativeArray(healthType);
			var chunkTranslations = chunk.GetNativeArray(translationType);
			var chunkRadius = chunk.GetNativeArray(radius);

			for (int i = 0; i < chunk.Count; i++)
			{
				float damage = 0;
				Health health = chunkHealths[i];
				if (health.Value <= 0) continue;
				Radius radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				for (int j = 0; j < targetTrans.Length; j++)
				{
					if (targetHealth[j].Value <= 0) continue;
					Translation pos2 = targetTrans[j];

					if (CheckCollision(pos.Value, pos2.Value, targetRadius[j].Value + radius.Value))
					{
						damage += targetDamage[j].Value;
					}
				}

				if (damage > 0)
				{
					health.Value -= damage;
					chunkHealths[i] = health;
				}
			}
		}
	}
	#endregion
}
