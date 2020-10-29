using System;
using Unity.Entities;

[Serializable]
public struct ObjID : IComponentData
{
    public int Value;
}
