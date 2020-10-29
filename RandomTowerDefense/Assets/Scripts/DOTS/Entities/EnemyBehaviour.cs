using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;

public class EnemyBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
	public static EnemyBehaviour Instance { private set; get; }
	private readonly int TotalNum = 10000;

	public static NativeArray<float> enmHealth;
	public static NativeArray<int> enmMoney;
	public static NativeArray<float> enmDamage;
	public static NativeArray<float> enmSpeed;
	public static NativeArray<float> enmWait;

	public static NativeArray<float> enmSlowRate;
	public static NativeArray<float> enmBuffTime;

	public static NativeArray<int> enmId;

	public static NativeArray<float3> enmTargetPos;
	public static NativeArray<int> enmTargetID;

	private void Awake()
	{
		Instance = this;

		enmHealth = new NativeArray<float>(TotalNum, Allocator.Persistent);
		enmMoney = new NativeArray<int>(TotalNum, Allocator.Persistent);
		enmDamage = new NativeArray<float>(TotalNum, Allocator.Persistent);
		enmSpeed = new NativeArray<float>(TotalNum, Allocator.Persistent);
		enmWait = new NativeArray<float>(TotalNum, Allocator.Persistent);

		enmSlowRate = new NativeArray<float>(TotalNum, Allocator.Persistent);
		enmBuffTime = new NativeArray<float>(TotalNum, Allocator.Persistent);

		enmId = new NativeArray<int>(TotalNum, Allocator.Persistent);
		for (int i = 0; i < enmId.Length; ++i)
			enmId[i] = i;

		enmTargetPos = new NativeArray<float3>(TotalNum, Allocator.Persistent);
		enmTargetID = new NativeArray<int>(TotalNum, Allocator.Persistent);
	}

	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		var archeType = manager.CreateArchetype(typeof(Hybrid), typeof(EnemyTag), typeof(PathFollow), typeof(Health), typeof(Money)
			, typeof(Speed), typeof(Damage), typeof(WaitingFrame), typeof(SlowRate), typeof(BuffTime)
			, typeof(Area), typeof(Target), typeof(ObjID));
		var instance = new NativeArray<Entity>(TotalNum, Allocator.Temp);
		manager.CreateEntity(archeType, instance);
		instance.Dispose();
	}
	public static void ChgData(int id,float health, float damage, float speed, float wait, 
		int money, float slowrate, float bufftime, float3 targetpos, int targetid)
	{
		EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
		manager.CompleteAllJobs();
		EnemyBehaviour.enmHealth[id] = health;
		EnemyBehaviour.enmMoney[id] = money;
		EnemyBehaviour.enmDamage[id] =damage;
		EnemyBehaviour.enmSpeed[id] = speed;
		EnemyBehaviour.enmWait[id] = wait;
		EnemyBehaviour.enmSlowRate[id] = slowrate;
		EnemyBehaviour.enmBuffTime[id] = bufftime;
		EnemyBehaviour.enmId[id] = id;
		EnemyBehaviour.enmTargetPos[id] = targetpos;
		EnemyBehaviour.enmTargetID[id] = targetid;
	}

	private void OnDestroy()
	{
		enmHealth.Dispose();
		enmMoney.Dispose();
		enmDamage.Dispose();
		enmSpeed.Dispose();
		enmWait.Dispose();
		enmSlowRate.Dispose();
		enmBuffTime.Dispose();
		enmId.Dispose();
		enmTargetPos.Dispose();
		enmTargetID.Dispose();
	}
}