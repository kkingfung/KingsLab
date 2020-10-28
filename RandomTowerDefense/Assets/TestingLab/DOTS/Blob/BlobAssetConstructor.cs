using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

    public struct BehaviourAsset
{
    public BlobArray<BlobTranslation> array;


    //public BlobArray<BlobTranslation> Array;
    public BlobPtr<BlobTranslation> Ptr;
    //public BlobString blobString;
}

[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
public class BlobAssetConstructor : GameObjectConversionSystem
{
    public static BlobAssetReference<BehaviourAsset> reference;
    protected override void OnUpdate()
    {
        using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
        {
            ref BehaviourAsset asset = ref blobBuilder.ConstructRoot<BehaviourAsset>();
            BlobEnemy authoring = 
                GetEntityQuery(typeof(BlobEnemy)).
                ToComponentArray<BlobEnemy>()[0];

            BlobBuilderArray<BlobTranslation> array = blobBuilder.Allocate(ref asset.array, asset.array.Length);
            //assign array
            for (int i = 0; i < authoring.transformArray.Length; ++i) {
                Transform transform = authoring.transformArray[i];
                array[i] = new BlobTranslation { position = transform.position };
            }

            //blobBuilder.AllocateString(ref waypointBlobAsset.blobString, "Test String!");

            ref BlobTranslation point = ref blobBuilder.Allocate(ref asset.Ptr);
            point = new BlobTranslation { position = new float3(0, 0, 0) };

            reference = blobBuilder.CreateBlobAssetReference<BehaviourAsset>(Allocator.Persistent);
        }
        //EntityQuery enmEntityQuery = DstEntityManager.CreateEntityQuery(typeof(BlobEnemy));
        //Entity enmEntity = enmEntityQuery.GetSingletonEntity();

        //DstEntityManager.AddComponentData(enmEntity, new BlobEnemyBehaviour
        //{
        //    behaviourBlobAssetRef = reference,
        //    behaviourIndex = 0
        //});
    }
}
