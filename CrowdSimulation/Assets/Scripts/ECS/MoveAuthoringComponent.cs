using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class MoveAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private float speed = 10;
    [SerializeField] private float acceleration = 1f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new MoveComponent()
        {
            maxSpeed = speed,
            acceleration = acceleration
        });
    }
}

[Serializable]
public struct MoveComponent : IComponentData
{
    public float maxSpeed;
    public float3 velocity;
    public float currentSpeed;
    public float acceleration;
}
