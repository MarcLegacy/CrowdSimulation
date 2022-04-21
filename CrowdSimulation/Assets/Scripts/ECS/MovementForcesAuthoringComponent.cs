using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class MovementForcesAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float separationWeight = 1f;
    [SerializeField] private float obstacleAvoidanceForce = 1f;
    [SerializeField] private float flockingNeighborRadius = 10f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MovementForcesComponent
        {
            alignmentWeight = alignmentWeight,
            cohesionWeight = cohesionWeight,
            separationWeight = separationWeight,
            obstacleAvoidanceWeight = obstacleAvoidanceForce,
            flockingNeighborRadius = flockingNeighborRadius
        });
    }
}

[Serializable]
public struct MovementForcesComponent : IComponentData
{
    public float alignmentWeight;
    public float3 alignmentForce;
    public float cohesionWeight;
    public float3 cohesionForce;
    public float separationWeight;
    public float3 separationForce;
    public float obstacleAvoidanceWeight;
    public float3 obstacleAvoidanceForce;
    public float flockingNeighborRadius;
}


