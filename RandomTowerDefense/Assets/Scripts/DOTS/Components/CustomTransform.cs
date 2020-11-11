using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct CustomTransform : IComponentData
{
    public float angle;
    public float3 translation;
}

