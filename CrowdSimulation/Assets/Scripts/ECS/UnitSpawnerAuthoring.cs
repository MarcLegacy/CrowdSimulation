using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int unitAmount = 100;

    public static Entity unitEntity;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        using BlobAssetStore blobAssetStore = new BlobAssetStore();

        Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(unitPrefab,
            GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
        dstManager.AddComponent<UnitComponent>(prefabEntity);
        dstManager.AddComponentData(prefabEntity, new MoveComponent { speed = 10 });
        dstManager.AddComponent<SpawnEntityComponent>(prefabEntity);

        for (int i = 0; i < unitAmount; i++)
        {
            dstManager.Instantiate(prefabEntity);
        }

        dstManager.DestroyEntity(prefabEntity);
    }
}