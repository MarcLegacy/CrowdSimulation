using Unity.Collections;
using Unity.Entities;
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
    private BeginSimulationEntityCommandBufferSystem m_beginSimECBS;
    private EndSimulationEntityCommandBufferSystem m_endSimECBS;

    private NativeList<Entity> m_destroyedEntities;

    protected override void OnCreate()
    {
        m_beginSimECBS = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        m_endSimECBS = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        m_destroyedEntities = new NativeList<Entity>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        m_destroyedEntities.Dispose();
    }

    protected override void OnUpdate()
    {
        var beginSimECB = m_beginSimECBS.CreateCommandBuffer();
        var endSimECB = m_endSimECBS.CreateCommandBuffer();

        NativeList<Entity> destroyedEntities = m_destroyedEntities;

        Entities
            .WithName("Unit_RemoveComponent")
            .WithReadOnly(destroyedEntities)
            .WithAll<DestroyComponent, UnitComponent>()
            .ForEach((Entity entity) =>
            {
                endSimECB.RemoveComponent<UnitComponent>(entity);

                destroyedEntities.Add(entity);
            })
            .Run();

        Entities
            .WithName("Unit_RemoveReferences")
            .WithReadOnly(destroyedEntities)
            .WithAll<UnitComponent>()
            .ForEach((DynamicBuffer<NeighborUnitBufferElement> neighborUnitBuffer) =>
            {
                if (destroyedEntities.IsEmpty) return;

                for (int i = neighborUnitBuffer.Length - 1; i >= 0; i--)
                {
                    Entity unitEntity = neighborUnitBuffer[i].unit;

                    if (!HasComponent<Translation>(unitEntity) || destroyedEntities.Contains(unitEntity))
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
                beginSimECB.DestroyEntity(entity);
            })
            .Run();

        m_destroyedEntities.Clear();

        m_beginSimECBS.AddJobHandleForProducer(Dependency);
        m_endSimECBS.AddJobHandleForProducer(Dependency);
    }
}
