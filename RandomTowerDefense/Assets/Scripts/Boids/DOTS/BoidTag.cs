using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct BoidTag : IComponentData
{
}

[Serializable]
public struct BoidData : IComponentData
{
    public float3 position;
    public float3 direction;

    public float3 flockHeading;
    public float3 flockCentre;
    public float3 avoidanceHeading;
    public int numFlockmates;

    //public static int Size
    //{
    //    get
    //    {
    //        return sizeof(float) * 3 * 5 + sizeof(int);
    //    }
    //}
}


[Serializable]
public struct BoidRotation : IComponentData
{
    public Quaternion rotation;
}

[Serializable]
public struct BoidDataAvg : IComponentData
{
    public float3 avgFlockHeading;
    public float3 avgAvoidanceHeading;
    public float3 centreOfFlockmates;
    public int numPerceivedFlockmates;
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

//Sphere Bounding instead of Gameobj Bounding Box
[Serializable]
public struct OriPos : IComponentData
{
    public float3 Value;
    public float BoundingRadius;
}