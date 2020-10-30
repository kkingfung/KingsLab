using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TargetPos : IComponentData
{
    public float3 Value;
}

public struct TargetFound : IComponentData
{
    public bool Value;
}