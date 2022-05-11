using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class DestroyAuhoringSystem : AuthoringSystem
{
    private DestroySystem destroySystem;

    protected override void Start()
    {
        destroySystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<DestroySystem>();

        base.Start();
    }
}

public partial class DestroySystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem beginSimulationEntityCommandBufferSystem;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    private NativeList<Entity> destroyedEntities;

    protected override void OnCreate()
    {
        beginSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        destroyedEntities = new NativeList<Entity>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        destroyedEntities.Dispose();
    }

    protected override void OnUpdate()
    {
        var beginSimulationEntityCommandBuffer = beginSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        var endSimulationEntityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        NativeList<Entity> _destroyedEntities = destroyedEntities;

        Entities
            .WithName("Unit_RemoveComponent")
            .WithReadOnly(_destroyedEntities)
            .WithAll<DestroyComponent, UnitComponent>()
            .ForEach((Entity entity) =>
            {
                endSimulationEntityCommandBuffer.RemoveComponent<UnitComponent>(entity);

                _destroyedEntities.Add(entity);
            })
            .Run();

        Entities
            .WithName("Unit_RemoveReferences")
            .WithReadOnly(_destroyedEntities)
            .WithAll<UnitComponent>()
            .ForEach((DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer) =>
            {
                if (_destroyedEntities.IsEmpty) return;

                for (int i = neighborUnitBuffer.Length - 1; i >= 0; i--)
                {
                    Entity unitEntity = neighborUnitBuffer[i].unit;

                    if (!HasComponent<Translation>(unitEntity) || _destroyedEntities.Contains(unitEntity))
                    {
                        neighborUnitBuffer.RemoveAt(i);
                    }
                }
            })
            .ScheduleParallel();

        Entities
            .WithName("Unit_DestroyEntity")
            .WithAll<DestroyComponent>()
            .WithNone<UnitComponent>()
            .ForEach((Entity entity) =>
            {
                beginSimulationEntityCommandBuffer.DestroyEntity(entity);
            })
            .Run();

        destroyedEntities.Clear();

        beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
