using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MovementForcesAuthoringSystem : AuthoringSystem
{
    [SerializeField] private float collisionRayAngleOffset = 15f;
    [SerializeField] private int entitiesSkippedInFindNeighborsJob = 10;
    [SerializeField] private int entitiesSkippedInObstacleAvoidanceJob = 10;
    [SerializeField] private float pushAwayForce = 0.2f;

    private MovementForcesSystem movementForcesSystem;

    protected override void Start()
    {
        movementForcesSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<MovementForcesSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        movementForcesSystem.collisionRayAngleOffset = collisionRayAngleOffset;
        movementForcesSystem.entitiesSkippedInFindNeighborsJob = entitiesSkippedInFindNeighborsJob;
        movementForcesSystem.entitiesSkippedInObstacleAvoidanceJob = entitiesSkippedInObstacleAvoidanceJob;
        movementForcesSystem.pushAwayForce = pushAwayForce;
    }
}

public partial class MovementForcesSystem : SystemBase
{
    private const float SCALE_DEFAULT = 1f;

    public float collisionRayAngleOffset;
    public int entitiesSkippedInFindNeighborsJob;
    public int entitiesSkippedInObstacleAvoidanceJob;
    public float pushAwayForce = 0.2f;

    private int currentWorkingEntityInFindNeighborsJob;
    private int currentWorkingEntityInObstacleAvoidanceJob;

    private UnitGridIndexSystem unitGridIndexSystem;

    protected override void OnCreate()
    {
        unitGridIndexSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitGridIndexSystem>();
    }

    protected override void OnUpdate()
    {
        if (unitGridIndexSystem.grid == null || !unitGridIndexSystem.indexMap.IsCreated) return;

        NativeMultiHashMap<int2, Entity> indexMap = unitGridIndexSystem.indexMap;

        int _entitiesSkippedInFindNeighborsJob = entitiesSkippedInFindNeighborsJob;
        int _currentWorkingEntityInFindNeighborsJob = currentWorkingEntityInFindNeighborsJob;
        int _entitiesSkippedInObstacleAvoidanceJob = entitiesSkippedInObstacleAvoidanceJob;
        int _currentWorkingEntityInObstacleAvoidanceJob = currentWorkingEntityInObstacleAvoidanceJob;
        float _pushAwayForce = pushAwayForce;

        if (currentWorkingEntityInFindNeighborsJob++ > _entitiesSkippedInFindNeighborsJob)
        {
            currentWorkingEntityInFindNeighborsJob = 0;
        }

        if (currentWorkingEntityInObstacleAvoidanceJob++ > _entitiesSkippedInObstacleAvoidanceJob)
        {
            currentWorkingEntityInObstacleAvoidanceJob = 0;
        }

        Entities
            .WithName("Units_NewFindNeighbors")
            .WithReadOnly(indexMap)
            .WithAll<UnitComponent>()
            .ForEach(
                (Entity entity,
                int entityInQueryIndex,
                DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer,
                in MovementForcesComponent movementForcesComponent,
                in GridIndexComponent gridIndexComponent) =>
            {
                if (entityInQueryIndex % _entitiesSkippedInFindNeighborsJob != _currentWorkingEntityInFindNeighborsJob) return;

                neighborUnitBuffer.Clear();
                Translation translation = GetComponent<Translation>(entity);
                float currentSpeed = GetComponent<MoveComponent>(entity).currentSpeed;

                if (currentSpeed <= 0.0f) return;

                for (int i = 0; i < 9; i++)
                {
                    int2 gridPosition = new int2(-1 + i / 3, -1 + i % 3) + gridIndexComponent.gridPosition; // This makes sure that it also looks to the neighboring cells

                    if (indexMap.TryGetFirstValue(gridPosition, out Entity unitEntity, out NativeMultiHashMapIterator<int2> iterator))
                    {
                        do
                        {
                            if (!unitEntity.Equals(entity) && HasComponent<Translation>(unitEntity))
                            {
                                float unitCurrentSpeed = GetComponent<MoveComponent>(unitEntity).currentSpeed;
                                float3 unitPosition = GetComponent<Translation>(unitEntity).Value;

                                float distance = math.distance(translation.Value, unitPosition);

                                if (distance < SCALE_DEFAULT) translation.Value += math.normalizesafe(translation.Value - unitPosition) * _pushAwayForce;

                                if (unitCurrentSpeed <= 0.0f) continue;

                                NeighborUnitBufferElement neighborUnit = new NeighborUnitBufferElement { unit = unitEntity };

                                if (distance < movementForcesComponent.alignment.radius) neighborUnit.inAlignmentRadius = true;
                                if (distance < movementForcesComponent.cohesion.radius) neighborUnit.inCohesionRadius = true;
                                if (distance < movementForcesComponent.separation.radius) neighborUnit.inSeparationRadius = true;

                                if (neighborUnit.inAlignmentRadius || neighborUnit.inCohesionRadius || neighborUnit.inSeparationRadius)
                                    neighborUnitBuffer.Add(neighborUnit);
                            }
                        } while (indexMap.TryGetNextValue(out unitEntity, ref iterator));
                    }
                }

                SetComponent(entity, translation);
            })
            .Schedule();

        Entities
            .WithName("Units_CalculateFlockingForces")
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer, ref MovementForcesComponent movementForceComponent) =>
            {
                float3 position = GetComponent<Translation>(entity).Value;
                float3 alignmentForce = float3.zero;
                float3 cohesionForce = float3.zero;
                float3 separationForce = float3.zero;
                int alignmentCount = 0;
                int cohesionCount = 0;
                int separationCount = 0;

                foreach (NeighborUnitBufferElement neighborUnit in neighborUnitBuffer)
                {
                    Entity unitEntity = neighborUnit.unit;

                    if (!HasComponent<Translation>(unitEntity)) continue;
                    
                    float3 neighborPosition = GetComponent<Translation>(unitEntity).Value;

                    if (neighborUnit.inAlignmentRadius)
                    {
                        alignmentForce += GetComponent<MoveComponent>(unitEntity).velocity;
                        alignmentCount++;
                    }

                    if (neighborUnit.inCohesionRadius)
                    {
                        cohesionForce += neighborPosition;
                        cohesionCount++;
                    }

                    if (neighborUnit.inSeparationRadius)
                    {
                        separationForce += neighborPosition - position;
                        separationCount++;
                    }
                }

                alignmentForce /= alignmentCount;
                cohesionForce /= cohesionCount;
                separationForce /= separationCount;
                movementForceComponent.alignment.force = math.normalizesafe(alignmentForce);
                movementForceComponent.cohesion.force = math.normalizesafe(cohesionForce - position);
                movementForceComponent.separation.force = math.normalizesafe(-separationForce);
            })
            .ScheduleParallel();

        //Entities
        //    .WithName("Units_ObstacleAvoidance")
        //    .WithAll<UnitComponent>()
        //    .ForEach((
        //        Entity entity,
        //        int entityInQueryIndex,
        //        ref MovementForcesComponent movementForcesComponent,
        //        in Translation translation,
        //        in MoveComponent moveComponent) =>
        //    {
        //        if (entityInQueryIndex % _entitiesSkippedInObstacleAvoidanceJob != _currentWorkingEntityInObstacleAvoidanceJob) return;

        //        movementForcesComponent.obstacleAvoidance.force = float3.zero;

        //        Ray leftRay = new Ray(translation.Value, Quaternion.Euler(0, -collisionRayAngleOffset, 0) * moveComponent.velocity);
        //        Ray rightRay = new Ray(translation.Value, Quaternion.Euler(0, collisionRayAngleOffset, 0) * moveComponent.velocity);
        //        float3 obstacleAvoidanceForce = float3.zero;

        //        obstacleAvoidanceForce += GetAvoidanceForce(translation.Value, leftRay, movementForcesComponent.obstacleAvoidance.radius);
        //        obstacleAvoidanceForce += GetAvoidanceForce(translation.Value, rightRay, movementForcesComponent.obstacleAvoidance.radius);

        //        movementForcesComponent.obstacleAvoidance.force = math.normalizesafe(obstacleAvoidanceForce);
        //    })
        //    .WithoutBurst()
        //    .Run();

        CompleteDependency();
    }

    private static float3 GetAvoidanceForce(float3 position, Ray ray, float rayLength)
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

//EntityQuery unitQuery = GetEntityQuery(ComponentType.ReadOnly<UnitComponent>(), ComponentType.ReadOnly<Translation>());
//NativeArray<Entity> unitEntities = unitQuery.ToEntityArray(Allocator.TempJob);

//Entities
//    .WithName("Units_OldFindNeighbors")
//    .WithAll<UnitComponent>()
//    .ForEach((
//        Entity entity,
//        int entityInQueryIndex,
//        DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer,
//        in MovementForcesComponent movementForcesComponent,
//        in GridIndexComponent gridIndexComponent) =>
//    {
//        if (entityInQueryIndex % _entitiesSkippedInFindNeighborsJob != _currentWorkingEntityInFindNeighborsJob) return;

//        neighborUnitBuffer.Clear();
//        Translation translation = GetComponent<Translation>(entity);

//        foreach (Entity unitEntity in unitEntities)
//        {
//            if (unitEntity == entity || !HasComponent<Translation>(unitEntity)) continue;

//            MoveComponent moveComponent = GetComponent<MoveComponent>(entity);

//            if (moveComponent.currentSpeed <= 0.0f) return;

//            Translation unitTranslation = GetComponent<Translation>(unitEntity);
//            float distance = math.distance(translation.Value, unitTranslation.Value);
//            NeighborUnitBufferElement neighborUnit = new NeighborUnitBufferElement { unit = unitEntity };

//            if (distance < movementForcesComponent.alignment.radius) neighborUnit.inAlignmentRadius = true;
//            if (distance < movementForcesComponent.cohesion.radius) neighborUnit.inCohesionRadius = true;
//            if (distance < movementForcesComponent.separation.radius) neighborUnit.inSeparationRadius = true;

//            if (neighborUnit.inAlignmentRadius || neighborUnit.inCohesionRadius || neighborUnit.inSeparationRadius)
//                neighborUnitBuffer.Add(neighborUnit);
//        }
//    })
//    .WithDisposeOnCompletion(unitEntities)
//    .Schedule();
