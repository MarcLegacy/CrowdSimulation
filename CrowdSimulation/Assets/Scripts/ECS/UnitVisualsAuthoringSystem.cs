using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[System.Serializable]
public struct BehaviorForceDebug
{
    public bool showForce;
    public Color color;
}

public class UnitVisualsAuthoringSystem : AuthoringSystem
{
    [SerializeField] private BehaviorForceDebug direction;
    [SerializeField] private BehaviorForceDebug alignment;
    [SerializeField] private BehaviorForceDebug cohesion;
    [SerializeField] private BehaviorForceDebug separation;
    [SerializeField] private BehaviorForceDebug collisionAvoidance;

    private UnitVisualsSystem unitVisualSystem;

    protected override void Start()
    {
        unitVisualSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitVisualsSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        unitVisualSystem.direction = direction;
        unitVisualSystem.alignment = alignment;
        unitVisualSystem.cohesion = cohesion;
        unitVisualSystem.separation = separation;
        unitVisualSystem.collisionAvoidance = collisionAvoidance;
    }
}

public partial class UnitVisualsSystem : SystemBase
{
    public BehaviorForceDebug direction;
    public BehaviorForceDebug alignment;
    public BehaviorForceDebug cohesion;
    public BehaviorForceDebug separation;
    public BehaviorForceDebug collisionAvoidance;

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        Entities
            .WithName("Unit_ShowForces_Job")
            .WithAll<UnitComponent>()
            .ForEach((ref Translation translation, in Rotation rotation, in MoveComponent moveComponent, in MovementForcesComponent movementForcesComponent) =>
            {
                if (direction.showForce)
                {
                    Debug.DrawRay(translation.Value, moveComponent.velocity, direction.color);
                    Debug.Log("Oui!");
                }

                if (alignment.showForce)
                {
                    Debug.DrawRay(translation.Value, movementForcesComponent.alignmentForce, alignment.color);
                }

                if (cohesion.showForce)
                {
                    Debug.DrawRay(translation.Value, movementForcesComponent.cohesionForce, cohesion.color);
                }

                if (separation.showForce)
                {
                    Debug.DrawRay(translation.Value, movementForcesComponent.separationForce, separation.color);
                }

                if (collisionAvoidance.showForce)
                {
                    Debug.DrawRay(translation.Value, movementForcesComponent.collisionAvoidanceForce, collisionAvoidance.color);
                }
            })
            .WithoutBurst()
            .Run();
    }
}
