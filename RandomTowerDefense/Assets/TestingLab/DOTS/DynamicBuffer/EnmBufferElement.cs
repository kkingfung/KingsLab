using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[InternalBufferCapacity(1000)]
public struct EnmBufferElement : IBufferElementData
{
    public Entity targetEntity;
}

//public class EnmBufferElementAuthoring : MonoBehaviour, IConvertGameObjectToEntity {

//    public Entity[] targetEntityArray;

//    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
//        DynamicBuffer<EnmBufferElement> dynamicBuffer = dstManager.AddBuffer<EnmBufferElement>(entity);
//        foreach (Entity targetEntity in targetEntityArray) {
//            dynamicBuffer.Add(new EnmBufferElement { targetEntity = targetEntity });
//        }
//    }

//}
