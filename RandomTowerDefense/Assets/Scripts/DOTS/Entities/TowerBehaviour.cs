using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

public class TowerBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
	public static TowerBehaviour Instance { private set; get; }
	private readonly int TotalNum = 2000;

	public static NativeArray<float> twrDamage;
	public static NativeArray<float> twrWait;
	public static NativeArray<float> twrArea;

	public static NativeArray<int> twrRank;
	public static NativeArray<float> twrLevel;
	public static NativeArray<float> twrExp;
	public static NativeArray<int> twrType;

	public static NativeArray<int> twrId;

	public static NativeArray<float3> twrTargetPos;
	public static NativeArray<int> twrTargetID;
	private void Awake()
	{
		Instance = this;

		twrDamage = new NativeArray<float>(TotalNum, Allocator.Persistent);
		twrWait = new NativeArray<float>(TotalNum, Allocator.Persistent);
		twrArea = new NativeArray<float>(TotalNum, Allocator.Persistent);

		twrRank = new NativeArray<int>(TotalNum, Allocator.Persistent);
		twrLevel = new NativeArray<float>(TotalNum, Allocator.Persistent);
		twrExp = new NativeArray<float>(TotalNum, Allocator.Persistent);
		twrType = new NativeArray<int>(TotalNum, Allocator.Persistent);

		twrId = new NativeArray<int>(TotalNum, Allocator.Persistent);
		for (int i = 0; i < twrId.Length; ++i)
			twrId[i] = i;

		twrTargetPos = new NativeArray<float3>(TotalNum, Allocator.Persistent);
		twrTargetID = new NativeArray<int>(TotalNum, Allocator.Persistent);
	}

	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		var archeType = manager.CreateArchetype(typeof(Hybrid), typeof(PlayerTag), typeof(Damage), typeof(WaitingFrame)
	 ,typeof(Area), typeof(Rank), typeof(Exp), typeof(Level), typeof(Target), typeof(Type), typeof(ObjID));
		var instance = new NativeArray<Entity>(TotalNum, Allocator.Temp);
		manager.CreateEntity(archeType, instance);
		instance.Dispose();
	}

	public static void ChgData(int id, int type, float damage, float wait, float area, int rank, int level, float exp, float3 targetpos, int targetid)
	{
		EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
		manager.CompleteAllJobs();
		TowerBehaviour.twrDamage[id] = damage;
		TowerBehaviour.twrWait[id] = wait;
		TowerBehaviour.twrArea[id] = area;

		TowerBehaviour.twrRank[id] = rank;
		TowerBehaviour.twrLevel[id] = level;
		TowerBehaviour.twrExp[id] = exp;
		TowerBehaviour.twrType[id] = type;

		TowerBehaviour.twrId[id] = id;
		TowerBehaviour.twrTargetPos[id] = targetpos;
		TowerBehaviour.twrTargetID[id] = targetid;
	}

	private void OnDestroy()
	{
		twrDamage.Dispose();
		twrWait.Dispose();
		twrArea.Dispose();
		twrRank.Dispose();
		twrLevel.Dispose();
		twrExp.Dispose();
		twrType.Dispose();
		twrId.Dispose();
		twrTargetPos.Dispose();
		twrTargetID.Dispose();
	}
}
