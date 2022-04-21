using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[System.Serializable]
public struct DebugInfo
{
    public bool show;
    public Color color;
}

public class UnitVisualsAuthoringSystem : AuthoringSystem
{
    [SerializeField] private DebugInfo velocity;
    [SerializeField] private DebugInfo alignment;
    [SerializeField] private DebugInfo cohesion;
    [SerializeField] private DebugInfo separation;
    [SerializeField] private DebugInfo obstacleAvoidance;
    [SerializeField] private DebugInfo flockingNeighborRadius;
    [SerializeField] private DebugInfo obstacleRaycasts;

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
        unitVisualSystem.obstacleAvoidanceAvoidance = obstacleAvoidance;
        unitVisualSystem.flockingNeighborRadius = flockingNeighborRadius;
        unitVisualSystem.obstacleRaycasts = obstacleRaycasts;
    }
}

public partial class UnitVisualsSystem : SystemBase
{
    private const float DEBUG_ARROW_SIZE = 2f;

    public DebugInfo velocity;
    public DebugInfo alignment;
    public DebugInfo cohesion;
    public DebugInfo separation;
    public DebugInfo obstacleAvoidanceAvoidance;
    public DebugInfo flockingNeighborRadius;
    public DebugInfo obstacleRaycasts;

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
                if (velocity.show)
                {
                    Utilities.DrawDebugArrow(translation.Value, moveComponent.velocity, DEBUG_ARROW_SIZE, velocity.color);
                }

                if (alignment.show)
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.alignmentForce, DEBUG_ARROW_SIZE, velocity.color);
                }

                if (cohesion.show)
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.cohesionForce, DEBUG_ARROW_SIZE, velocity.color);
                }

                if (separation.show)
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.separationForce, DEBUG_ARROW_SIZE, velocity.color);
                }

                if (obstacleAvoidanceAvoidance.show)
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.obstacleAvoidanceForce, DEBUG_ARROW_SIZE, velocity.color);
                }

                if (flockingNeighborRadius.show)
                {
                    Utilities.DrawDebugCircle(translation.Value, movementForcesComponent.flockingNeighborRadius, flockingNeighborRadius.color);
                }

                if (obstacleRaycasts.show)
                {
                    Debug.DrawRay(translation.Value,
                        Quaternion.Euler(0, movementForcesSystem.collisionRayOffset, 0) * moveComponent.velocity * movementForcesComponent.flockingNeighborRadius,
                        obstacleRaycasts.color);
                    Debug.DrawRay(translation.Value,
                        Quaternion.Euler(0, -movementForcesSystem.collisionRayOffset, 0) * moveComponent.velocity * movementForcesComponent.flockingNeighborRadius,
                        obstacleRaycasts.color);
                }
            })
            .WithoutBurst()
            .Run();
    }
}
