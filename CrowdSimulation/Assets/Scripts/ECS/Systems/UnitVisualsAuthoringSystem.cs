using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[System.Serializable]
public struct ShowMovementDebugInfo
{
    public bool showForce;
    public bool showRadius;
    public Color color;
}

public class UnitVisualsAuthoringSystem : AuthoringSystem
{
    [SerializeField] private ShowDebugInfo velocity;
    [SerializeField] private ShowMovementDebugInfo alignment;
    [SerializeField] private ShowMovementDebugInfo cohesion;
    [SerializeField] private ShowMovementDebugInfo separation;
    [SerializeField] private ShowDebugInfo obstacleAvoidanceForce;
    [SerializeField] private ShowDebugInfo obstacleAvoidanceRays;
    [SerializeField] private ShowDebugInfo sense;

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
        unitVisualSystem.obstacleAvoidanceRays = obstacleAvoidanceRays;
        unitVisualSystem.sense = sense;
    }
}

public partial class UnitVisualsSystem : SystemBase
{
    private const float DEBUG_ARROW_SIZE = 2f;

    public ShowDebugInfo velocity;
    public ShowMovementDebugInfo alignment;
    public ShowMovementDebugInfo cohesion;
    public ShowMovementDebugInfo separation;
    public ShowDebugInfo obstacleAvoidanceForce;
    public ShowDebugInfo obstacleAvoidanceRays;
    public ShowDebugInfo sense;

    //private MovementForcesSystem movementForcesSystem;

    protected override void OnCreate()
    {
        //movementForcesSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<MovementForcesSystem>();
    }

    protected override void OnUpdate()
    {
        if (!velocity.show && !alignment.showForce && !cohesion.showForce && !separation.showForce && !obstacleAvoidanceForce.show &&
            !obstacleAvoidanceRays.show && !sense.show) return;

        Entities
            .WithName("Units_ShowForces")
            .WithAll<UnitComponent>()
            .ForEach((
                ref Translation translation, 
                in Rotation rotation, 
                in MoveComponent moveComponent, 
                in MovementForcesComponent movementForcesComponent,
                in UnitSenseComponent unitSenseComponent) =>
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

                //if (obstacleAvoidanceRays.show)
                //{
                //    Debug.DrawRay(translation.Value,
                //        Quaternion.Euler(0, movementForcesSystem.collisionRayAngleOffset, 0) * moveComponent.velocity * movementForcesComponent.obstacleAvoidance.radius,
                //        obstacleAvoidanceRays.color);
                //    Debug.DrawRay(translation.Value,
                //        Quaternion.Euler(0, -movementForcesSystem.collisionRayAngleOffset, 0) * moveComponent.velocity * movementForcesComponent.obstacleAvoidance.radius,
                //        obstacleAvoidanceRays.color);
                //}

                if (sense.show)
                {
                    Debug.DrawRay(translation.Value, moveComponent.velocity * unitSenseComponent.distance, sense.color);
                }
            })
            .WithoutBurst()
            .Run();
    }
}
