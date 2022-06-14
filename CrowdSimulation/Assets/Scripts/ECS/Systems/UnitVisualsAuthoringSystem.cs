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
    [SerializeField] private DebugInfo velocity;
    [SerializeField] private ShowMovementDebugInfo alignment;
    [SerializeField] private ShowMovementDebugInfo cohesion;
    [SerializeField] private ShowMovementDebugInfo separation;
    [SerializeField] private DebugInfo obstacleAvoidanceForce;
    [SerializeField] private DebugInfo obstacleAvoidanceRays;
    [SerializeField] private DebugInfo sense;

    private UnitVisualsSystem unitVisualSystem;

    protected override void Start()
    {
        unitVisualSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitVisualsSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        unitVisualSystem.m_velocity = velocity;
        unitVisualSystem.m_alignment = alignment;
        unitVisualSystem.m_cohesion = cohesion;
        unitVisualSystem.m_separation = separation;
        unitVisualSystem.m_obstacleAvoidanceForce = obstacleAvoidanceForce;
        unitVisualSystem.m_obstacleAvoidanceRays = obstacleAvoidanceRays;
        unitVisualSystem.m_sense = sense;
    }
}

public partial class UnitVisualsSystem : SystemBase
{
    private const float DEBUG_ARROW_SIZE = 2f;

    public DebugInfo m_velocity;
    public ShowMovementDebugInfo m_alignment;
    public ShowMovementDebugInfo m_cohesion;
    public ShowMovementDebugInfo m_separation;
    public DebugInfo m_obstacleAvoidanceForce;
    public DebugInfo m_obstacleAvoidanceRays;
    public DebugInfo m_sense;

    //private MovementForcesSystem movementForcesSystem;

    protected override void OnCreate()
    {
        //movementForcesSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<MovementForcesSystem>();
    }

    protected override void OnUpdate()
    {
        if (!m_velocity.show && !m_alignment.showForce && !m_cohesion.showForce && !m_separation.showForce && !m_obstacleAvoidanceForce.show &&
            !m_obstacleAvoidanceRays.show && !m_sense.show) return;

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
                if (m_velocity.show && !moveComponent.velocity.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, moveComponent.velocity, DEBUG_ARROW_SIZE, m_velocity.color);
                }

                if (m_alignment.showForce && !movementForcesComponent.alignment.force.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.alignment.force, DEBUG_ARROW_SIZE, m_alignment.color);
                }

                if (m_alignment.showRadius)
                {
                    Utilities.DrawDebugCircle(translation.Value, movementForcesComponent.alignment.radius, m_alignment.color);
                }

                if (m_cohesion.showForce && !movementForcesComponent.cohesion.force.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.cohesion.force, DEBUG_ARROW_SIZE, m_cohesion.color);
                }

                if (m_cohesion.showRadius)
                {
                    Utilities.DrawDebugCircle(translation.Value, movementForcesComponent.cohesion.radius, m_cohesion.color);
                }

                if (m_separation.showForce && !movementForcesComponent.separation.force.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.separation.force, DEBUG_ARROW_SIZE, m_separation.color);
                }

                if (m_separation.showRadius)
                {
                    Utilities.DrawDebugCircle(translation.Value, movementForcesComponent.separation.radius, m_separation.color);
                }

                if (m_obstacleAvoidanceForce.show && !movementForcesComponent.obstacleAvoidance.force.Equals(float3.zero))
                {
                    Utilities.DrawDebugArrow(translation.Value, movementForcesComponent.obstacleAvoidance.force, DEBUG_ARROW_SIZE, m_obstacleAvoidanceForce.color);
                }

                //if (m_obstacleAvoidanceRays.show)
                //{
                //    Debug.DrawRay(translation.Value,
                //        Quaternion.Euler(0, movementForcesSystem.m_collisionRayAngleOffset, 0) * moveComponent.m_velocity * movementForcesComponent.obstacleAvoidance.radius,
                //        m_obstacleAvoidanceRays.color);
                //    Debug.DrawRay(translation.Value,
                //        Quaternion.Euler(0, -movementForcesSystem.m_collisionRayAngleOffset, 0) * moveComponent.m_velocity * movementForcesComponent.obstacleAvoidance.radius,
                //        m_obstacleAvoidanceRays.color);
                //}

                if (m_sense.show)
                {
                    Debug.DrawRay(translation.Value, moveComponent.velocity * unitSenseComponent.distance, m_sense.color);
                }
            })
            .WithoutBurst()
            .Run();
    }
}
