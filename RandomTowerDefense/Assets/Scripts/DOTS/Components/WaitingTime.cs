using System;
using Unity.Entities;

//Wait until Next Repeated Collision Check
[Serializable]
public struct WaitingTime : IComponentData
{
    public float Value;
}

