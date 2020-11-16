using System;
using Unity.Entities;
using Unity.Mathematics;

//Wait until First Collision Check

[Serializable]
public struct BoidTag : IComponentData
{
    public float Value;
}

[Serializable]
public struct BoidData : IComponentData
{
    //public float3 position;
    public float3 direction;

    public float3 flockHeading;
    public float3 flockCentre;
    public float3 avoidanceHeading;
    public int numFlockmates;

    public static int Size
    {
        get
        {
            return sizeof(float) * 3 * 5 + sizeof(int);
        }
    }
}

[Serializable]
public struct BoidSettingDots : IComponentData
{
    public float minSpeed;
    public float maxSpeed;
    public float perceptionRadius;
    public float avoidanceRadius;
    public float maxSteerForce;

    public float alignWeight;
    public float cohesionWeight;
    public float seperateWeight;

    public float targetWeight;

    public float boundsRadius;
    public float avoidCollisionWeight;
    public float collisionAvoidDst;
}

[Serializable]
public struct OriPos : IComponentData
{
    public float3 Value;
}

[Serializable]
public struct BoidDirection : IComponentData
{
    public float3 Value;
}