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
            .ForEach((Entity entity, int entityInQueryIndex, DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer) =>
            {
                if (entityInQueryIndex % _entitiesSkippedInFindNeighborsJob != _currentWorkingEntityInFindNeighborsJob) return;

                neighborUnitBuffer.Clear();
                Translation translation = GetComponent<Translation>(entity);

                foreach (Entity unitEntity in unitEntities)
                {
                    if (unitEntity == entity) continue;

                    Translation unitTranslation = GetComponent<Translation>(unitEntity);

                    if (math.distance(translation.Value, unitTranslation.Value) < 10)
                    {
                        DynamicBuffer<Entity> entityBuffer = neighborUnitBuffer.Reinterpret<Entity>();
                        entityBuffer.Add(unitEntity);
                    }
                }
            })
            .WithDisposeOnCompletion(unitEntities)
            .Schedule();

        Entities
            .WithName("Units_CalculateFlockingForces")
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer, ref MovementForcesComponent movementForceComponent) =>
            {
                DynamicBuffer<Entity> entityBuffer = neighborUnitBuffer.Reinterpret<Entity>();
                Translation translation = GetComponent<Translation>(entity);
                float3 alignmentForce = float3.zero;
                float3 cohesionForce = float3.zero;
                float3 separationForce = float3.zero;

                foreach (Entity unitEntity in entityBuffer)
                {
                    Translation unitTranslation = GetComponent<Translation>(unitEntity);

                    alignmentForce += GetComponent<MoveComponent>(unitEntity).velocity;
                    cohesionForce += unitTranslation.Value;
                    separationForce += unitTranslation.Value - translation.Value;
                }

                alignmentForce /= entityBuffer.Length;
                cohesionForce /= entityBuffer.Length;
                separationForce /= entityBuffer.Length;
                movementForceComponent.alignmentForce = math.normalizesafe(alignmentForce);
                movementForceComponent.cohesionForce = math.normalizesafe(cohesionForce - translation.Value);
                movementForceComponent.separationForce = math.normalizesafe(-separationForce);
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

                movementForcesComponent.obstacleAvoidanceForce = float3.zero;

                Ray leftRay = new Ray(translation.Value,
                    Quaternion.Euler(0, -collisionRayOffset, 0) * moveComponent.velocity * movementForcesComponent.flockingNeighborRadius);
                Ray rightRay = new Ray(translation.Value,
                    Quaternion.Euler(0, collisionRayOffset, 0) * moveComponent.velocity * movementForcesComponent.flockingNeighborRadius);
                float3 obstacleAvoidanceForce = float3.zero;
                if (Physics.Raycast(leftRay, out RaycastHit hit, movementForcesComponent.flockingNeighborRadius))
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer(GlobalConstants.OBSTACLES_STRING))
                    {
                        obstacleAvoidanceForce +=
                            moveComponent.velocity - math.normalizesafe((float3)hit.transform.position - translation.Value);
                    }
                }

                if (Physics.Raycast(rightRay, out hit, movementForcesComponent.flockingNeighborRadius))
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer(GlobalConstants.OBSTACLES_STRING))
                    {
                        obstacleAvoidanceForce +=
                            moveComponent.velocity - math.normalizesafe((float3)hit.transform.position - translation.Value);
                    }
                }

                movementForcesComponent.obstacleAvoidanceForce = obstacleAvoidanceForce;
            })
            .WithoutBurst()
            .Run();
    }
}
