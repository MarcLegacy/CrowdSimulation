using System.Linq;
using System.Windows.Markup;
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
    [SerializeField] private int entitiesSkippedInJob = 1;

    private UnitSenseSystem unitSenseSystem;

    protected override void Start()
    {
        unitSenseSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitSenseSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        unitSenseSystem.m_entitiesSkippedInJob = entitiesSkippedInJob;
    }
}

public partial class UnitSenseSystem : SystemBase
{
    private const float SENSE_RAY_ANGLE_OFFSET = 20f;

    public int m_entitiesSkippedInJob = 1;

    private int m_currentWorkingEntityInJob;

    protected override void OnUpdate()
    {
        int entitiesSkippedInJob = m_entitiesSkippedInJob;
        int currentWorkingEntityInJob = m_currentWorkingEntityInJob;
        PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        if (m_currentWorkingEntityInJob++ > m_entitiesSkippedInJob)
        {
            m_currentWorkingEntityInJob = 0;
        }

        Entities
            .WithName("Unit_Sensing")
            .WithReadOnly(physicsWorld)
            .WithAll<UnitComponent>()
            .ForEach(
                (Entity entity,
                int entityInQueryIndex,
                ref Translation translation,
                ref UnitSenseComponent unitSenseComponent,
                ref MovementForcesComponent movementForcesComponent,
                in Rotation rotation,
                in PhysicsCollider physicsCollider) =>
            {
                if (entitiesSkippedInJob != 0 && entityInQueryIndex % entitiesSkippedInJob != currentWorkingEntityInJob) return;

                //return;
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
                    if (math.distance(translation.Value, hit.Position) < 2.0f)
                    {
                        unitSenseComponent.isLeftBlocking = true;
                    }

                    //if (!HasComponent<UnitComponent>(hit.Entity))
                    {
                        obstacleAvoidanceForce += new float3(-hit.SurfaceNormal.z, hit.SurfaceNormal.y, hit.SurfaceNormal.x);   // Rotates the direction of the surface normal 90 degrees clockwise.
                    }
                }

                if (physicsWorld.CastRay(rightRayInput, out hit))
                {
                    if (math.distance(translation.Value, hit.Position) < 2.0f)
                    {
                        unitSenseComponent.isRightBlocking = true;
                    }

                    //if (!HasComponent<UnitComponent>(hit.Entity))
                    {
                        obstacleAvoidanceForce += new float3(hit.SurfaceNormal.z, hit.SurfaceNormal.y, -hit.SurfaceNormal.x);   // Rotates the direction of the surface normal 90 degrees counter-clockwise.
                    }
                }

                movementForcesComponent.obstacleAvoidance.force = math.normalizesafe(obstacleAvoidanceForce);
            })
            .ScheduleParallel();
    }
}
