using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct CastlePos : IComponentData
{
    public float3 Value;
}

