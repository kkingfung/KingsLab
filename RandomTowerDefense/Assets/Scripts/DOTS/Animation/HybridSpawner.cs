using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
public struct Hybrid : IComponentData
{
    public int Index;
}
public class HybridSpawner : MonoBehaviour
{
    public GameObject Object;
    public int Width;
    public int Height;
    public float Spacing;
    [HideInInspector]
    public TransformAccessArray TransformAccessArray;
    [HideInInspector]
    public GameObject[] GameObjects;
    public NativeArray<Entity> Entities;
    EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
    public static HybridSpawner Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void OnDisable()
    {
        if (Entities.IsCreated)
            Entities.Dispose();

        if (TransformAccessArray.isCreated)
            TransformAccessArray.Dispose();
    }
    void Start()
    {
        var count = Width * Height;
        GameObjects = new GameObject[count];
        var transforms = new Transform[count];
        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<Hybrid>());
        EntityManager.CreateEntity(archetype, Entities);
        for (int i = 0; i < count; i++)
        {
            GameObjects[i] = Instantiate(Object, transform);
            transforms[i] = GameObjects[i].transform;
            var x = i % Width;
            var y = i / Width;
            var position = new float3(x * Spacing, 0f, y * Spacing);
            EntityManager.SetComponentData(Entities[i], new Translation
            {
                Value = position,
            });

            EntityManager.SetComponentData(Entities[i], new Hybrid
            {
                Index = i,
            });
        }

        TransformAccessArray = new TransformAccessArray(transforms);
    }
}