using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MoveToDirectionComponent : IComponentData
{
    public float3 direction;
}
