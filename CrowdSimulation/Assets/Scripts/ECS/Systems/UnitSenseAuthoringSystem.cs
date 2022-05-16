using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

public class UnitSenseAuthoringSystem : AuthoringSystem
{
    public int entitiesSkippedInJob = 1;
    public PhysicsCategoryTags unitTag;
    public PhysicsCategoryTags obstacleTag;

    private UnitSenseSystem unitSenseSystem;

    protected override void Start()
    {
        unitSenseSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitSenseSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        unitSenseSystem.entitiesSkippedInJob = entitiesSkippedInJob;
        unitSenseSystem.unitTag = unitTag;
        unitSenseSystem.obstacleTag = obstacleTag;
    }
}

public partial class UnitSenseSystem : SystemBase
{
    private const float SENSE_RAY_ANGLE_OFFSET = 20f;

    public int entitiesSkippedInJob = 1;
    public PhysicsCategoryTags unitTag;
    public PhysicsCategoryTags obstacleTag;

    private int currentWorkingEntityInJob;

    protected override void OnUpdate()
    {
        int _entitiesSkippedInJob = entitiesSkippedInJob;
        int _currentWorkingEntityInJob = currentWorkingEntityInJob;
        PhysicsCategoryTags _unitTag = unitTag;
        PhysicsCategoryTags _obstacleTag = obstacleTag;
        PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        if (currentWorkingEntityInJob++ > entitiesSkippedInJob)
        {
            currentWorkingEntityInJob = 0;
        }

        Entities
            .WithName("Unit_Sensing")
            .WithReadOnly(physicsWorld)
            .WithAll<UnitComponent>()
            .ForEach((
                Entity entity,
                int entityInQueryIndex,
                ref Translation translation,
                ref UnitSenseComponent unitSenseComponent,
                ref MovementForcesComponent movementForcesComponent,
                in Rotation rotation) =>
            {
                if (entityInQueryIndex % _entitiesSkippedInJob != _currentWorkingEntityInJob) return;

                float3 leftRayStartPos =
                    translation.Value + (float3)(Quaternion.Euler(0, -SENSE_RAY_ANGLE_OFFSET, 0) * math.forward(rotation.Value));
                float3 leftRayEndPos = translation.Value +
                                       (float3)(Quaternion.Euler(0, -SENSE_RAY_ANGLE_OFFSET, 0) * math.forward(rotation.Value)) *
                                       unitSenseComponent.distance;
                float3 rightRayStartPos =
                    translation.Value + (float3)(Quaternion.Euler(0, SENSE_RAY_ANGLE_OFFSET, 0) * math.forward(rotation.Value));
                float3 rightRayEndPos = translation.Value +
                                        (float3)(Quaternion.Euler(0, SENSE_RAY_ANGLE_OFFSET, 0) * math.forward(rotation.Value)) *
                                        unitSenseComponent.distance;

                movementForcesComponent.obstacleAvoidance.force = float3.zero;

                CollisionFilter collisionFilter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                };

                var leftRayInput = new RaycastInput
                {
                    Start = leftRayStartPos,
                    End = leftRayEndPos,
                    Filter = collisionFilter
                };

                var rightRayInput = new RaycastInput
                {
                    Start = rightRayStartPos,
                    End = rightRayEndPos,
                    Filter = collisionFilter
                };

                unitSenseComponent.isBlocking = false;
                unitSenseComponent.leftIsBlocking = false;
                unitSenseComponent.rightIsBlocking = false;
                float3 obstacleAvoidanceForce = float3.zero;

                NativeList<Unity.Physics.RaycastHit> hits = new NativeList<RaycastHit>(Allocator.Temp);

                if (physicsWorld.CastRay(leftRayInput, ref hits))
                {
                    unitSenseComponent.isBlocking = true;
                    unitSenseComponent.leftIsBlocking = true;

                    foreach (var hit in hits)
                    {
                        if (!HasComponent<UnitComponent>(hit.Entity))
                        {
                            obstacleAvoidanceForce += (translation.Value - hit.Position);
                            break;
                        }
                    }
                }

                if (physicsWorld.CastRay(rightRayInput, ref hits))
                {
                    unitSenseComponent.isBlocking = true;
                    unitSenseComponent.rightIsBlocking = true;

                    foreach (var hit in hits)
                    {
                        if (!HasComponent<UnitComponent>(hit.Entity))
                        {
                            obstacleAvoidanceForce += (translation.Value - hit.Position);
                            break;
                        }
                    }
                }

                movementForcesComponent.obstacleAvoidance.force = math.normalizesafe(obstacleAvoidanceForce);
            })
            .ScheduleParallel();
    }
}
