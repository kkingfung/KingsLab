using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

public class BlizzardBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
	private readonly int TotalNum = 1000;
	public static BlizzardBehaviour Instance { private set; get; }

	public static NativeArray<float> skWait;
	public static NativeArray<float> skActive;
	public static NativeArray<float> skCycle;
	public static NativeArray<float> skArea;
	public static NativeArray<float> skDamage;

	public static NativeArray<float> skAction;

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
		skAction = new NativeArray<float>(TotalNum, Allocator.Persistent);

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
		var archeType = manager.CreateArchetype(typeof(Hybrid), typeof(SkillTag), typeof(BlizzardTag),
			typeof(ActiveTime), typeof(WaitingFrame), typeof(CycleTime), typeof(Area), typeof(ActionTime),
			typeof(Damage), typeof(Exp), typeof(Level), typeof(Target), typeof(ObjID));
		var instance = new NativeArray<Entity>(TotalNum, Allocator.Temp);
		manager.CreateEntity(archeType, instance);
		instance.Dispose();
	}

	public static void ChgData(int id, float damage, float area, float active, float action,
		float wait, float cycle, float exp, int level, float3 targetpos, int targetid)
	{
		EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
		manager.CompleteAllJobs();
		BlizzardBehaviour.skWait[id] = wait;
		BlizzardBehaviour.skActive[id] = active;
		BlizzardBehaviour.skCycle[id] = cycle;
		BlizzardBehaviour.skAction[id] = action;
		BlizzardBehaviour.skArea[id] = area;
		BlizzardBehaviour.skDamage[id] = damage;
		BlizzardBehaviour.skExp[id] = exp;
		BlizzardBehaviour.skLevel[id] = level;
		BlizzardBehaviour.skId[id] = id;
		BlizzardBehaviour.skTargetPos[id] = targetpos;
		BlizzardBehaviour.skTargetID[id] = targetid;
	}

	private void OnDestroy()
	{
		skWait.Dispose();
		skActive.Dispose();
		skCycle.Dispose();
		skAction.Dispose();
		skArea.Dispose();
		skDamage.Dispose();
		skExp.Dispose();
		skLevel.Dispose();
		skId.Dispose();
		skTargetPos.Dispose();
		skTargetID.Dispose();
	}
}
