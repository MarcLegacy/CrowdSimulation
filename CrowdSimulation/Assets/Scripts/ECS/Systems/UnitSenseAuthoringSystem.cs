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
    public int entitiesSkippedInJob = 1;
    public int entitiesSkippedInFindNeighborsJob = 1;

    private UnitSenseSystem unitSenseSystem;

    protected override void Start()
    {
        unitSenseSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitSenseSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        unitSenseSystem.entitiesSkippedInJob = entitiesSkippedInJob;
        unitSenseSystem.entitiesSkippedInFindNeighborsJob = entitiesSkippedInFindNeighborsJob;
    }
}

public partial class UnitSenseSystem : SystemBase
{
    private const float SENSE_RAY_ANGLE_OFFSET = 20f;

    public int entitiesSkippedInJob = 1;

    private int currentWorkingEntityInJob;

    public int entitiesSkippedInFindNeighborsJob;
    private int currentWorkingEntityInFindNeighborsJob;

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
            .ForEach(
                (Entity entity,
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
                    if (math.distance(translation.Value, hit.Position) < 2.0f)
                    {
                        unitSenseComponent.isLeftBlocking = true;
                    }
                    
                    if (!HasComponent<UnitComponent>(hit.Entity))
                    {
                        obstacleAvoidanceForce += new float3(hit.SurfaceNormal.z, hit.SurfaceNormal.y, -hit.SurfaceNormal.x);
                        //obstacleAvoidanceForce += hit.SurfaceNormal;
                    }
                }

                if (physicsWorld.CastRay(rightRayInput, out hit))
                {
                    if (math.distance(translation.Value, hit.Position) < 2.0f)
                    {
                        unitSenseComponent.isRightBlocking = true;
                    }
                    
                    if (!HasComponent<UnitComponent>(hit.Entity))
                    {
                        obstacleAvoidanceForce += new float3(hit.SurfaceNormal.z, hit.SurfaceNormal.y, -hit.SurfaceNormal.x);
                        //obstacleAvoidanceForce += hit.SurfaceNormal;
                    }
                }

                movementForcesComponent.obstacleAvoidance.force = math.normalizesafe(obstacleAvoidanceForce);
            })
            .ScheduleParallel();

        int _entitiesSkippedInFindNeighborsJob = entitiesSkippedInFindNeighborsJob;
        int _currentWorkingEntityInFindNeighborsJob = currentWorkingEntityInFindNeighborsJob;

        if (currentWorkingEntityInFindNeighborsJob++ > _entitiesSkippedInFindNeighborsJob)
        {
            currentWorkingEntityInFindNeighborsJob = 0;
        }

        //Entities
        //    .WithName("Units_TestColliderCasting")
        //    .WithReadOnly(physicsWorld)
        //    .WithAll<UnitComponent>()
        //    .ForEach(
        //        (Entity entity,
        //            int entityInQueryIndex,
        //            DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer,
        //            ref UnitSenseComponent unitSenseComponent,
        //            ref MovementForcesComponent movementForcesComponent,
        //            in PhysicsCollider physicsCollider,
        //            in Rotation rotation) =>
        //        {
        //            if (entityInQueryIndex % _entitiesSkippedInFindNeighborsJob != _currentWorkingEntityInFindNeighborsJob) return;

        //            //NativeList<ColliderCastHit> outHits = new NativeList<ColliderCastHit>(Allocator.Temp);
        //            NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);
        //            float3 position = GetComponent<Translation>(entity).Value;

        //            neighborUnitBuffer.Clear();
        //            float radius = math.max(movementForcesComponent.alignment.radius,
        //                math.max(movementForcesComponent.cohesion.radius, movementForcesComponent.separation.radius));

        //            float3 forwardDirection = math.forward(rotation.Value);
        //            //float3 leftDirection = Quaternion.Euler(0, -SENSE_RAY_ANGLE_OFFSET, 0) * forwardDirection;
        //            //float3 rightDirection = Quaternion.Euler(0, SENSE_RAY_ANGLE_OFFSET, 0) * forwardDirection;
        //            //Utilities.DrawDebugCircle(position, radius, Color.blue);
        //            //Debug.DrawLine(position, position + forwardDirection * radius, Color.red);
        //            //Debug.DrawLine(position, position + leftDirection * radius, Color.red);
        //            //Debug.DrawLine(position, position + rightDirection * radius, Color.red);

        //            unitSenseComponent.isLeftBlocking = false;
        //            unitSenseComponent.isRightBlocking = false;
        //            float3 obstacleAvoidanceForce = float3.zero;

        //            if (!physicsWorld.OverlapSphere(position, radius, ref distanceHits, physicsCollider.Value.Value.Filter)) return;

        //            foreach (DistanceHit distanceHit in distanceHits)
        //            {
        //                Entity hitEntity = distanceHit.Entity;

        //                if (entity.Equals(hitEntity) || !HasComponent<Translation>(hitEntity)) continue;

        //                float3 hitPosition = distanceHit.Position;
        //                float distance = math.distance(position, hitPosition);

        //                //Debug.DrawLine(position, distanceHit.Position, Color.red);

        //                if (HasComponent<UnitComponent>(hitEntity))
        //                {
        //                    if (GetComponent<MoveComponent>(entity).currentSpeed <= 0.0f ||
        //                        GetComponent<MoveComponent>(hitEntity).currentSpeed <= 0.0f) continue;

        //                    NeighborUnitBufferElement neighborUnit = new NeighborUnitBufferElement { unit = hitEntity };

        //                    if (distance < movementForcesComponent.alignment.radius) neighborUnit.inAlignmentRadius = true;
        //                    if (distance < movementForcesComponent.cohesion.radius) neighborUnit.inCohesionRadius = true;
        //                    if (distance < movementForcesComponent.separation.radius) neighborUnit.inSeparationRadius = true;

        //                    neighborUnitBuffer.Add(neighborUnit);
        //                }

        //                float3 hitDirection = math.normalizesafe(hitPosition - position);
        //                float angle = Quaternion.FromToRotation(forwardDirection, hitDirection).eulerAngles.y;
        //                //Color color = Color.black;

        //                if (distance < unitSenseComponent.distance)
        //                {

        //                    //Debug.Log(angle);
        //                    angle = angle <= 180 ? angle : angle - 360;



        //                    if (angle < 0 && angle > -SENSE_RAY_ANGLE_OFFSET)
        //                    {
        //                        unitSenseComponent.isLeftBlocking = true;
        //                        //color = Color.red;

        //                        if (!HasComponent<UnitComponent>(hitEntity))
        //                        {
        //                            obstacleAvoidanceForce += (position - hitPosition);
        //                        }
        //                    }

        //                    if (angle > 0 && angle < SENSE_RAY_ANGLE_OFFSET)
        //                    {
        //                        unitSenseComponent.isRightBlocking = true;
        //                        //color = Color.yellow;

        //                        if (!HasComponent<UnitComponent>(hitEntity))
        //                        {
        //                            obstacleAvoidanceForce += (position - hitPosition);
        //                        }
        //                    }


        //                }

        //                //Debug.DrawLine(position, position + (float3)(Quaternion.Euler(0, angle, 0) * forwardDirection) * radius, color);

        //            }

        //            obstacleAvoidanceForce /= distanceHits.Length;
        //            movementForcesComponent.obstacleAvoidance.force =
        //                math.normalizesafe(new float3(obstacleAvoidanceForce.x, 0, obstacleAvoidanceForce.z));
        //        })
        //    .ScheduleParallel();
    }
}
