using System;
using Unity.Entities;
using Unity.Mathematics;

public struct Target : IComponentData
{
    public Entity targetEntity;
    public float3 targetPos;
    public float targetHealth;
}