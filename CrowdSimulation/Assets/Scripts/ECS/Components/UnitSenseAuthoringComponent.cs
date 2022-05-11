using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitSenseAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private float distance = 1f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new UnitSenseComponent { distance = distance });
    }
}

[Serializable]
public struct UnitSenseComponent : IComponentData
{
    public float distance;
    public bool isBlocking;
    public float3 force;
    public bool leftIsBlocking;
    public bool rightIsBlocking;
}
