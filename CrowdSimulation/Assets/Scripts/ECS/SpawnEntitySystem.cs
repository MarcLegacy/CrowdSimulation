using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial class SpawnEntitySystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    private float cellSize;

    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        cellSize = PathingManager.GetInstance().CellSize;
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
            .WithAll<SpawnEntityComponent>()
            .ForEach((
                Entity entity,
                ref Translation translation, 
                in Rotation rotation) =>
            {
                if (positions.Count == 0) return;

                float3 cellPosition = random.NextFloat3(new float3(-cellSize * 0.5f, 0f, -cellSize * 0.5f),
                    new float3(cellSize * 0.5f, 0f, cellSize * 0.5f));
                translation.Value = positions[random.NextInt(0, positions.Count)] + cellPosition;

                entityCommandBuffer.RemoveComponent<SpawnEntityComponent>(entity);
                entityCommandBuffer.AddComponent(entity, new MoveToDirectionComponent { direction = float3.zero });
                entityCommandBuffer.AddComponent(entity, new PhysicsCollider()
                {
                    Value = Unity.Physics.CapsuleCollider.Create
                    (
                        new CapsuleGeometry()
                        {
                            Radius = 0.5f
                        },
                        new CollisionFilter()
                        {
                            BelongsTo = ~0u,
                            CollidesWith = ~0u,
                            GroupIndex = 0
                        }
                    )
                });
                entityCommandBuffer.AddSharedComponent(entity, new PhysicsWorldIndex());
                entityCommandBuffer.AddBuffer<NeighborUnitBufferElement>(entity);
            })
            .WithoutBurst()
            .Run();
    }
}
