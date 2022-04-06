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

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        // Assign values to local variables captured in your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     float deltaTime = Time.DeltaTime;

        // This declares a new kind of job, which is a unit of work to do.
        // The job is declared as an Entities.ForEach with the target components as parameters,
        // meaning it will process all entities in the world that have both
        // Translation and Rotation components. Change it to process the component
        // types you want.

        var entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        float deltaTime = Time.DeltaTime;
        
        Entities
            .ForEach((
                Entity entity,
                int entityInQueryIndex,
                ref Translation translation, 
                in Rotation rotation, 
                in MoveComponent moveComponent, 
                in MoveToPositionComponent moveToPositionComponent) => 
            {
                float distance = math.distance(translation.Value, moveToPositionComponent.position);

                if (distance > 5)
                {
                    float3 direction = moveToPositionComponent.position - translation.Value;

                    translation.Value += math.normalize(direction) * moveComponent.speed * deltaTime;
                }
                else
                {
                    entityCommandBuffer.RemoveComponent<MoveToPositionComponent>(entityInQueryIndex, entity);
                }

            })
            .ScheduleParallel();

        CompleteDependency();
    }
}
