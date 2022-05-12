using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private UnitSO unitSO;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.SetName(entity, unitSO.unitName);
        dstManager.AddComponent<UnitComponent>(entity);
        dstManager.AddComponentData(entity, new MoveComponent
        {
            maxSpeed = unitSO.speed,
            acceleration = unitSO.acceleration

        });
        dstManager.AddComponentData(entity, new MoveToDirectionComponent { direction = float3.zero });
        dstManager.AddBuffer<NeighborUnitBufferElement>(entity);
        dstManager.AddComponentData(entity, new MovementForcesComponent
        {
            alignment = unitSO.alignment,
            cohesion = unitSO.cohesion,
            separation = unitSO.separation,
            obstacleAvoidance = unitSO.obstacleAvoidance,
        });
        dstManager.AddComponentData(entity, new UnitSenseComponent { distance = unitSO.senseDistance });
        dstManager.AddComponent<GridIndexComponent>(entity);
    }
}

[Serializable]
public struct UnitComponent : IComponentData { }
