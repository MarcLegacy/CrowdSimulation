using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial class SpawnEntitySystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

        float deltaTime = Time.DeltaTime;
        Random random = new Random((uint)(deltaTime * 1000));
        List<float3> positions = new List<float3>();

        if (UnitManager.GetInstance().spawnLocations != null && UnitManager.GetInstance().spawnLocations.Count > 0)
        {
            foreach (Vector3 spawnLocation in UnitManager.GetInstance().spawnLocations)
            {
                positions.Add(spawnLocation);
            }
        }
        
        Entities
            .WithAll<SpawnEntityComponent, UnitComponent>()
            .ForEach((
                Entity entity,
                ref Translation translation, 
                in Rotation rotation) =>
            {
                if (positions.Count == 0) return;

                translation.Value = positions[random.NextInt(0, positions.Count - 1)];

                entityCommandBuffer.RemoveComponent<SpawnEntityComponent>(entity);
                entityCommandBuffer.AddComponent(entity, new MoveToDirectionComponent { direction = float3.zero });
            })
            .WithoutBurst()
            .Run();
    }
}
