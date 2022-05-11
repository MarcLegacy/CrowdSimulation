
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class KillEntitiesAuthoringSystem : AuthoringSystem
{
    [SerializeField] private float killRadius = 10f;
    [SerializeField] private Color radiusColor = Color.black;

    private KillEntitiesSystem killEntitiesSystem;

    protected override void Start()
    {
        killEntitiesSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<KillEntitiesSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        killEntitiesSystem.killRadius = killRadius;
        killEntitiesSystem.radiusColor = radiusColor;
    }
}

public partial class KillEntitiesSystem : SystemBase
{
    public float killRadius = 10f;
    public Color radiusColor = Color.black;

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
        float _killRadius = killRadius;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            float3 mouseWorldPosition = Utilities.GetMouseWorldPosition();

            Utilities.DrawDebugCircle(mouseWorldPosition, killRadius, radiusColor);

            if (Input.GetMouseButtonDown(1))
            {
                Entities
                    .WithReadOnly(physicsWorld)
                    .WithAll<GameManagerComponent>()
                    .ForEach(() => 
                    {
                        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

                        CollisionFilter collisionFilter = new CollisionFilter
                        {
                            BelongsTo = ~0u,
                            CollidesWith = ~0u,
                            GroupIndex = 0
                        };

                        if (physicsWorld.SphereCastAll(mouseWorldPosition, _killRadius, Vector3.up, 10f, ref hits, CollisionFilter.Default))
                        {
                            foreach (ColliderCastHit hit in hits)
                            {
                                if (HasComponent<UnitComponent>(hit.Entity))
                                {
                                    entityCommandBuffer.AddComponent<DestroyComponent>(hit.Entity);
                                }
                            }
                        }
                    })
                    .Schedule();
            }
        }

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
