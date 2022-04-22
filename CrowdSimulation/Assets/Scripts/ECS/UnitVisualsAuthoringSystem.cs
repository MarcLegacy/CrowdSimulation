using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[System.Serializable]
public struct MovementForcesDebugInfo
{
    public bool show;
    public Color color;
}

public class UnitVisualsAuthoringSystem : AuthoringSystem
{
    [SerializeField] private MovementForcesDebugInfo velocity;
    [SerializeField] private MovementForcesDebugInfo alignment;
    [SerializeField] private MovementForcesDebugInfo cohesion;
    [SerializeField] private MovementForcesDebugInfo separation;
    [SerializeField] private MovementForcesDebugInfo obstacleAvoidance;
    [SerializeField] private MovementForcesDebugInfo flockingNeighborRadius;
    [SerializeField] private MovementForcesDebugInfo obstacleRaycasts;

    private UnitVisualsSystem unitVisualSystem;

    protected override void Start()
    {
        unitVisualSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitVisualsSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        unitVisualSystem.velocity = velocity;
        unitVisualSystem.alignment = alignment;
        unitVisualSystem.cohesion = cohesion;
        unitVisualSystem.separation = separation;
        unitVisualSystem.obstacleAvoidance = obstacleAvoidance;
        unitVisualSystem.flockingNeighborRadius = flockingNeighborRadius;
        unitVisualSystem.obstacleRaycasts = obstacleRaycasts;
    }
}

public partial class UnitVisualsSystem : SystemBase
{
    private const float DEBUG_ARROW_SIZE = 2f;

    public MovementForcesDebugInfo velocity;
    public MovementForcesDebugInfo alignment;
    public MovementForcesDebugInfo cohesion;
    public MovementForcesDebugInfo separation;
    public MovementForcesDebugInfo obstacleAvoidance;
    public MovementForcesDebugInfo flockingNeighborRadius;
    public MovementForcesDebugInfo obstacleRaycasts;

    private MovementForcesSystem movementForcesSystem;

    protected override void OnCreate()
    {
        movementForcesSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<MovementForcesSystem>();
    }

    protected override void OnUpdate()
    {
        Entities
            .WithName("Units_ShowForces")
            .WithAll<UnitComponent>()
            .ForEach((ref Translation translation, in Rotation rotation, in MoveComponent moveComponent, in MovementForcesComponent movementForcesComponent) =>
            {
                if (velocity.show && !moveComponent.velocity.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, moveComponent.velocity, DEBUG_ARROW_SIZE, velocity.color);
                }

                if (alignment.show && !movementForcesComponent.alignmentForce.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.alignmentForce, DEBUG_ARROW_SIZE, alignment.color);
                }

                if (cohesion.show && !movementForcesComponent.cohesionForce.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.cohesionForce, DEBUG_ARROW_SIZE, cohesion.color);
                }

                if (separation.show && !movementForcesComponent.separationForce.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.separationForce, DEBUG_ARROW_SIZE, separation.color);
                }

                if (obstacleAvoidance.show && !movementForcesComponent.obstacleAvoidanceForce.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.obstacleAvoidanceForce, DEBUG_ARROW_SIZE, obstacleAvoidance.color);
                }

                if (flockingNeighborRadius.show)
                {
                    Utilities.DrawDebugCircle(translation.Value, movementForcesComponent.flockingNeighborRadius, flockingNeighborRadius.color);
                }

                if (obstacleRaycasts.show)
                {
                    Debug.DrawRay(translation.Value,
                        Quaternion.Euler(0, movementForcesSystem.collisionRayOffset, 0) * moveComponent.velocity * movementForcesComponent.obstacleAvoidance.radius,
                        obstacleRaycasts.color);
                    Debug.DrawRay(translation.Value,
                        Quaternion.Euler(0, -movementForcesSystem.collisionRayOffset, 0) * moveComponent.velocity * movementForcesComponent.obstacleAvoidance.radius,
                        obstacleRaycasts.color);
                }
            })
            .WithoutBurst()
            .Run();
    }
}
