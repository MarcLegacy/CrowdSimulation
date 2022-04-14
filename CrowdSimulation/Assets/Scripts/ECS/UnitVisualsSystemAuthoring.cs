using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[System.Serializable]
public struct BehaviorForceDebug
{
    public bool showForce;
    public Color color;
}

public class UnitVisualsSystemAuthoring : SystemAuthoring
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
        var entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithName("Unit_ShowForces_Job")
            .WithAll<UnitComponent>()
            .ForEach((ref Translation translation, in Rotation rotation, in MoveToDirectionComponent moveToDirectionComponent) =>
            {
                if (direction.showForce)
                {
                    Debug.DrawRay(translation.Value, moveToDirectionComponent.direction, direction.color);
                }

                if (alignment.showForce)
                {
                    Debug.DrawRay(translation.Value, moveToDirectionComponent.direction, alignment.color);
                }

                if (cohesion.showForce)
                {
                    Debug.DrawRay(translation.Value, moveToDirectionComponent.direction, cohesion.color);
                }

                if (separation.showForce)
                {
                    Debug.DrawRay(translation.Value, moveToDirectionComponent.direction, separation.color);
                }

                if (collisionAvoidance.showForce)
                {
                    Debug.DrawRay(translation.Value, moveToDirectionComponent.direction, collisionAvoidance.color);
                }
            })
            .WithoutBurst()
            .Run();
    }
}
