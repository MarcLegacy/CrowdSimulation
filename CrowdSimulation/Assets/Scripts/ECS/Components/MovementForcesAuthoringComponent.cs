using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct MovementForcesInfo
{
    public float weight;
    public float radius;
    [HideInInspector] public float3 force;
}

[DisallowMultipleComponent]
public class MovementForcesAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private MovementForcesInfo alignment = new MovementForcesInfo();
    [SerializeField] private MovementForcesInfo cohesion = new MovementForcesInfo();
    [SerializeField] private MovementForcesInfo separation = new MovementForcesInfo();
    [SerializeField] private MovementForcesInfo obstacleAvoidance = new MovementForcesInfo();

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MovementForcesComponent
        {
            alignment = alignment,
            cohesion = cohesion,
            separation = separation,
            obstacleAvoidance = obstacleAvoidance,
        });

        dstManager.AddBuffer<NeighborUnitBufferElement>(entity);
    }
}

[Serializable]
public struct MovementForcesComponent : IComponentData
{
    public MovementForcesInfo alignment;
    public MovementForcesInfo cohesion;
    public MovementForcesInfo separation;
    public MovementForcesInfo obstacleAvoidance;
    public float3 tempAvoidanceDirection;
}


