using System;
using Unity.Entities;

[Serializable]
public struct Money : IComponentData
{
    public int Value;
}

