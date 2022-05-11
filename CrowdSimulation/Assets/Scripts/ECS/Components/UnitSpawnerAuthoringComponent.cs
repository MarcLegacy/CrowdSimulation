using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[DisallowMultipleComponent]
public class UnitSpawnerAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private Collider spawnCollider;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int unitAmount = 100;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(unitPrefab,
            GameObjectConversionSettings.FromWorld(dstManager.World, conversionSystem.BlobAssetStore));

        for (int i = 0; i < unitAmount; i++)
        {
            float3 position = new float3(UnityEngine.Random.Range(spawnCollider.bounds.min.x, spawnCollider.bounds.max.x), 0f,
                UnityEngine.Random.Range(spawnCollider.bounds.min.z, spawnCollider.bounds.max.z));

            dstManager.SetComponentData(prefabEntity, new Translation { Value = position });

            dstManager.Instantiate(prefabEntity);
        }

        dstManager.DestroyEntity(prefabEntity);
        dstManager.DestroyEntity(entity);
    }
}
