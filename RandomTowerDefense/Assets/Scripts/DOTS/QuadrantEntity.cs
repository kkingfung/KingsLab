using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;


public struct QuadrantEntity : IComponentData
{
    public TypeEnum typeEnum;

    public enum TypeEnum
    {
        PlayerTag,
        EnemyTag,
        MinionsTag
    }
}

public struct HasTarget : IComponentData
{
    public Entity targetEntity;
}

public struct QuadrantData
{
    public Entity entity;
    public float3 position;
    public QuadrantEntity quadrantEntity;
}

public class QuadrantSystem : ComponentSystem
{

    public static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;

    public const int quadrantYMultiplier = 100;
    private const int quadrantCellSize =10;

    public static int GetPositionHashMapKey(float3 position)
    {
        return (int)(math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.z / quadrantCellSize)));
    }

    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation, QuadrantEntity>
    {

        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrantMultiHashMap;

        public void Execute(Entity entity, int index, ref Translation transform, ref QuadrantEntity quadrantEntity)
        {
            int hashMapKey = GetPositionHashMapKey(transform.Value);
            quadrantMultiHashMap.Add(hashMapKey, new QuadrantData
            {
                entity = entity,
                position = transform.Value,
                quadrantEntity = quadrantEntity
            });
        }

    }

    protected override void OnCreate()
    {
        quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        quadrantMultiHashMap.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantEntity));

        quadrantMultiHashMap.Clear();
        if (entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity)
        {
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob
        {
            quadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter(),
        };
        JobHandle jobHandle = JobForEachExtensions.Schedule(setQuadrantDataHashMapJob, entityQuery);
        jobHandle.Complete();
    }

}
