using System;
using Unity.Entities;

[Serializable]
public struct SlowRate : IComponentData
{
    public float Value;
}

