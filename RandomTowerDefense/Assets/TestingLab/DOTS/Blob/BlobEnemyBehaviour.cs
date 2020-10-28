using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public struct BlobTranslation
{
    public float3 position;
}

public struct BlobEnemyBehaviour : IComponentData
{
    public BlobAssetReference<BehaviourAsset> behaviourBlobAssetRef;
    public int behaviourIndex;
}

public class EnemyBehaviourSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        return Entities.WithAll<BlobEnemy>().ForEach((ref BlobEnemyBehaviour enemyBehaviour) =>
        {
            ref BehaviourAsset asset = ref enemyBehaviour.behaviourBlobAssetRef.Value;
            float3 position = asset.array[enemyBehaviour.behaviourIndex].position;

            //Proceed any code
        }).Schedule(inputDeps);
    }
}
