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

    private UnitSenseSystem unitSenseSystem;

    protected override void Start()
    {
        unitSenseSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitSenseSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        unitSenseSystem.entitiesSkippedInJob = entitiesSkippedInJob;
    }
}

public partial class UnitSenseSystem : SystemBase
{
    private const float SENSE_RAY_ANGLE_OFFSET = 20f;

    public int entitiesSkippedInJob = 1;

    private int currentWorkingEntityInJob;

    protected override void OnUpdate()
    {
        int _entitiesSkippedInJob = entitiesSkippedInJob;
        int _currentWorkingEntityInJob = currentWorkingEntityInJob;
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
                in Rotation rotation,
                in PhysicsCollider physicsCollider) =>
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

                RaycastInput leftRayInput = new RaycastInput
                {
                    Start = leftRayStartPos,
                    End = leftRayEndPos,
                    Filter = physicsCollider.Value.Value.Filter
                };

                RaycastInput rightRayInput = new RaycastInput
                {
                    Start = rightRayStartPos,
                    End = rightRayEndPos,
                    Filter = physicsCollider.Value.Value.Filter
                };

                unitSenseComponent.isLeftBlocking = false;
                unitSenseComponent.isRightBlocking = false;
                float3 obstacleAvoidanceForce = float3.zero;

                if (physicsWorld.CastRay(leftRayInput, out RaycastHit hit))
                {
                    unitSenseComponent.isLeftBlocking = true;

                    if (!HasComponent<UnitComponent>(hit.Entity))
                    {
                        obstacleAvoidanceForce += (translation.Value - hit.Position);
                    }
                }

                if (physicsWorld.CastRay(rightRayInput, out hit))
                {
                    unitSenseComponent.isRightBlocking = true;

                    if (!HasComponent<UnitComponent>(hit.Entity))
                    {
                        obstacleAvoidanceForce += (translation.Value - hit.Position);
                    }
                }

                movementForcesComponent.obstacleAvoidance.force = math.normalizesafe(obstacleAvoidanceForce);
            })
            .ScheduleParallel();

        //Entities
        //    .WithName("Units_TestColliderCasting")
        //    .WithReadOnly(physicsWorld)
        //    .WithAll<UnitComponent>()
        //    .ForEach((Entity entity, DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer, in Translation translation, in PhysicsCollider physicsCollider) =>
        //    {
        //        NativeList<ColliderCastHit> outHits = new NativeList<ColliderCastHit>(Allocator.Temp);

        //        //Utilities.DrawDebugCircle(translation.Value, 5f, Color.blue);
        //        neighborUnitBuffer.Clear();

        //        if (physicsWorld.SphereCastAll(translation.Value, 5f, Vector3.up, 1f, ref outHits, physicsCollider.Value.Value.Filter))
        //        {
        //            foreach (ColliderCastHit outHit in outHits)
        //            {
        //                if (outHit.Entity.Equals(entity)) continue;


        //                NeighborUnitBufferElement neighborUnitBufferElement = new NeighborUnitBufferElement()
        //                {
        //                    unit = outHit.Entity
        //                };
        //                neighborUnitBuffer.Add(neighborUnitBufferElement);
        //            }
        //        }

        //    })
        //    .ScheduleParallel();
    }
}
