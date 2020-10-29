using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class CollisionSystem : JobComponentSystem
{
	EntityQuery castleGroup;

	EntityQuery enemyGroup;
	//EntityQuery towerGroup;

	EntityQuery MeteorGroup;
	EntityQuery BlizzardGroup;
	EntityQuery PetrificationGroup;
	EntityQuery MinionsGroup;

	EntityQuery AttackGroup;
	protected override void OnCreate()
	{
		castleGroup = GetEntityQuery(typeof(Health), typeof(Area), 
			ComponentType.ReadOnly<Translation>(),ComponentType.ReadOnly<PlayerTag>());
		enemyGroup = GetEntityQuery(typeof(Health), typeof(Area), typeof(SlowRate), typeof(BuffTime),
			ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<EnemyTag>());

		MeteorGroup = GetEntityQuery(typeof(Area), typeof(Damage), typeof(WaitingFrame), typeof(CycleTime), 
			ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<MeteorTag>());
		PetrificationGroup = GetEntityQuery(typeof(Area), typeof(Damage), typeof(WaitingFrame), typeof(CycleTime),
			ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PetrificationTag>());
		MinionsGroup = GetEntityQuery(typeof(Area), typeof(Damage), typeof(WaitingFrame), typeof(CycleTime),
			ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<MinionsTag>());

		BlizzardGroup = GetEntityQuery(typeof(Area), typeof(Damage), typeof(WaitingFrame), typeof(CycleTime),
		ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<BlizzardTag>());

		AttackGroup = GetEntityQuery(typeof(Area), typeof(Damage), typeof(WaitingFrame), typeof(ActiveTime),
			ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<AttackTag>());
	}

	protected override JobHandle OnUpdate(JobHandle inputDependencies)
	{
		var translationType = GetComponentTypeHandle<Translation>(true);

		var healthType = GetComponentTypeHandle<Health>(false);
		var radiusType = GetComponentTypeHandle<Area>(true);

		var damageType = GetComponentTypeHandle<Damage>(true);

		var waitType = GetComponentTypeHandle<WaitingFrame>(true);
		var cycleType = GetComponentTypeHandle<CycleTime>(true);
		var activeType = GetComponentTypeHandle<ActiveTime>(false);

		var slowType = GetComponentTypeHandle<SlowRate>(false);
		var buffType = GetComponentTypeHandle<BuffTime>(false);

		JobHandle jobHandle;
		//enemy by castle
		var jobEvC = new CollisionJobEvC()
		{
			healthType = healthType,
			translationType = translationType,
			radius = castleGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			transToTestAgainst = castleGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
		};
		jobHandle = jobEvC.Schedule(enemyGroup, inputDependencies);

		//castle by enemy
		var jobCvE = new CollisionJobCvE()
		{
			healthType = healthType,
			translationType = translationType,
			radius = enemyGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			transToTestAgainst = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
		};
		jobHandle = jobCvE.Schedule(castleGroup, inputDependencies);

		//For GameOver
		//if (Settings.IsPlayerDead())
		//	return jobHandle;

		//enemy by Attack
		var jobEvA = new CollisionJobEvA()
		{
			radius = radiusType,
			healthType = healthType,
			translationType = translationType,

			targetRadius = AttackGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			targetDamage = AttackGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
			targetTrans = AttackGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
		};
		jobHandle = jobEvA.Schedule(enemyGroup, inputDependencies);

		//enemy by meteor
		var jobEvA = new CollisionJobEvA()
		{
			radius = radiusType,
			healthType = healthType,
			translationType = translationType,

			targetRadius = AttackGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			targetDamage = AttackGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
			targetTrans = AttackGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
		};
		jobHandle = jobEvA.Schedule(enemyGroup, inputDependencies);

		//enemy by blizzard
		var jobEvA = new CollisionJobEvA()
		{
			radius = radiusType,
			healthType = healthType,
			translationType = translationType,

			targetRadius = AttackGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			targetDamage = AttackGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
			targetTrans = AttackGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
		};
		jobHandle = jobEvA.Schedule(enemyGroup, inputDependencies);

		//enemy by blizzard
		var jobEvA = new CollisionJobEvA()
		{
			radius = radiusType,
			healthType = healthType,
			translationType = translationType,

			targetRadius = AttackGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			targetDamage = AttackGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
			targetTrans = AttackGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
		};
		jobHandle = jobEvA.Schedule(enemyGroup, inputDependencies);








		return jobPvE.Schedule(castleGroup, jobHandle);
	}

	//Common Function
	static bool CheckCollision(float3 posA, float3 posB, float radiusSqr)
	{
		float3 delta = posA - posB;
		float distanceSquare = delta.x * delta.x + delta.z * delta.z;

		return distanceSquare <= radiusSqr;
	}

	//Collision Job
	#region JobEvC
	[BurstCompile]
	struct CollisionJobEvC : IJobChunk
	{
		[ReadOnly] public ComponentTypeHandle<Area> radius;
		public ComponentTypeHandle<Health> healthType;
		[ReadOnly] public ComponentTypeHandle<Translation> translationType;

		[DeallocateOnJobCompletion]
		public NativeArray<Area> targetRadius;
		[DeallocateOnJobCompletion]
		public NativeArray<Damage> targetDamage;
		[DeallocateOnJobCompletion]
		public NativeArray<Translation> targetTrans;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkHealths = chunk.GetNativeArray(healthType);
			var chunkTranslations = chunk.GetNativeArray(translationType);
			var chunkRadius = chunk.GetNativeArray(radius);

			for (int i = 0; i < chunk.Count; i++)
			{
				float damage = 0f;
				Health health = chunkHealths[i];
				Area radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				Damage health = chunkHealths[i];
				Area radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				for (int j = 0; j < targetTrans.Length; j++)
				{
					Translation pos2 = targetTrans[j];

					if (CheckCollision(pos.Value, pos2.Value, radius.Value + targetRadius))
					{
						damage += 1;
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

	#region JobCvE
	[BurstCompile]
	struct CollisionJobCvE : IJobChunk
	{
		[ReadOnly] public ComponentTypeHandle<Area> radius;
		public ComponentTypeHandle<Health> healthType;
		[ReadOnly] public ComponentTypeHandle<Translation> translationType;

		[DeallocateOnJobCompletion]
		public NativeArray<Area> targetRadius;
		[DeallocateOnJobCompletion]
		public NativeArray<Damage> targetDamage;
		[DeallocateOnJobCompletion]
		public NativeArray<Translation> targetTrans;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkHealths = chunk.GetNativeArray(healthType);
			var chunkTranslations = chunk.GetNativeArray(translationType);
			var chunkRadius = chunk.GetNativeArray(radius);

			for (int i = 0; i < chunk.Count; i++)
			{
				float damage = 0f;
				Health health = chunkHealths[i];
				Area radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				Damage health = chunkHealths[i];
				Area radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				for (int j = 0; j < targetTrans.Length; j++)
				{
					Translation pos2 = targetTrans[j];

					if (CheckCollision(pos.Value, pos2.Value, radius.Value+ targetRadius))
					{
						damage += 1;
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

	#region JobAvE
	[BurstCompile]
	struct CollisionJobAvE : IJobChunk
	{
		[ReadOnly] public ComponentTypeHandle<Area> radius;
		public ComponentTypeHandle<Health> healthType;
		[ReadOnly] public ComponentTypeHandle<Translation> translationType;

		[DeallocateOnJobCompletion]
		public NativeArray<Translation> transToTestAgainst;


		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkHealths = chunk.GetNativeArray(healthType);
			var chunkTranslations = chunk.GetNativeArray(translationType);
			var chunkRadius = chunk.GetNativeArray(radius);

			for (int i = 0; i < chunk.Count; i++)
			{
				float damage = 0f;
				Health health = chunkHealths[i];
				Area radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				for (int j = 0; j < transToTestAgainst.Length; j++)
				{
					Translation pos2 = transToTestAgainst[j];

					if (CheckCollision(pos.Value, pos2.Value, radius.Value))
					{
						damage += 1;
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