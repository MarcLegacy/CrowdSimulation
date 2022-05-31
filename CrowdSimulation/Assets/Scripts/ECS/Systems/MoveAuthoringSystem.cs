using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MoveAuthoringSystem : AuthoringSystem
{
    [SerializeField] private float maxForce = 0.05f;

    private MoveSystem moveSystem;

    protected override void Start()
    {
        moveSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<MoveSystem>();
        moveSystem.pathingManager = PathingManager.GetInstance();
        moveSystem.gridDirectionMap = new NativeHashMap<int2, int2>(moveSystem.pathingManager.GridWidth * moveSystem.pathingManager.GridHeight,
            Allocator.Persistent);

        base.Start();

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return moveSystem.pathingManager.FlowField;

        moveSystem.pathingManager.FlowField.OnGridDirectionChanged += moveSystem.OnGridDirectionChanged;
    }

    protected override void SetVariables()
    {
        moveSystem.maxForce = maxForce;
    }
}

public partial class MoveSystem : SystemBase
{
    private const float DELTA_ROTATE_DEGREES = 15f;

    public float maxForce;
    public NativeHashMap<int2, int2> gridDirectionMap;

    public PathingManager pathingManager;

    protected override void OnDestroy()
    {
        gridDirectionMap.Dispose();
    }

    protected override void OnUpdate()
    {
        if (pathingManager == null || pathingManager.FlowField == null || gridDirectionMap.Count().Equals(0)) return;

        float deltaTime = Time.DeltaTime;
        MyGrid<FlowFieldCell> flowFieldGrid = pathingManager.FlowField.Grid;
        float _maxForce = maxForce;
        float3 targetPosition = pathingManager.TargetPosition;
        float3 gridOriginPosition = flowFieldGrid.OriginPosition;
        float gridCellSize = flowFieldGrid.CellSize;
        NativeHashMap<int2, int2> _gridDirectionMap = gridDirectionMap;
        EntityQuery entityQuery = GetEntityQuery(ComponentType.ReadOnly<UnitComponent>());
        NativeHashSet<float3> checkPositions = new NativeHashSet<float3>(entityQuery.CalculateEntityCount(), Allocator.TempJob);
        NativeHashSet<float3>.ParallelWriter checkPositionsParallel = checkPositions.AsParallelWriter();

        Entities
            .WithName("Unit_PathForDirection")
            .WithReadOnly(_gridDirectionMap)
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

                if (!_gridDirectionMap.ContainsKey(gridIndexComponent.gridPosition))
                {
                    translation.Value += math.normalizesafe(float3.zero - translation.Value); // Makes sure that the entities are pushed towards the middle of the map
                    return;
                }

                int2 direction = _gridDirectionMap[gridIndexComponent.gridPosition];

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
            .WithName("Units_Moving")
            .WithAll<UnitComponent>()
            .ForEach((
                ref Translation translation,
                ref MoveComponent moveComponent,
                ref Rotation rotation,
                in MoveToDirectionComponent moveToDirectionComponent,
                in MovementForcesComponent movementForcesComponent,
                in UnitSenseComponent unitSenseComponent) =>
            {
                float3 steering = moveToDirectionComponent.direction +
                                  movementForcesComponent.alignment.force * movementForcesComponent.alignment.weight +
                                  movementForcesComponent.cohesion.force * movementForcesComponent.cohesion.weight +
                                  movementForcesComponent.separation.force * movementForcesComponent.separation.weight +
                                  movementForcesComponent.obstacleAvoidance.force * movementForcesComponent.obstacleAvoidance.weight;

                moveComponent.velocity = math.normalizesafe(math.lerp(moveComponent.velocity, steering, _maxForce));

                if (moveComponent.velocity.Equals(float3.zero)) return;

                //rotation.Value = Quaternion.RotateTowards(rotation.Value, Quaternion.LookRotation(moveComponent.velocity, Vector3.up), DELTA_ROTATE_DEGREES);   // Seems they can get quicker loose if the rotation is already done before adjusting the velocity on the units in front of them
                rotation.Value = math.slerp(rotation.Value, quaternion.LookRotation(moveComponent.velocity, math.up()), deltaTime * DELTA_ROTATE_DEGREES);

                if (!moveToDirectionComponent.direction.Equals(float3.zero) && !unitSenseComponent.isLeftBlocking && !unitSenseComponent.isRightBlocking)
                {
                    if (moveComponent.currentSpeed < moveComponent.maxSpeed)
                    {
                        moveComponent.currentSpeed += moveComponent.acceleration * deltaTime;
                    }
                }
                else
                {
                    if (moveComponent.currentSpeed > 0f)
                    {
                        moveComponent.currentSpeed -= moveComponent.acceleration * deltaTime;
                    }
                }

                // TODO: Maybe rotate instead of velocity
                if (unitSenseComponent.isLeftBlocking && !unitSenseComponent.isRightBlocking)
                {
                    moveComponent.velocity = math.lerp(moveComponent.velocity, Quaternion.Euler(0, 45f, 0) * moveComponent.velocity, 0.5f);
                    //moveComponent.velocity = math.lerp(moveComponent.velocity, movementForcesComponent.obstacleAvoidance.force, 0.2f);
                    //moveComponent.velocity = movementForcesComponent.obstacleAvoidance.force;
                }

                if (unitSenseComponent.isRightBlocking && !unitSenseComponent.isLeftBlocking)
                {
                    moveComponent.velocity = math.lerp(moveComponent.velocity, Quaternion.Euler(0, -45f, 0) * moveComponent.velocity, 0.5f);
                    //moveComponent.velocity = math.lerp(moveComponent.velocity, movementForcesComponent.obstacleAvoidance.force, 0.2f);
                    //moveComponent.velocity = movementForcesComponent.obstacleAvoidance.force;
                }

                if (movementForcesComponent.tempAvoidanceDirection.Equals(float3.zero))
                {
                    translation.Value += moveComponent.velocity * moveComponent.currentSpeed * deltaTime;
                }
                else
                {
                    translation.Value += movementForcesComponent.tempAvoidanceDirection * 0.3f * moveComponent.currentSpeed * deltaTime;
                }

            })
            .ScheduleParallel();

        CompleteDependency();

        foreach (float3 checkPosition in checkPositions)
        {
            if (pathingManager.CheckedAreas.Contains(pathingManager.AreaMap.Grid.GetCell(checkPosition))) continue;

            pathingManager.StartPathing(checkPosition, pathingManager.TargetPosition);
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
                if (gridDirectionMap.ContainsKey(key))
                {
                    gridDirectionMap[key] = Utilities.Vector2IntToInt2(eventArgs.grid.GetCell(x, y).bestDirection.vector2D);
                }
                else
                {
                    gridDirectionMap.Add(key, Utilities.Vector2IntToInt2(eventArgs.grid.GetCell(x, y).bestDirection.vector2D));
                }
            }
        }
    }
}
