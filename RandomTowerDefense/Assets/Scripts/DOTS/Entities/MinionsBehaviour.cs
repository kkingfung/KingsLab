using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

public class MinionsBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
	private readonly int TotalNum = 1000;
	public static MinionsBehaviour Instance { private set; get; }

	public static NativeArray<float> skWait;
	public static NativeArray<float> skActive;
	public static NativeArray<float> skCycle;
	public static NativeArray<float> skArea;
	public static NativeArray<float> skDamage;

	public static NativeArray<float> skExp;
	public static NativeArray<int> skLevel;

	public static NativeArray<int> skId;

	public static NativeArray<float3> skTargetPos;
	public static NativeArray<int> skTargetID;

	private void Awake()
	{
		Instance = this;

		skWait = new NativeArray<float>(TotalNum, Allocator.Persistent);
		skActive = new NativeArray<float>(TotalNum, Allocator.Persistent);
		skCycle = new NativeArray<float>(TotalNum, Allocator.Persistent);

		skExp = new NativeArray<float>(TotalNum, Allocator.Persistent);
		skLevel = new NativeArray<int>(TotalNum, Allocator.Persistent);

		skId = new NativeArray<int>(TotalNum, Allocator.Persistent);
		for (int i = 0; i < skId.Length; ++i)
			skId[i] = i;

		skTargetPos = new NativeArray<float3>(TotalNum, Allocator.Persistent);
		skTargetID = new NativeArray<int>(TotalNum, Allocator.Persistent);
	}

	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		var archeType = manager.CreateArchetype(typeof(Hybrid), typeof(SkillTag), typeof(MinionsTag),
			typeof(ActiveTime), typeof(WaitingFrame), typeof(CycleTime), typeof(Area),
			typeof(Damage), typeof(Exp), typeof(Level), typeof(Target), typeof(ObjID));
		var instance = new NativeArray<Entity>(TotalNum, Allocator.Temp);
		manager.CreateEntity(archeType, instance);
		instance.Dispose();
	}

	public static void ChgData(int id, float damage, float area, float active,
		float wait, float cycle, float exp, int level, float3 targetpos, int targetid)
	{
		EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
		manager.CompleteAllJobs();
		MinionsBehaviour.skWait[id] = wait;
		MinionsBehaviour.skActive[id] = active;
		MinionsBehaviour.skCycle[id] = cycle;
		MinionsBehaviour.skArea[id] = area;
		MinionsBehaviour.skDamage[id] = damage;
		MinionsBehaviour.skExp[id] = exp;
		MinionsBehaviour.skLevel[id] = level;
		MinionsBehaviour.skId[id] = id;
		MinionsBehaviour.skTargetPos[id] = targetpos;
		MinionsBehaviour.skTargetID[id] = targetid;
	}

	private void OnDestroy()
	{
		skWait.Dispose();
		skActive.Dispose();
		skCycle.Dispose();
		skArea.Dispose();
		skDamage.Dispose();
		skExp.Dispose();
		skLevel.Dispose();
		skId.Dispose();
		skTargetPos.Dispose();
		skTargetID.Dispose();
	}
}
