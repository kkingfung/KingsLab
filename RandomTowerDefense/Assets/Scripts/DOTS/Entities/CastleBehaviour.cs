using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using System.Linq;

public class CastleBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{	public static CastleBehaviour Instance { private set; get; }
	public static NativeArray<float> castleHP;

	private void Awake()
	{
		Instance = this;
		castleHP = new NativeArray<float>(1, Allocator.Persistent);
	}

	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		var archeType = manager.CreateArchetype(typeof(Hybrid), typeof(CastleTag), typeof(Health), typeof(Area));
		var instance = new NativeArray<Entity>(1, Allocator.Temp);
		manager.CreateEntity(archeType, instance);
		instance.Dispose();
	}

	public static void ChgData(int castleHP) {
		EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
		manager.CompleteAllJobs();
		CastleBehaviour.castleHP[0] = (float)castleHP;
	}
    private void OnDestroy()
    {
		castleHP.Dispose();
    }
}