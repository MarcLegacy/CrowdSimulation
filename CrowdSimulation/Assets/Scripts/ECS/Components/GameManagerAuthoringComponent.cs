using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManagerAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<GameManagerComponent>(entity);
    }
}

public struct GameManagerComponent : IComponentData {}
