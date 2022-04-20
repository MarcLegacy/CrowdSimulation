
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MovementForcesAuthoringSystem : AuthoringSystem
{
    private MovementForcesSystem movementForcesSystem;

    protected override void Start()
    {
        movementForcesSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<MovementForcesSystem>();

        base.Start();
    }
}

public partial class MovementForcesSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        EntityQuery unitQuery = GetEntityQuery(ComponentType.ReadOnly<UnitComponent>(), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> unitEntities = unitQuery.ToEntityArray(Allocator.TempJob);
        int layerMask = LayerMask.GetMask(GlobalConstants.OBSTACLES_STRING);

        Entities
            .WithName("Units_FindNeighbors_Job")
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer) =>
            {
                neighborUnitBuffer.Clear();
                Translation translation = GetComponent<Translation>(entity);

                foreach (Entity unitEntity in unitEntities)
                {
                    if (unitEntity == entity) continue;

                    Translation unitTranslation = GetComponent<Translation>(unitEntity);

                    if (math.distance(translation.Value, unitTranslation.Value) < 10)
                    {
                        DynamicBuffer<Entity> entityBuffer = neighborUnitBuffer.Reinterpret<Entity>();
                        entityBuffer.Add(unitEntity);
                    }
                }
            })
            .WithDisposeOnCompletion(unitEntities)
            .Schedule();


        Entities
            .WithName("Units_CalculateFlockingForces_Job")
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer, ref MovementForcesComponent movementForceComponent) =>
            {
                DynamicBuffer<Entity> entityBuffer = neighborUnitBuffer.Reinterpret<Entity>();
                Translation translation = GetComponent<Translation>(entity);
                float3 alignmentForce = float3.zero;
                float3 cohesionForce = float3.zero;
                float3 separationForce = float3.zero;

                foreach (Entity unitEntity in entityBuffer)
                {
                    Translation unitTranslation = GetComponent<Translation>(unitEntity);

                    alignmentForce += GetComponent<MoveComponent>(unitEntity).velocity;
                    cohesionForce += unitTranslation.Value;
                    separationForce += unitTranslation.Value - translation.Value;
                }

                alignmentForce /= entityBuffer.Length;
                cohesionForce /= entityBuffer.Length;
                separationForce /= entityBuffer.Length;
                movementForceComponent.alignmentForce = math.normalizesafe(alignmentForce);
                movementForceComponent.cohesionForce = math.normalizesafe(cohesionForce - translation.Value);
                movementForceComponent.separationForce = math.normalizesafe(-separationForce);
            })
            .ScheduleParallel();

        Entities
            .WithName("Units_ObstacleAvoidance_Job")
            .WithAll<UnitComponent>()
            .ForEach((ref MovementForcesComponent movementForcesComponent, in Translation translation, in MoveComponent moveComponent) =>
            {
                Collider[] colliders = Physics.OverlapSphere(translation.Value, movementForcesComponent.flockingNeighborRadius, layerMask);
                float3 cohesionForce = float3.zero;
                foreach (Collider collider in colliders)
                {
                    Debug.DrawLine(translation.Value, collider.gameObject.transform.position, Color.red);

                    //cohesionForce = moveComponent.velocity - collider.
                }
            })
            .WithoutBurst()
            .Run();
    }
}
