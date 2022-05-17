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
        yield return new WaitForEndOfFrame();

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

    protected override void OnCreate()
    {


    }

    protected override void OnDestroy()
    {
        gridDirectionMap.Dispose();
    }

    protected override void OnUpdate()
    {
        if (pathingManager == null || pathingManager.FlowField == null) return;

        float deltaTime = Time.DeltaTime;
        MyGrid<FlowFieldCell> flowFieldGrid = pathingManager.FlowField.Grid;
        float _maxForce = maxForce;
        float3 targetPosition = pathingManager.TargetPosition;
        float3 gridOriginPosition = flowFieldGrid.OriginPosition;
        float gridCellSize = flowFieldGrid.CellSize;
        NativeHashMap<int2, int2> _gridDirectionMap = gridDirectionMap;

        Entities
            .WithName("Unit_PathForDirection_Job")
            .WithAll<UnitComponent>()
            .ForEach((
                Entity entity,
                ref MoveToDirectionComponent moveToDirectionComponent,
                ref GridIndexComponent gridIndexComponent,
                ref Translation translation) =>
            {
                if (Utilities.CalculateCellGridPosition(translation.Value, gridOriginPosition, gridCellSize)
                    .Equals(Utilities.CalculateCellGridPosition(targetPosition, gridOriginPosition, gridCellSize)))
                {
                    moveToDirectionComponent.direction = float3.zero;
                    return;
                }

                FlowFieldCell flowFieldCell = flowFieldGrid.GetCell(translation.Value);

                if (flowFieldCell == null)
                {
                    translation.Value += math.normalizesafe(float3.zero - translation.Value);   // Makes sure that the entities are pushed towards the middle of the map
                    return;
                }

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
            .WithName("Units_Steering")
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

                rotation.Value = Quaternion.RotateTowards(rotation.Value, Quaternion.LookRotation(moveComponent.velocity, Vector3.up), DELTA_ROTATE_DEGREES);   // Seems they can get quicker loose if the rotation is already done before adjusting the velocity on the units in front of them

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

                if (unitSenseComponent.isLeftBlocking && !unitSenseComponent.isRightBlocking)
                {
                    moveComponent.velocity = math.lerp(moveComponent.velocity, Quaternion.Euler(0, 45f, 0) * moveComponent.velocity, 0.5f);
                }

                if (unitSenseComponent.isRightBlocking && !unitSenseComponent.isLeftBlocking)
                {
                    moveComponent.velocity = math.lerp(moveComponent.velocity, Quaternion.Euler(0, -45f, 0) * moveComponent.velocity, 0.5f);
                }

                translation.Value += moveComponent.velocity * moveComponent.currentSpeed * deltaTime;
            })
            .ScheduleParallel();
    }

    public void OnGridDirectionChanged(object sender, FlowField.OnGridDirectionChangedEventArgs eventArgs)
    {
        //for (int x = 0; x < eventArgs.grid.Width; x++)
        //{
        //    for (int y = 0; y < eventArgs.grid.Height; y++)
        //    {
        //        if (gridDirectionMap.TryGetValue(new int2(x, y), out int2 item))
        //        {
        //            Debug.Log("item: " + item);
        //            item = Utilities.Vector2IntToInt2(eventArgs.grid.GetCell(x, y).bestDirection.vector2D);
        //            gridDirectionMap.TryGetValue(new int2(x, y), out int2 newItem);
        //            Debug.Log("newItem: " + newItem);
        //        }
        //        else
        //        {
        //            gridDirectionMap.Add(new int2(x, y), Utilities.Vector2IntToInt2(eventArgs.grid.GetCell(x, y).bestDirection.vector2D));
        //        }

                
        //    }
        //}
    }
}
