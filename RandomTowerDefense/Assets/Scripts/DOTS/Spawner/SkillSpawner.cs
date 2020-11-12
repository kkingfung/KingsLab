using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public class SkillSpawner : MonoBehaviour
{
    private readonly int count = 50;
    public static SkillSpawner Instance { get; private set; }
    public List<GameObject> PrefabObject;

    private EntityManager EntityManager;

    //Array
    [HideInInspector]
    public TransformAccessArray TransformAccessArray;
    public NativeArray<float> lifetimeArray;
    public NativeArray<float3> targetArray;
    public NativeArray<bool> hastargetArray;

    //Bridge
    [HideInInspector]
    public GameObject[] GameObjects;
    public NativeArray<Entity> Entities;

    //For input
    private Transform[] transforms;

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

        //Disposing Array
        if (lifetimeArray.IsCreated)
            lifetimeArray.Dispose();
        if (targetArray.IsCreated)
            targetArray.Dispose();
        if (hastargetArray.IsCreated)
            hastargetArray.Dispose();
    }
    void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //Prepare input
        GameObjects = new GameObject[count];
        transforms = new Transform[count];

        Entities = new NativeArray<Entity>(count, Allocator.Persistent);
        var archetype = EntityManager.CreateArchetype(
            typeof(Damage), typeof(Radius), 
            typeof(WaitingTime), typeof(ActionTime),
            typeof(Lifetime), typeof(SlowRate), typeof(BuffTime),
            ComponentType.ReadOnly<Translation>()
            //ComponentType.ReadOnly<Hybrid>()
            );
        EntityManager.CreateEntity(archetype, Entities);

        TransformAccessArray = new TransformAccessArray(transforms);
        lifetimeArray = new NativeArray<float>(count, Allocator.Persistent);
        targetArray = new NativeArray<float3>(count, Allocator.Persistent);
        hastargetArray = new NativeArray<bool>(count, Allocator.Persistent);
    }
    private void Update() {
        UpdateArrays();
    }

    public void UpdateArrays()
    {
        for (int i = 0; i < GameObjects.Length; ++i)
        {
            if (GameObjects[i] == null) continue;
            lifetimeArray[i] = EntityManager.GetComponentData<Lifetime>(Entities[i]).Value;
            if (EntityManager.HasComponent<Target>(Entities[i]))
            {
                Target target = EntityManager.GetComponentData<Target>(Entities[i]);
                targetArray[i] = target.targetPos;
                hastargetArray[i] = EntityManager.HasComponent<EnemyTag>(target.targetEntity);
                Debug.DrawLine(target.targetPos, GameObjects[i].transform.position, Color.cyan);
            }
            else
            {
                hastargetArray[i] = false;
            }
        }
    }


    public int[] Spawn(int prefabID, float3 Position, float3 EntityPos, float3 Rotation, float damage, float radius,
        float wait, float lifetime,
        float action, float slow=0,float buff=0,
        int num = 1)
    {
        int spawnCnt = 0;
        int[] spawnIndexList = new int[num];
        for (int i = 0; i < count && spawnCnt < num; i++)
        {
            if (GameObjects[i] != null) continue;
            GameObjects[i] = Instantiate(PrefabObject[prefabID], transform);
            GameObjects[i].transform.position = Position;
            GameObjects[i].transform.localRotation = Quaternion.Euler(Rotation);
            transforms[i] = GameObjects[i].transform;
            lifetimeArray[i] = lifetime;

            //AddtoEntities
            EntityManager.SetComponentData(Entities[i], new Damage
            {
                Value = damage,
            });
            EntityManager.SetComponentData(Entities[i], new Radius
            {
                Value = radius,
            });
            EntityManager.SetComponentData(Entities[i], new WaitingTime
            {
                Value = wait,
            });
            EntityManager.SetComponentData(Entities[i], new Lifetime
            {
                Value = lifetime,
            });
            EntityManager.SetComponentData(Entities[i], new ActionTime
            {
                Value = action,
            });
            EntityManager.SetComponentData(Entities[i], new SlowRate
            {
                Value = slow,
            });
            EntityManager.SetComponentData(Entities[i], new BuffTime
            {
                Value = buff,
            });

            if (EntityManager.HasComponent<QuadrantEntity>(Entities[i]))
                EntityManager.RemoveComponent(Entities[i], typeof(QuadrantEntity));
            if (EntityManager.HasComponent<MeteorTag>(Entities[i]))
                EntityManager.RemoveComponent(Entities[i], typeof(MeteorTag));
            if (EntityManager.HasComponent<BlizzardTag>(Entities[i]))
                EntityManager.RemoveComponent(Entities[i], typeof(BlizzardTag));
            if (EntityManager.HasComponent<PetrificationTag>(Entities[i]))
                EntityManager.RemoveComponent(Entities[i], typeof(PetrificationTag));
            if (EntityManager.HasComponent<MinionsTag>(Entities[i]))
                EntityManager.RemoveComponent(Entities[i], typeof(MinionsTag));

            switch (prefabID) {
                case 0:
                        EntityManager.AddComponent(Entities[i], typeof(MeteorTag));
                    break;
                case 1:
                        EntityManager.AddComponent(Entities[i], typeof(BlizzardTag));
                    break;
                case 2:
                        EntityManager.AddComponent(Entities[i], typeof(PetrificationTag));
                    break;
                case 3:
                    EntityManager.AddComponent(Entities[i], typeof(MinionsTag));
                    EntityManager.AddComponent(Entities[i], typeof(QuadrantEntity));
                    EntityManager.SetComponentData(Entities[i], new QuadrantEntity
                    {
                        typeEnum = QuadrantEntity.TypeEnum.MinionsTag
                    });
                    break;
            }

            EntityManager.SetComponentData(Entities[i], new Translation
            {
                Value = EntityPos
            });

            //EntityManager.SetComponentData(Entities[i], new Hybrid
            //{
            //    Index = i,
            //});

            if (EntityManager.HasComponent<SkillTag>(Entities[i]) == false)
                EntityManager.AddComponent<SkillTag>(Entities[i]);
            spawnIndexList[spawnCnt++] = i;
        }

        //Change Whenever Spawned (Not Needed?)
        //TransformAccessArray = new TransformAccessArray(transforms);
        return spawnIndexList;
    }


    public void UpdateEntityPos(int entityID,Vector3 pos) {
        EntityManager.SetComponentData(Entities[entityID], new Translation
        {
            Value = pos,
        });
    }
}