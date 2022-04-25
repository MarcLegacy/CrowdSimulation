using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MovementForcesAuthoringSystem : AuthoringSystem
{
    public float collisionRayOffset = 15f;
    public int entitiesSkippedInFindNeighborsJob = 10;
    public int entitiesSkippedInObstacleAvoidanceJob = 10;

    private MovementForcesSystem movementForcesSystem;

    protected override void Start()
    {
        movementForcesSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<MovementForcesSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        movementForcesSystem.collisionRayOffset = collisionRayOffset;
        movementForcesSystem.entitiesSkippedInFindNeighborsJob = entitiesSkippedInFindNeighborsJob;
        movementForcesSystem.entitiesSkippedInObstacleAvoidanceJob = entitiesSkippedInObstacleAvoidanceJob;
    }
}

public partial class MovementForcesSystem : SystemBase
{
    public float collisionRayOffset;
    public int entitiesSkippedInFindNeighborsJob;
    public int entitiesSkippedInObstacleAvoidanceJob;

    private int currentWorkingEntityInFindNeighborsJob;
    private int currentWorkingEntityInObstacleAvoidanceJob;

    protected override void OnUpdate()
    {
        int _entitiesSkippedInFindNeighborsJob = entitiesSkippedInFindNeighborsJob;
        int _currentWorkingEntityInFindNeighborsJob = currentWorkingEntityInFindNeighborsJob;
        int _entitiesSkippedInObstacleAvoidanceJob = entitiesSkippedInObstacleAvoidanceJob;
        int _currentWorkingEntityInObstacleAvoidanceJob = currentWorkingEntityInObstacleAvoidanceJob;

        EntityQuery unitQuery = GetEntityQuery(ComponentType.ReadOnly<UnitComponent>(), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> unitEntities = unitQuery.ToEntityArray(Allocator.TempJob);

        if (currentWorkingEntityInFindNeighborsJob++ > _entitiesSkippedInFindNeighborsJob)
        {
            currentWorkingEntityInFindNeighborsJob = 0;
        }

        if (currentWorkingEntityInObstacleAvoidanceJob++ > _entitiesSkippedInObstacleAvoidanceJob)
        {
            currentWorkingEntityInObstacleAvoidanceJob = 0;
        }

        Entities
            .WithName("Units_FindNeighbors")
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, int entityInQueryIndex, DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer, in MovementForcesComponent movementForcesComponent) =>
            {
                if (entityInQueryIndex % _entitiesSkippedInFindNeighborsJob != _currentWorkingEntityInFindNeighborsJob) return;

                neighborUnitBuffer.Clear();
                Translation translation = GetComponent<Translation>(entity);

                foreach (Entity unitEntity in unitEntities)
                {
                    if (unitEntity == entity) continue;

                    Translation unitTranslation = GetComponent<Translation>(unitEntity);
                    float distance = math.distance(translation.Value, unitTranslation.Value);
                    NeighborUnitBufferElement neighborUnit = new NeighborUnitBufferElement{unit = unitEntity};

                    if (distance < movementForcesComponent.alignment.radius) neighborUnit.inAlignmentRadius = true;
                    if (distance < movementForcesComponent.cohesion.radius) neighborUnit.inCohesionRadius = true;
                    if (distance < movementForcesComponent.separation.radius) neighborUnit.inSeparationRadius = true;

                    if (neighborUnit.inAlignmentRadius || neighborUnit.inCohesionRadius || neighborUnit.inSeparationRadius)
                        neighborUnitBuffer.Add(neighborUnit);
                }
            })
            .WithDisposeOnCompletion(unitEntities)
            .Schedule();

        Entities
            .WithName("Units_CalculateFlockingForces")
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer, ref MovementForcesComponent movementForceComponent) =>
            {
                Translation translation = GetComponent<Translation>(entity);
                float3 alignmentForce = float3.zero;
                float3 cohesionForce = float3.zero;
                float3 separationForce = float3.zero;

                foreach (NeighborUnitBufferElement neighborUnit in neighborUnitBuffer)
                {
                    Entity unitEntity = neighborUnit.unit;
                    Translation unitTranslation = GetComponent<Translation>(unitEntity);

                    if (neighborUnit.inAlignmentRadius) alignmentForce += GetComponent<MoveComponent>(unitEntity).velocity;
                    if (neighborUnit.inCohesionRadius) cohesionForce += unitTranslation.Value;
                    if (neighborUnit.inSeparationRadius) separationForce += unitTranslation.Value - translation.Value;
                }

                alignmentForce /= neighborUnitBuffer.Length;
                cohesionForce /= neighborUnitBuffer.Length;
                separationForce /= neighborUnitBuffer.Length;
                movementForceComponent.alignment.force = math.normalizesafe(alignmentForce);
                movementForceComponent.cohesion.force = math.normalizesafe(cohesionForce - translation.Value);
                movementForceComponent.separation.force = math.normalizesafe(-separationForce);
            })
            .ScheduleParallel();

        Entities
            .WithName("Units_ObstacleAvoidance")
            .WithAll<UnitComponent>()
            .ForEach((
                Entity entity, 
                int entityInQueryIndex, 
                ref MovementForcesComponent movementForcesComponent, 
                in Translation translation,
                in MoveComponent moveComponent) =>
            {
                if (entityInQueryIndex % _entitiesSkippedInObstacleAvoidanceJob != _currentWorkingEntityInObstacleAvoidanceJob) return;

                movementForcesComponent.obstacleAvoidance.force = float3.zero;

                Ray leftRay = new Ray(translation.Value, Quaternion.Euler(0, -collisionRayOffset, 0) * moveComponent.velocity);
                Ray rightRay = new Ray(translation.Value, Quaternion.Euler(0, collisionRayOffset, 0) * moveComponent.velocity);
                float3 obstacleAvoidanceForce = float3.zero;

                obstacleAvoidanceForce += GetAvoidanceForce(translation.Value, leftRay, movementForcesComponent.obstacleAvoidance.radius);
                obstacleAvoidanceForce += GetAvoidanceForce(translation.Value, rightRay, movementForcesComponent.obstacleAvoidance.radius);

                movementForcesComponent.obstacleAvoidance.force = math.normalizesafe(obstacleAvoidanceForce);
            })
            .WithoutBurst()
            .Run();
    }

    private float3 GetAvoidanceForce(float3 position, Ray ray, float rayLength)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer(GlobalConstants.OBSTACLES_STRING))
            {
                return position - (float3)hit.point;
            }
        }

        return float3.zero;
    }
}
