using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[System.Serializable]
public struct showDebugInfo
{
    public bool show;
    public Color color;
}

[System.Serializable]
public struct showMovementDebugInfo
{
    public bool showForce;
    public bool showRadius;
    public Color color;
}

public class UnitVisualsAuthoringSystem : AuthoringSystem
{
    [SerializeField] private showDebugInfo velocity;
    [SerializeField] private showMovementDebugInfo alignment;
    [SerializeField] private showMovementDebugInfo cohesion;
    [SerializeField] private showMovementDebugInfo separation;
    [SerializeField] private showDebugInfo obstacleAvoidanceForce;
    [SerializeField] private showDebugInfo obstacleAvoidanceRays;

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
        unitVisualSystem.obstacleAvoidanceForce = obstacleAvoidanceForce;
        unitVisualSystem.obstacleAvoiandeRays = obstacleAvoidanceRays;
    }
}

public partial class UnitVisualsSystem : SystemBase
{
    private const float DEBUG_ARROW_SIZE = 2f;

    public showDebugInfo velocity;
    public showMovementDebugInfo alignment;
    public showMovementDebugInfo cohesion;
    public showMovementDebugInfo separation;
    public showDebugInfo obstacleAvoidanceForce;
    public showDebugInfo obstacleAvoiandeRays;

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

                if (alignment.showForce && !movementForcesComponent.alignment.force.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.alignment.force, DEBUG_ARROW_SIZE, alignment.color);
                }

                if (alignment.showRadius)
                {
                    Utilities.DrawDebugCircle(translation.Value, movementForcesComponent.alignment.radius, alignment.color);
                }

                if (cohesion.showForce && !movementForcesComponent.cohesion.force.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.cohesion.force, DEBUG_ARROW_SIZE, cohesion.color);
                }

                if (cohesion.showRadius)
                {
                    Utilities.DrawDebugCircle(translation.Value, movementForcesComponent.cohesion.radius, cohesion.color);
                }

                if (separation.showForce && !movementForcesComponent.separation.force.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.separation.force, DEBUG_ARROW_SIZE, separation.color);
                }

                if (separation.showRadius)
                {
                    Utilities.DrawDebugCircle(translation.Value, movementForcesComponent.separation.radius, separation.color);
                }

                if (obstacleAvoidanceForce.show && !movementForcesComponent.obstacleAvoidance.force.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.obstacleAvoidance.force, DEBUG_ARROW_SIZE, obstacleAvoidanceForce.color);
                }

                if (obstacleAvoiandeRays.show)
                {
                    Debug.DrawRay(translation.Value,
                        Quaternion.Euler(0, movementForcesSystem.collisionRayOffset, 0) * moveComponent.velocity * movementForcesComponent.obstacleAvoidance.radius,
                        obstacleAvoiandeRays.color);
                    Debug.DrawRay(translation.Value,
                        Quaternion.Euler(0, -movementForcesSystem.collisionRayOffset, 0) * moveComponent.velocity * movementForcesComponent.obstacleAvoidance.radius,
                        obstacleAvoiandeRays.color);
                }
            })
            .WithoutBurst()
            .Run();
    }
}
