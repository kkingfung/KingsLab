using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEditorInternal;

//IMPORTANT:ConverttoEntity Script Required

[GenerateAuthoringComponent]
public struct PrefabEntityComponent : IComponentData
{
    public Entity prefabEntity;
}

public class EntitySpawner : ComponentSystem
{
    private readonly float spawnWaitTime = 0.5f;
    private readonly Vector3[] spawnPosition = new Vector3[3];

    private float spawnTimer;
    private int spawnPositionID;

    protected override void OnCreate()
    {
        for (int i = 0; i < spawnPosition.Length; ++i)
        {
            spawnPosition[i].x = PlayerPrefs.GetFloat("SpawnPointx" + i);
            spawnPosition[i].x = PlayerPrefs.GetFloat("SpawnPointy" + i);
            spawnPosition[i].x = PlayerPrefs.GetFloat("SpawnPointz" + i);
        }
    }


    protected override void OnUpdate()
    {
        spawnTimer -= Time.DeltaTime;
        if (spawnTimer <= 0) {
            spawnTimer = spawnWaitTime;
            spawnPositionID = PlayerPrefs.GetInt("SpawnPositionID");
            //Spawn
            Entity spawnedEntity = EntityManager.Instantiate(PrefabEntitiesExtra.prefabEntity);
            EntityManager.SetComponentData(spawnedEntity, new Translation
            {
                Value = spawnPosition[spawnPositionID]
            });

            //Alternative
            //PrefabEntityComponent prefabEntityComponent = GetSingleton<PrefabEntityComponent>();
            //Entity spawnedEntity = EntityManager.Instantiate(prefabEntityComponent.prefabEntity);
            //EntityManager.SetComponentData(spawnedEntity, new Translation
            //{
            //    Value = spawnPosition[spawnPositionID]
            //});

            //Alternative2
            //Entities.ForEach((ref PrefabEntityComponent prefabEntityComponent) =>
            //{
            //    Entity spawnedEntity = EntityManager.Instantiate(prefabEntityComponent.prefabEntity);
            //    EntityManager.SetComponentData(spawnedEntity, new Translation
            //    {
            //        Value = spawnPosition[spawnPositionID]
            //    });
            //});
        }
    }
}

public class PrefabEntities : MonoBehaviour, IConvertGameObjectToEntity 
{
    public static Entity prefabEntity;
    public List<GameObject> prefabGameObject;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) 
    {
        using (BlobAssetStore blobAssetStore = new BlobAssetStore())
        {
            Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                prefabGameObject[PlayerPrefs.GetInt("CurrMonsterID")],
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            PrefabEntities.prefabEntity = prefabEntity;
        }
    }
}

public class PrefabEntitiesExtra : MonoBehaviour, IDeclareReferencedPrefabs,IConvertGameObjectToEntity
{
    public static Entity prefabEntity;
    public List<GameObject> prefabGameObject;

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) 
    {
        foreach (GameObject i in prefabGameObject)
            referencedPrefabs.Add(i);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Entity prefabEntity = conversionSystem.GetPrimaryEntity(
            prefabGameObject[PlayerPrefs.GetInt("CurrMonsterID")]);
        PrefabEntitiesExtra.prefabEntity = prefabEntity;
    }
}