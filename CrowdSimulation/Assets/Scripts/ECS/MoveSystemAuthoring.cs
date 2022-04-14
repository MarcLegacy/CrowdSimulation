using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MoveSystemAuthoring : SystemAuthoring
{
    [SerializeField] private float unitBehaviorRadius = 5f;

    private MoveSystem moveSystem;

    protected override void Start()
    {
        moveSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<MoveSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        moveSystem.unitBehaviorRadius = unitBehaviorRadius;
    }
}

public partial class MoveSystem : SystemBase
{
    public float unitBehaviorRadius = 5f;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    private PathingManager pathingManager;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        pathingManager = PathingManager.GetInstance();
    }

    protected override void OnUpdate()
    {
        if (pathingManager.FlowField == null) return;

        var entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        float deltaTime = Time.DeltaTime;
        MyGrid<FlowFieldCell> flowFieldGrid = pathingManager.FlowField.Grid;
        int layerMask = LayerMask.GetMask(GlobalConstants.OBSTACLES_STRING);

        Entities
            .WithName("Unit_PathForDirection_Job")
            .WithAll<UnitComponent>()
            .ForEach((
                ref MoveToDirectionComponent moveToDirectionComponent,             
                in Translation translation) =>
            {
                if (flowFieldGrid.GetCellGridPosition(translation.Value) ==
                    flowFieldGrid.GetCellGridPosition(pathingManager.TargetPosition)) return;

                FlowFieldCell flowFieldCell = flowFieldGrid.GetCell(translation.Value);

                if (flowFieldCell == null) return;

                if (flowFieldCell.bestDirection == GridDirection.None)
                {
                    if (pathingManager.CheckedAreas.Contains(pathingManager.AreaMap.Grid.GetCell(translation.Value))) return;

                    pathingManager.StartPathing(translation.Value, pathingManager.TargetPosition);
                }
                else
                {
                    moveToDirectionComponent.direction =
                        new float3(flowFieldCell.bestDirection.vector2D.x, 0f, flowFieldCell.bestDirection.vector2D.y);
                }
            })
            .WithoutBurst()
            .Run();

        Entities
            .WithName("Unit_MoveToDirection_Job")
            .WithAll<UnitComponent>()
            .ForEach((
                ref Translation translation,
                in MoveComponent moveComponent,
                in MoveToDirectionComponent moveToDirectionComponent) =>
            {
                translation.Value += moveToDirectionComponent.direction * moveComponent.speed * deltaTime;
            })
            .ScheduleParallel();

        Entities
            .WithName("Unit_CollectNeighbors_Job")
            .ForEach((in Translation translation) =>
            {
                Collider[] colliders = Physics.OverlapSphere(translation.Value, unitBehaviorRadius, layerMask);
                foreach (Collider collider in colliders)
                {
                    Debug.DrawLine(translation.Value, collider.gameObject.transform.position, Color.red);
                }
            })
            .WithoutBurst()
            .Run();
    }
}
