using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ObstacleSpawnManager : MonoBehaviour
{
    private const int TOTAL_WALLS = 4;
    private const float WALL_OFFSET = 0.6f;

    [SerializeField] private GameObject mapObject;
    [SerializeField] private GameObject baseObject;
    [SerializeField] private float avoidanceDistance = 10f;
    [SerializeField] private int obstacleAmount = 10;
    [SerializeField] private Vector2 obstacleScale = new Vector2(1f, 10f);
    [SerializeField] private Color colorA = Color.clear;
    [SerializeField] private Color colorB = Color.clear;
    [SerializeField] private int numOfBorderCellsAvoided = 3;
    [SerializeField] private Color wallColor = Color.clear;
    [SerializeField] private string obstacleName = "Obstacle";
    [SerializeField] private string wallName = "Wall";

    private BlobAssetStore blobAssetStore;

    #region Singleton
    public static ObstacleSpawnManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<ObstacleSpawnManager>();
        }
        return instance;
    }

    private static ObstacleSpawnManager instance;
    #endregion

    private void Start()
    {
        blobAssetStore = new BlobAssetStore();

        CreateWalls();

        for (int i = 0; i < obstacleAmount; i++)
        {
            CreateObstacle();
        }
    }

    private void OnDestroy()
    {
        blobAssetStore.Dispose();
    }

    private void CreateObstacle()
    {
        Vector3 position = FindRandomPosition();
        if (position == Vector3.zero) return;

        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = obstacleName;
        Transform obstacleTransform = obstacle.transform;
        obstacleTransform.SetParent(transform, false);
        obstacleTransform.position = position;
        obstacleTransform.localScale =
            new Vector3(Random.Range(obstacleScale.x, obstacleScale.y), 1f, Random.Range(obstacleScale.x, obstacleScale.y));
        obstacleTransform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        obstacle.GetComponent<MeshRenderer>().materials[0].color = new Color(Random.Range(colorA.r, colorB.r), Random.Range(colorA.g, colorB.g),
            Random.Range(colorA.b, colorB.b));
        obstacle.layer = LayerMask.NameToLayer(GlobalConstants.OBSTACLES_STRING);

        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        Entity entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(obstacle, settings);
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.SetName(entity, obstacleName);
        entityManager.GetComponentData<PhysicsCollider>(entity).Value.Value.Filter = new CollisionFilter()
        {
            BelongsTo = PhysicsCategoryTagNames.OBSTACLE,
            CollidesWith = PhysicsCategoryTagNames.UNIT,
            GroupIndex = 0
        };
    }

    private Vector3 FindRandomPosition()
    {
        Vector2 mapGridSize = new Vector2(mapObject.transform.localScale.x * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER,
            mapObject.transform.localScale.z * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER);
        Vector3 position;
        int positioningTries = 0;
        float cellSize = PathingManager.GetInstance().CellSize;
        Vector3 mapPosition = mapObject.transform.position;

        do
        {
            position = new Vector3(
                Random.Range(mapPosition.x - (mapGridSize.x - cellSize),
                    mapPosition.x + (mapGridSize.x - cellSize)), 0,
                Random.Range(mapPosition.z - (mapGridSize.y - cellSize),
                    mapPosition.z + (mapGridSize.y - cellSize * numOfBorderCellsAvoided)));
            positioningTries++;
        } 
        while (positioningTries < GlobalConstants.MAX_POSITIONING_TRIES && Vector3.Distance(baseObject.transform.position, position) < avoidanceDistance); 
        
        return positioningTries <= GlobalConstants.MAX_POSITIONING_TRIES ? position : Vector3.zero;
    }

    private void CreateWalls()
    {
        Vector2 mapGridSize = new Vector2(mapObject.transform.localScale.x * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER,
            mapObject.transform.localScale.z * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER);
        Vector3 mapPosition = mapObject.transform.position;

        for (int i = 0; i < TOTAL_WALLS; i++)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Transform wallTransform = wall.transform;
            wallTransform.SetParent(transform, false);
            wall.GetComponent<MeshRenderer>().materials[0].color = wallColor;
            wall.layer = LayerMask.NameToLayer(GlobalConstants.OBSTACLES_STRING);

            switch (i)
            {
                case 0:
                    wallTransform.localScale = new Vector3(1f, 1f, mapGridSize.y * 2f);
                    wallTransform.position = new Vector3(mapPosition.x - mapGridSize.x - wallTransform.localScale.x * WALL_OFFSET, 0, mapPosition.y);
                    break;
                case 1:
                    wallTransform.localScale = new Vector3(1f, 1f, mapGridSize.y * 2f);
                    wallTransform.position = new Vector3(mapPosition.x + mapGridSize.x + wallTransform.localScale.x * WALL_OFFSET, 0, mapPosition.y);
                    break;
                case 2:
                    wallTransform.localScale = new Vector3(mapGridSize.x * 2f, 1f, 1f);
                    wallTransform.position = new Vector3(mapPosition.x, 0, mapPosition.y + mapGridSize.y + wallTransform.localScale.y * WALL_OFFSET);
                    break;
                case 3:
                    wallTransform.localScale = new Vector3(mapGridSize.x * 2f, 1f, 1f);
                    wallTransform.position = new Vector3(mapPosition.x, 0, mapPosition.y - mapGridSize.y - wallTransform.localScale.y * WALL_OFFSET);
                    break;
            }

            GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
            Entity entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(wall, settings);
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.SetName(entity, wallName);
            entityManager.GetComponentData<PhysicsCollider>(entity).Value.Value.Filter = new CollisionFilter()
            {
                BelongsTo = PhysicsCategoryTagNames.OBSTACLE,
                CollidesWith = PhysicsCategoryTagNames.UNIT,
                GroupIndex = 0
            };

        }
    }
}
