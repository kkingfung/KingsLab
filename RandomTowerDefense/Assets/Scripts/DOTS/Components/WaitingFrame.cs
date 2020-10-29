using System;
using Unity.Entities;

[Serializable]
public struct WaitingFrame : IComponentData
{
    public float Value;
}

