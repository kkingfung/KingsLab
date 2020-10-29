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
	public static NativeArray<float> twrArea;

	public static NativeArray<int> twrRank;
	public static NativeArray<int> twrType;

	public static NativeArray<int> twrId;

	public static NativeArray<float> twrTime;
	private void Awake()
	{
		Instance = this;

		twrDamage = new NativeArray<float>(TotalNum, Allocator.Persistent);
		twrArea = new NativeArray<float>(TotalNum, Allocator.Persistent);
		twrTime = new NativeArray<float>(TotalNum, Allocator.Persistent);
		twrRank = new NativeArray<int>(TotalNum, Allocator.Persistent);
		twrType = new NativeArray<int>(TotalNum, Allocator.Persistent);

		twrId = new NativeArray<int>(TotalNum, Allocator.Persistent);
		for (int i = 0; i < twrId.Length; ++i)
			twrId[i] = i;
	}

	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		var archeType = manager.CreateArchetype(typeof(Hybrid), typeof(PlayerTag), typeof(Damage),
			typeof(ActiveTime) , typeof(Area), typeof(Rank), typeof(Type), typeof(ObjID));
		var instance = new NativeArray<Entity>(TotalNum, Allocator.Temp);
		manager.CreateEntity(archeType, instance);

		QuadrantEntity quadrant = new QuadrantEntity { typeEnum= QuadrantEntity.TypeEnum.PlayerTag };
		manager.AddComponentData(entity, quadrant);

		instance.Dispose();
	}

	public static void ChgData(int id, int type, float damage, float area, int rank)
	{
		EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
		manager.CompleteAllJobs();
		TowerBehaviour.twrDamage[id] = damage;
		TowerBehaviour.twrArea[id] = area;

		TowerBehaviour.twrRank[id] = rank;
		TowerBehaviour.twrType[id] = type;

		TowerBehaviour.twrId[id] = id;

		TowerBehaviour.twrTime[id] = 1;
	}

	private void OnDestroy()
	{
		twrDamage.Dispose();
		twrArea.Dispose();
		twrRank.Dispose();
		twrType.Dispose();
		twrTime.Dispose();
		twrId.Dispose();
	}
}
