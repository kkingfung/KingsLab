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

public class QuadrantSystem : JobComponentSystem
{
    EntityQuery entityQuery;

    public static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;

    public const int quadrantYMultiplier = 100;
    private const int quadrantCellSize =10;

    public static int GetPositionHashMapKey(float3 position)
    {
        return (int)(math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.z / quadrantCellSize)));
    }

    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobChunk
    {
        public EntityTypeHandle entityType;
        public ComponentTypeHandle<Translation> translationType;
        public ComponentTypeHandle<QuadrantEntity> quadrantEntityType;

        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrantMultiHashMap;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslation = chunk.GetNativeArray(translationType);
            var chunkQuadrant = chunk.GetNativeArray(quadrantEntityType);
            var chunkEntity = chunk.GetNativeArray(entityType);

            for (int i = 0; i < chunk.Count; ++i)
            {
                int hashMapKey = GetPositionHashMapKey(chunkTranslation[i].Value);
                quadrantMultiHashMap.Add(hashMapKey, new QuadrantData
                {
                    entity = chunkEntity[i],
                    position = chunkTranslation[i].Value,
                    quadrantEntity = chunkQuadrant[i]
                });
            }
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

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        entityQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantEntity));

        var entityType = GetEntityTypeHandle();
        var translationType = GetComponentTypeHandle<Translation>(true);
        var quadrantEntityType = GetComponentTypeHandle<QuadrantEntity>(true);

        JobHandle jobHandle = inputDependencies;

        quadrantMultiHashMap.Clear();
        if (entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity)
        {
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        //SetQuadrantDataHashMapJob setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob
        //{
        //    quadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter(),
        //};

        //JobHandle jobHandle = JobForEachExtensions.Schedule(setQuadrantDataHashMapJob, entityQuery);
        //jobHandle.Complete();

        var setQuadrantDataHashMapJob = new SetQuadrantDataHashMapJob()
        {
            quadrantMultiHashMap = quadrantMultiHashMap.AsParallelWriter(),
            entityType = entityType,
            translationType = translationType,
            quadrantEntityType = quadrantEntityType,
        };

        jobHandle = setQuadrantDataHashMapJob.Schedule(entityQuery, inputDependencies);

        return jobHandle;
    }

}
