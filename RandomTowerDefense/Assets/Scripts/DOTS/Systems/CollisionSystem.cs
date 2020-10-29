using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
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
		enemyGroup = GetEntityQuery(typeof(Health), typeof(Area), typeof(Damage), typeof(SlowRate), typeof(BuffTime),
			ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<EnemyTag>());

		MeteorGroup = GetEntityQuery(typeof(Area), typeof(Damage),  typeof(ActionTime),
			ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<MeteorTag>());
		PetrificationGroup = GetEntityQuery(typeof(Area), typeof(Damage), typeof(WaitingFrame), typeof(ActionTime),
			ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PetrificationTag>());
		MinionsGroup = GetEntityQuery(typeof(Area), typeof(Damage),  typeof(ActionTime),
			ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<MinionsTag>());

		BlizzardGroup = GetEntityQuery(typeof(Area), typeof(Damage), typeof(WaitingFrame), typeof(ActionTime),
		ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<BlizzardTag>());

		AttackGroup = GetEntityQuery(typeof(Area), typeof(Damage),typeof(ActionTime),
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
		var activeType = GetComponentTypeHandle<ActiveTime>(true);
		var actionType = GetComponentTypeHandle<ActionTime>(true);

		var slowType = GetComponentTypeHandle<SlowRate>(false);
		var buffType = GetComponentTypeHandle<BuffTime>(false);

		JobHandle jobHandle;
		//enemy by castle
		var jobEvC = new CollisionJobEvC()
		{
			healthType = healthType,
			translationType = translationType,
			radius = radiusType,
			targetRadius = castleGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			targetTrans = castleGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
		};
		jobHandle = jobEvC.Schedule(enemyGroup, inputDependencies);

		//castle by enemy
		var jobCvE = new CollisionJobCvE()
		{
			healthType = healthType,
			translationType = translationType,
			radius = radiusType,
			targetDamage = enemyGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
			targetRadius = enemyGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			targetTrans = enemyGroup.ToComponentDataArray<Translation>(Allocator.TempJob)
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
			targetTrans = AttackGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
			targetAction = AttackGroup.ToComponentDataArray<ActionTime>(Allocator.TempJob)
		};
		jobHandle = jobEvA.Schedule(enemyGroup, inputDependencies);

		//enemy by meteor
		var JobEvSM1 = new CollisionJobEvSM()
		{
			radius = radiusType,
			healthType = healthType,
			translationType = translationType,

			targetRadius = MeteorGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			targetDamage = MeteorGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
			targetTrans = MeteorGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
			targetAction = MeteorGroup.ToComponentDataArray<ActionTime>(Allocator.TempJob)
		};
		jobHandle = JobEvSM1.Schedule(enemyGroup, inputDependencies);

		//enemy by minions
		var JobEvSM2 = new CollisionJobEvSM()
		{
			radius = radiusType,
			healthType = healthType,
			translationType = translationType,

			targetRadius = MinionsGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			targetDamage = MinionsGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
			targetTrans = MinionsGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
			targetAction = MinionsGroup.ToComponentDataArray<ActionTime>(Allocator.TempJob)
		};
		jobHandle = JobEvSM2.Schedule(enemyGroup, inputDependencies);

		//enemy by blizzard
		var jobEvSB = new CollisionJobEvSB()
		{
			radius = radiusType,
			healthType = healthType,
			translationType = translationType,

			targetRadius = BlizzardGroup.ToComponentDataArray<Area>(Allocator.TempJob),
			targetDamage = BlizzardGroup.ToComponentDataArray<Damage>(Allocator.TempJob),
			targetTrans = BlizzardGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
			targetAction = BlizzardGroup.ToComponentDataArray<ActionTime>(Allocator.TempJob),

			slowType = slowType,
			buffType = buffType,
			targetBuff = BlizzardGroup.ToComponentDataArray<WaitingFrame>(Allocator.TempJob)
		};
		jobHandle = jobEvSB.Schedule(enemyGroup, inputDependencies);

		//enemy by petrification
		var jobEvSP = new CollisionJobEvSP()
		{
			slowType = slowType,
			buffType = buffType,
			targetAction = PetrificationGroup.ToComponentDataArray<ActionTime>(Allocator.TempJob),
			targetBuff = PetrificationGroup.ToComponentDataArray<BuffTime>(Allocator.TempJob)
		};
		jobHandle = jobEvSP.Schedule(enemyGroup, inputDependencies);

		return jobHandle;
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
				Area radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				for (int j = 0; j < targetTrans.Length; j++)
				{
					Translation pos2 = targetTrans[j];

					if (CheckCollision(pos.Value, pos2.Value, targetRadius[j].Value + radius.Value))
					{
						damage += 1;
					}
				}

				if (damage > 0)
				{
					health.Value =0;
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
		public NativeArray<Damage> targetDamage;
		[DeallocateOnJobCompletion]
		public NativeArray<Area> targetRadius;
		[DeallocateOnJobCompletion]
		public NativeArray<Translation> targetTrans;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkHealths = chunk.GetNativeArray(healthType);
			var chunkTranslations = chunk.GetNativeArray(translationType);
			var chunkRadius = chunk.GetNativeArray(radius);

			for (int i = 0; i < chunk.Count; i++)
			{
				float damage = 0;
				Health health = chunkHealths[i];
				Area radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				for (int j = 0; j < targetTrans.Length; j++)
				{
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

	#region JobEvA
	[BurstCompile]
	struct CollisionJobEvA : IJobChunk
	{
		[ReadOnly] public ComponentTypeHandle<Area> radius;
		public ComponentTypeHandle<Health> healthType;
		[ReadOnly] public ComponentTypeHandle<Translation> translationType;

		[DeallocateOnJobCompletion]
		public NativeArray<Damage> targetDamage;
		[DeallocateOnJobCompletion]
		public NativeArray<Area> targetRadius;
		[DeallocateOnJobCompletion]
		public NativeArray<Translation> targetTrans;
		[DeallocateOnJobCompletion]
		public NativeArray<ActionTime> targetAction;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkHealths = chunk.GetNativeArray(healthType);
			var chunkTranslations = chunk.GetNativeArray(translationType);
			var chunkRadius = chunk.GetNativeArray(radius);

			for (int i = 0; i < chunk.Count; i++)
			{
				float damage = 0;
				Health health = chunkHealths[i];
				Area radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				for (int j = 0; j < targetTrans.Length; j++)
				{
					if (targetAction[j].Value <= 0) continue;

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

	//enemy by meteor/minions
	#region JobEvSM
	[BurstCompile]
	struct CollisionJobEvSM : IJobChunk
	{

		[ReadOnly] public ComponentTypeHandle<Area> radius;
		public ComponentTypeHandle<Health> healthType;
		[ReadOnly] public ComponentTypeHandle<Translation> translationType;

		[DeallocateOnJobCompletion]
		public NativeArray<Damage> targetDamage;
		[DeallocateOnJobCompletion]
		public NativeArray<Area> targetRadius;
		[DeallocateOnJobCompletion]
		public NativeArray<Translation> targetTrans;
		[DeallocateOnJobCompletion]
		public NativeArray<ActionTime> targetAction;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkHealths = chunk.GetNativeArray(healthType);
			var chunkTranslations = chunk.GetNativeArray(translationType);
			var chunkRadius = chunk.GetNativeArray(radius);

			for (int i = 0; i < chunk.Count; i++)
			{
				float damage = 0;
				Health health = chunkHealths[i];
				Area radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];

				for (int j = 0; j < targetTrans.Length; j++)
				{
					if (targetAction[j].Value <= 0) continue;

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

	//enemy by blizzard
	#region JobEvSB
	[BurstCompile]
	struct CollisionJobEvSB : IJobChunk
	{

		[ReadOnly] public ComponentTypeHandle<Area> radius;
		public ComponentTypeHandle<Health> healthType;
		[ReadOnly] public ComponentTypeHandle<Translation> translationType;
		public ComponentTypeHandle<SlowRate> slowType;
		public ComponentTypeHandle<BuffTime> buffType;

		[DeallocateOnJobCompletion]
		public NativeArray<Damage> targetDamage;
		[DeallocateOnJobCompletion]
		public NativeArray<Area> targetRadius;
		[DeallocateOnJobCompletion]
		public NativeArray<Translation> targetTrans;
		[DeallocateOnJobCompletion]
		public NativeArray<ActionTime> targetAction;
		[DeallocateOnJobCompletion]
		public NativeArray<WaitingFrame> targetBuff;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkHealths = chunk.GetNativeArray(healthType);
			var chunkTranslations = chunk.GetNativeArray(translationType);
			var chunkRadius = chunk.GetNativeArray(radius);

			var chunkSlow = chunk.GetNativeArray(slowType);
			var chunkBuff = chunk.GetNativeArray(buffType);

			for (int i = 0; i < chunk.Count; i++)
			{
				float damage = 0;
				Health health = chunkHealths[i];
				Area radius = chunkRadius[i];
				Translation pos = chunkTranslations[i];
				SlowRate slow = chunkSlow[i];
				BuffTime buff = chunkBuff[i];

				for (int j = 0; j < targetTrans.Length; j++)
				{
					if (targetAction[j].Value <= 0) continue;

					Translation pos2 = targetTrans[j];

					if (CheckCollision(pos.Value, pos2.Value, targetRadius[j].Value + radius.Value))
					{
						damage += targetDamage[j].Value;
						if (slow.Value > 0) slow.Value *= 0.95f;
						if (buff.Value < targetBuff[j].Value) buff.Value = targetBuff[j].Value;
					}
				}

				if (damage > 0)
				{
					health.Value -= damage;
					chunkHealths[i] = health;
					chunkSlow[i] = slow;
					chunkBuff[i] = buff;
				}
			}
		}
	}
	#endregion

	//enemy by petrification
	#region JobEvSP
	[BurstCompile]
	struct CollisionJobEvSP : IJobChunk
	{
		public ComponentTypeHandle<SlowRate> slowType;
		public ComponentTypeHandle<BuffTime> buffType;

		[DeallocateOnJobCompletion]
		public NativeArray<ActionTime> targetAction;
		[DeallocateOnJobCompletion]
		public NativeArray<BuffTime> targetBuff;

		public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
		{
			var chunkSlow = chunk.GetNativeArray(slowType);
			var chunkBuff = chunk.GetNativeArray(buffType);

			for (int i = 0; i < chunk.Count; i++)
			{
				SlowRate slow = chunkSlow[i];
				BuffTime buff = chunkBuff[i];

				for (int j = 0; j < targetAction.Length; j++)
				{
					if (targetAction[j].Value <= 0) continue;
						slow.Value = 0;
						buff.Value = targetBuff[j].Value;
					
				}
				chunkSlow[i] = slow;
				chunkBuff[i] = buff;
			}
		}
	}
	#endregion

}