using System;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class MoveAuthoringSystem : AuthoringSystem
{
    [SerializeField] private float maxForce = 0.05f;
    [SerializeField] private float sheerAngle = 5f;

    private MoveSystem moveSystem;

    protected override void Start()
    {
        moveSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<MoveSystem>();
        moveSystem.m_pathingManager = PathingManager.GetInstance();
        moveSystem.m_gridDirectionMap = new NativeHashMap<int2, int2>(moveSystem.m_pathingManager.GridWidth * moveSystem.m_pathingManager.GridHeight,
            Allocator.Persistent);

        base.Start();

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return moveSystem.m_pathingManager.FlowField;

        moveSystem.m_pathingManager.FlowField.OnGridDirectionChanged += moveSystem.OnGridDirectionChanged;
    }

    protected override void SetVariables()
    {
        moveSystem.m_maxForce = maxForce;
        moveSystem.m_sheerAngle = sheerAngle;
    }
}

public partial class MoveSystem : SystemBase
{
    private const float DELTA_ROTATE_DEGREES = 360;

    public float m_maxForce;
    public float m_sheerAngle;
    public NativeHashMap<int2, int2> m_gridDirectionMap;
    public PathingManager m_pathingManager;

    private EndSimulationEntityCommandBufferSystem m_endSimECBS;

    protected override void OnCreate()
    {
        m_endSimECBS = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        m_gridDirectionMap.Dispose();
    }

    protected override void OnUpdate()
    {
        if (m_pathingManager == null || m_pathingManager.FlowField == null || m_gridDirectionMap.Count().Equals(0)) return;

        float deltaTime = Time.DeltaTime;
        MyGrid<FlowFieldCell> flowFieldGrid = m_pathingManager.FlowField.Grid;
        float maxForce = m_maxForce;
        float sheerAngle = m_sheerAngle;
        float3 targetPosition = m_pathingManager.TargetPosition;
        float3 gridOriginPosition = flowFieldGrid.OriginPosition;
        float gridCellSize = flowFieldGrid.CellSize;
        NativeHashMap<int2, int2> gridDirectionMap = m_gridDirectionMap;
        EntityQuery entityQuery = GetEntityQuery(ComponentType.ReadOnly<UnitComponent>());
        NativeHashSet<float3> checkPositions = new NativeHashSet<float3>(entityQuery.CalculateEntityCount(), Allocator.TempJob);
        NativeHashSet<float3>.ParallelWriter checkPositionsParallel = checkPositions.AsParallelWriter();
        var ecb = m_endSimECBS.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithName("Unit_PathForDirection")
            .WithReadOnly(gridDirectionMap)
            .WithAll<UnitComponent>()
            .ForEach((
                Entity entity,
                ref MoveToDirectionComponent moveToDirectionComponent,
                ref GridIndexComponent gridIndexComponent,
                ref Translation translation) =>
            {
                if (gridIndexComponent.gridPosition.Equals(Utilities.CalculateCellGridPosition(targetPosition, gridOriginPosition,
                        gridCellSize)))
                {
                    moveToDirectionComponent.direction = float3.zero;
                    return;
                }

                if (!gridDirectionMap.ContainsKey(gridIndexComponent.gridPosition))
                {
                    translation.Value += math.normalizesafe(float3.zero - translation.Value); // Makes sure that the entities are pushed towards the middle of the map should they wander out of the grid.
                    return;
                }

                int2 direction = gridDirectionMap[gridIndexComponent.gridPosition];

                if (direction.Equals(int2.zero))
                {
                    checkPositionsParallel.Add(translation.Value);
                }
                else
                {
                    moveToDirectionComponent.direction = new float3(direction.x, 0f, direction.y);
                }
            })
            .Schedule();

        Entities
            .WithName("Unit_NewMoving")
            .WithAll<UnitComponent>()
            .ForEach(
                (Entity entity,
                int entityInQueryIndex,
                ref Translation translation,
                ref MoveComponent moveComponent,
                ref Rotation rotation,
                in MoveToDirectionComponent moveToDirectionComponent,
                in MovementForcesComponent movementForcesComponent,
                in UnitSenseComponent unitSenseComponent) =>
            {
                float3 steeringVelocity = moveToDirectionComponent.direction +
                                              movementForcesComponent.alignment.force * movementForcesComponent.alignment.weight +
                                              movementForcesComponent.cohesion.force * movementForcesComponent.cohesion.weight +
                                              movementForcesComponent.separation.force * movementForcesComponent.separation.weight +
                                              movementForcesComponent.obstacleAvoidance.force *
                                              movementForcesComponent.obstacleAvoidance.weight +
                                              movementForcesComponent.collisionPrediction.force *
                                              movementForcesComponent.collisionPrediction.weight;

                steeringVelocity = math.normalizesafe(steeringVelocity);
                moveComponent.velocity = math.normalizesafe(moveComponent.velocity);
                moveComponent.velocity = math.lerp(moveComponent.velocity, steeringVelocity, maxForce);

                // Makes unit able to steer alongside each other depending on the current speed of the unit
                if (unitSenseComponent.isRightBlocking && !unitSenseComponent.isLeftBlocking)
                {
                    moveComponent.velocity = Utilities.RotateVectorYAxis(moveComponent.velocity, math.lerp(-sheerAngle * 2f, -sheerAngle, moveComponent.currentSpeed / moveComponent.maxSpeed));
                }

                if (!unitSenseComponent.isRightBlocking && unitSenseComponent.isLeftBlocking)
                {
                    moveComponent.velocity = Utilities.RotateVectorYAxis(moveComponent.velocity, math.lerp(sheerAngle * 2f, sheerAngle, moveComponent.currentSpeed / moveComponent.maxSpeed));
                }

                // Makes sure that units will rotate towards the direction of the cell they're on when standing still.
                if (moveComponent.currentSpeed < 0.0f && !moveToDirectionComponent.direction.Equals(float3.zero))
                {
                    rotation.Value = Quaternion.RotateTowards(rotation.Value, Quaternion.LookRotation(math.normalizesafe(
                        moveToDirectionComponent.direction + movementForcesComponent.obstacleAvoidance.force *
                        movementForcesComponent.obstacleAvoidance.weight), Vector3.up), DELTA_ROTATE_DEGREES * deltaTime);
                }
                else if (!moveComponent.velocity.Equals(float3.zero))
                {
                    rotation.Value = Quaternion.RotateTowards(rotation.Value, Quaternion.LookRotation(moveComponent.velocity, Vector3.up),
                        DELTA_ROTATE_DEGREES * deltaTime);
                }

                if (moveToDirectionComponent.direction.Equals(float3.zero) || (unitSenseComponent.isLeftBlocking && unitSenseComponent.isRightBlocking))// || (!movementForcesComponent.collisionAvoidance.force.Equals(float3.zero) && (unitSenseComponent.isLeftBlocking || unitSenseComponent.isRightBlocking)))
                {
                    if (moveComponent.currentSpeed > 0f)
                    {
                        moveComponent.currentSpeed -= moveComponent.acceleration * deltaTime;
                    }
                }
                else
                {
                    if (moveComponent.currentSpeed < moveComponent.maxSpeed)
                        moveComponent.currentSpeed += moveComponent.acceleration * deltaTime;
                }

                //ecb.AddComponent(entityInQueryIndex, entity, new URPMaterialPropertyBaseColor { Value = new float4(math.lerp(1, 0, moveComponent.currentSpeed), 0, math.lerp(0, 1, moveComponent.currentSpeed), 1)});

                moveComponent.velocity *= moveComponent.currentSpeed;

                // Pushes units away from each if in collision range.
                if (!movementForcesComponent.collisionAvoidance.force.Equals(float3.zero))
                {
                    translation.Value += movementForcesComponent.collisionAvoidance.force * movementForcesComponent.collisionAvoidance.weight *
                                         moveComponent.currentSpeed * deltaTime;
                }
                else
                {
                    translation.Value += moveComponent.velocity * deltaTime;
                }
            })
            .ScheduleParallel();

        CompleteDependency();

        foreach (float3 checkPosition in checkPositions)
        {
            if (m_pathingManager.CheckedAreas.Contains(m_pathingManager.AreaMap.Grid.GetCell(checkPosition))) continue;

            m_pathingManager.StartPathing(checkPosition, m_pathingManager.TargetPosition);
        }

        checkPositions.Dispose();
    }

    public void OnGridDirectionChanged(object sender, FlowField.OnGridDirectionChangedEventArgs eventArgs)
    {
        for (int x = 0; x < eventArgs.grid.Width; x++)
        {
            for (int y = 0; y < eventArgs.grid.Height; y++)
            {
                int2 key = new int2(x, y);
                if (m_gridDirectionMap.ContainsKey(key))
                {
                    m_gridDirectionMap[key] = Utilities.Vector2IntToInt2(eventArgs.grid.GetCell(x, y).bestDirection.vector2D);
                }
                else
                {
                    m_gridDirectionMap.Add(key, Utilities.Vector2IntToInt2(eventArgs.grid.GetCell(x, y).bestDirection.vector2D));
                }
            }
        }
    }
}

//Entities
//    .WithName("Unit_OldMoving")
//    .WithAll<UnitComponent>()
//    .ForEach(
//        (ref Translation translation,
//        ref MoveComponent moveComponent,
//        ref Rotation rotation,
//        in MoveToDirectionComponent moveToDirectionComponent,
//        in MovementForcesComponent movementForcesComponent,
//        in UnitSenseComponent unitSenseComponent) =>
//    {
//        float3 steering = moveToDirectionComponent.direction +
//                          movementForcesComponent.alignment.force * movementForcesComponent.alignment.weight +
//                          movementForcesComponent.cohesion.force * movementForcesComponent.cohesion.weight +
//                          movementForcesComponent.separation.force * movementForcesComponent.separation.weight +
//                          movementForcesComponent.obstacleAvoidance.force * movementForcesComponent.obstacleAvoidance.weight +
//                          movementForcesComponent.collisionPrediction.force * movementForcesComponent.collisionPrediction.weight;

//        moveComponent.velocity = math.normalizesafe(math.lerp(moveComponent.velocity, steering, maxForce));

//        if (moveComponent.velocity.Equals(float3.zero)) return;

//        //rotation.Value = Quaternion.RotateTowards(rotation.Value, Quaternion.LookRotation(moveComponent.m_velocity, Vector3.up), DELTA_ROTATE_DEGREES);   // Seems they can get quicker loose if the rotation is already done before adjusting the m_velocity on the units in front of them
//        rotation.Value = math.slerp(rotation.Value, quaternion.LookRotation(moveComponent.velocity, math.up()), deltaTime * 15f);

//        if (!moveToDirectionComponent.direction.Equals(float3.zero) && !unitSenseComponent.isLeftBlocking && !unitSenseComponent.isRightBlocking)
//        {
//            if (moveComponent.currentSpeed < moveComponent.maxSpeed)
//                moveComponent.currentSpeed += moveComponent.acceleration * deltaTime;
//        }
//        else
//        {
//            if (moveComponent.currentSpeed > 0f) moveComponent.currentSpeed -= moveComponent.acceleration * deltaTime;
//        }

//        // TODO: Maybe rotate instead of m_velocity
//        if (unitSenseComponent.isLeftBlocking && !unitSenseComponent.isRightBlocking)
//        {
//            //moveComponent.velocity = math.lerp(moveComponent.velocity, Quaternion.Euler(0, 45f, 0) * moveComponent.velocity, 0.5f);
//            moveComponent.velocity = math.lerp(moveComponent.velocity, movementForcesComponent.collisionPrediction.force, 0.5f);
//            //moveComponent.m_velocity = movementForcesComponent.obstacleAvoidance.force;
//        }

//        if (unitSenseComponent.isRightBlocking && !unitSenseComponent.isLeftBlocking)
//        {
//            //moveComponent.velocity = math.lerp(moveComponent.velocity, Quaternion.Euler(0, -45f, 0) * moveComponent.velocity, 0.5f);
//            moveComponent.velocity = math.lerp(moveComponent.velocity, movementForcesComponent.collisionPrediction.force, 0.5f);
//            //moveComponent.m_velocity = movementForcesComponent.obstacleAvoidance.force;
//        }

//        if (movementForcesComponent.collisionAvoidance.force.Equals(float3.zero))
//        {
//            translation.Value += moveComponent.velocity * moveComponent.currentSpeed * deltaTime;
//        }
//        else
//        {
//            translation.Value += movementForcesComponent.collisionAvoidance.force * movementForcesComponent.collisionAvoidance.weight * moveComponent.currentSpeed * deltaTime;
//        }
//    })
//.ScheduleParallel();
