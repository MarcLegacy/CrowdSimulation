using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MovementForcesComponent : IComponentData
{
    public float3 alignmentForce;
    public float3 cohesionForce;
    public float3 separationForce;
    public float3 collisionAvoidanceForce;
}
