using System;
using Unity.Entities;

[Serializable]
public struct Lifetime : IComponentData
{
    public float Value;
}

