using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitSpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    // Add fields to your component here. Remember that:
    //
    // * The purpose of this class is to store data for authoring purposes - it is not for use while the game is
    //   running.
    // 
    // * Traditional Unity serialization rules apply: fields must be public or marked with [SerializeField], and
    //   must be one of the supported types.
    //
    // For example,
    //    public float scale;

    [SerializeField] private GameObject unitPrefab;

    public static Entity unitEntity;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Call methods on 'dstManager' to create runtime components on 'entity' here. Remember that:
        //
        // * You can add more than one component to the entity. It's also OK to not add any at all.
        //
        // * If you want to create more than one entity from the data in this class, use the 'conversionSystem'
        //   to do it, instead of adding entities through 'dstManager' directly.
        //
        // For example,
        //   dstManager.AddComponentData(entity, new Unity.Transforms.Scale { Value = scale });

        using BlobAssetStore blobAssetStore = new BlobAssetStore();

        Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(unitPrefab,
            GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
        dstManager.AddComponent<UnitComponent>(prefabEntity);
        dstManager.AddComponentData(prefabEntity, new MoveComponent { speed = 10 });

        for (int i = 0; i < 10; i++)
        {
            dstManager.Instantiate(prefabEntity);
        }

        dstManager.DestroyEntity(prefabEntity);
    }
}
