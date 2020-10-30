using System;
using Unity.Entities;

//Wait until First Collision Check

[Serializable]
public struct ActionTime : IComponentData
{
    public float Value;
}

