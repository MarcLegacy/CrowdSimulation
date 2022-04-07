using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public partial class MoveSystem : SystemBase
{
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

        Entities
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
            .WithName("PathForDirection_Job")
            .WithoutBurst()
            .Run();


        Entities
            .ForEach((
                ref Translation translation,
                in MoveComponent moveComponent,
                in MoveToDirectionComponent moveToDirectionComponent) =>
            {
                translation.Value += moveToDirectionComponent.direction * moveComponent.speed * deltaTime;
            })
            .WithName("MoveToDirection_Job")
            .ScheduleParallel();

        CompleteDependency();
    }
}
