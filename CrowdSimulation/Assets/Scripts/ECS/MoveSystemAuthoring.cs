using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using Collider = UnityEngine.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

public class MoveSystemAuthoring : SystemAuthoring
{
    [SerializeField] private float unitBehaviorRadius = 5f;

    private MoveSystem moveSystem;

    protected override void Start()
    {
        moveSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<MoveSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        moveSystem.unitBehaviorRadius = unitBehaviorRadius;
    }
}

public partial class MoveSystem : SystemBase
{
    public float unitBehaviorRadius = 5f;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    private PathingManager pathingManager;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        pathingManager = PathingManager.GetInstance();
    }

    protected override unsafe void OnUpdate()
    {
        if (pathingManager.FlowField == null) return;

        var entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        float deltaTime = Time.DeltaTime;
        MyGrid<FlowFieldCell> flowFieldGrid = pathingManager.FlowField.Grid;
        int layerMask = LayerMask.GetMask(GlobalConstants.OBSTACLES_STRING);
        PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
        NativeList<ColliderCastHit> colliderCastHits = new NativeList<ColliderCastHit>(Allocator.TempJob);
        NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.TempJob);
        var translations = GetComponentDataFromEntity<Translation>(true);

        Entities
            .WithName("Unit_PathForDirection_Job")
            .WithAll<UnitComponent>()
            .ForEach((
                ref MoveToDirectionComponent moveToDirectionComponent,             
                in Translation translation) =>
            {
                if (flowFieldGrid.GetCellGridPosition(translation.Value) ==
                    flowFieldGrid.GetCellGridPosition(pathingManager.TargetPosition)) return;

                FlowFieldCell flowFieldCell = flowFieldGrid.GetCell(translation.Value);

                if (flowFieldCell == null) return;

                if (flowFieldCell.bestDirection == GridDirection.None)
                {
                    if (pathingManager.CheckedAreas.Contains(pathingManager.AreaMap.Grid.GetCell(translation.Value))) return;

                    pathingManager.StartPathing(translation.Value, pathingManager.TargetPosition);
                }
                else
                {
                    moveToDirectionComponent.direction =
                        new float3(flowFieldCell.bestDirection.vector2D.x, 0f, flowFieldCell.bestDirection.vector2D.y);
                }
            })
            .WithoutBurst()
            .Run();

        Entities
            .WithName("Unit_MoveToDirection_Job")
            .WithAll<UnitComponent>()
            .ForEach((
                ref Translation translation,
                in MoveComponent moveComponent,
                in MoveToDirectionComponent moveToDirectionComponent) =>
            {
                translation.Value += moveToDirectionComponent.direction * moveComponent.speed * deltaTime;
            })
            .ScheduleParallel();

        Entities
            .WithName("Unit_CollectNeighbors_Job")
            //.WithReadOnly(physicsWorld)
            //.WithReadOnly(translations)
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, in Translation translation, in Rotation rotation, in PhysicsCollider physicsCollider) =>
            {
                Collider[] colliders = Physics.OverlapSphere(translation.Value, unitBehaviorRadius, layerMask);
                foreach (Collider collider in colliders)
                {
                    //Debug.DrawLine(translation.Value, collider.gameObject.transform.position, Color.red);
                }

                //CollisionFilter filter = new CollisionFilter()
                //{
                //    BelongsTo = ~0u,
                //    CollidesWith = ~0u,
                //    GroupIndex = 0
                //};

                //CapsuleGeometry capsuleGeometry = new CapsuleGeometry()
                //{
                //    Radius = 0.5f
                //};

                //BlobAssetReference<Unity.Physics.Collider> capsuleCollider = Unity.Physics.CapsuleCollider.Create(capsuleGeometry, filter);

                CollisionFilter filter = new CollisionFilter()
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                };

                SphereGeometry geometry = new SphereGeometry()
                {
                    Radius = unitBehaviorRadius
                };

                var sphereCollider = SphereCollider.Create(geometry, filter);

                ColliderCastInput input = new ColliderCastInput()
                {
                    //Collider = (Unity.Physics.Collider*)sphereCollider.GetUnsafePtr(),
                    Collider = physicsCollider.ColliderPtr,
                    Orientation = rotation.Value,
                    Start = translation.Value,
                    End = translation.Value
                };

                if (physicsWorld.CollisionWorld.CastCollider(input, ref colliderCastHits))
                {
                    foreach (ColliderCastHit colliderCastHit in colliderCastHits)
                    {
                        Entity hitEntity = colliderCastHit.Entity;
                        if (hitEntity != entity)
                        {
                            Debug.Log("Oui!");

                            float3 hitEntityPosition = translations[hitEntity].Value;
                            Debug.DrawLine(translation.Value, hitEntityPosition, Color.red);
                        }
                    }
                }

                sphereCollider.Dispose();
                colliderCastHits.Clear();



                if (physicsCollider.Value.Value.OverlapSphere(translation.Value, unitBehaviorRadius, ref distanceHits, filter))
                {
                    foreach (DistanceHit distanceHit in distanceHits)
                    {
                        Entity hitEntity = distanceHit.Entity;
                        if (hitEntity != entity)
                        {
                            Debug.Log(hitEntity);
                        }
                    }
                }
            })
            .WithDisposeOnCompletion(colliderCastHits)
            .WithDisposeOnCompletion(distanceHits)
            .WithoutBurst()
            .Run();
    }
}
