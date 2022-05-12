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

        base.Start();
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

    private PathingManager pathingManager;

    protected override void OnCreate()
    {
        pathingManager = PathingManager.GetInstance();
    }

    protected override void OnUpdate()
    {
        if (pathingManager == null || pathingManager.FlowField == null) return;

        float deltaTime = Time.DeltaTime;
        MyGrid<FlowFieldCell> flowFieldGrid = pathingManager.FlowField.Grid;
        float _maxForce = maxForce;

        Entities
            .WithName("Unit_PathForDirection_Job")
            .WithAll<UnitComponent>()
            .ForEach((
                Entity entity,
                ref MoveToDirectionComponent moveToDirectionComponent,
                ref GridIndexComponent gridIndexComponent,
                ref Translation translation) =>
            {
                if (flowFieldGrid.GetCellGridPosition(translation.Value) ==
                    flowFieldGrid.GetCellGridPosition(pathingManager.TargetPosition))
                {
                    moveToDirectionComponent.direction = float3.zero;
                    return;
                }

                FlowFieldCell flowFieldCell = flowFieldGrid.GetCell(translation.Value);

                if (flowFieldCell == null)
                {
                    translation.Value += math.normalizesafe(float3.zero - translation.Value);
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

                if (!moveToDirectionComponent.direction.Equals(float3.zero) && !unitSenseComponent.isBlocking)
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

                    if (unitSenseComponent.leftIsBlocking && !unitSenseComponent.rightIsBlocking)
                    {
                        moveComponent.velocity = math.lerp(moveComponent.velocity, Quaternion.Euler(0, 45f, 0) * moveComponent.velocity, 0.5f);
                    }

                    if (unitSenseComponent.rightIsBlocking && !unitSenseComponent.leftIsBlocking)
                    {
                        moveComponent.velocity = math.lerp(moveComponent.velocity, Quaternion.Euler(0, -45f, 0) * moveComponent.velocity, 0.5f);
                    }
                }

                translation.Value += moveComponent.velocity * moveComponent.currentSpeed * deltaTime;
            })
            .ScheduleParallel();
    }
}
