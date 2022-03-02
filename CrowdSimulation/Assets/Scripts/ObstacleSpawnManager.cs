using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ObstacleSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject mapObject;
    [SerializeField] private GameObject baseObject;
    [SerializeField] private float avoidanceDistance = 10f;
    [SerializeField] private int obstacleAmount = 10;
    [SerializeField] private Vector2 obstacleScale = new Vector2(1f, 10f);
    [SerializeField] private Color colorA = Color.clear;
    [SerializeField] private Color colorB = Color.clear;
    [SerializeField] private bool benchmark = false;

    public bool Benchmark => benchmark;

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
        if (benchmark)
        {
            Random.InitState(3);
        }

        for (int i = 0; i < obstacleAmount; i++)
        {
            CreateObstacle();
        }
    }

    private void CreateObstacle()
    {
        Vector3 position = FindRandomPosition();
        if (position == Vector3.zero) return;

        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.transform.position = position;
        obstacle.transform.localScale =
            new Vector3(Random.Range(obstacleScale.x, obstacleScale.y), 1f, Random.Range(obstacleScale.x, obstacleScale.y));
        obstacle.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        obstacle.GetComponent<MeshRenderer>().materials[0].color = new Color(Random.Range(colorA.r, colorB.r), Random.Range(colorA.g, colorB.g),
            Random.Range(colorA.b, colorB.b));
        obstacle.layer = LayerMask.NameToLayer(GlobalConstants.OBSTACLES_STRING);
    }

    private Vector3 FindRandomPosition()
    {
        Vector2 mapGridSize = new Vector2(mapObject.transform.localScale.x * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER,
            mapObject.transform.localScale.z * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER);
        Vector3 position;
        int positioningTries = 0;

        do 
        {
            position =
                new Vector3(Random.Range(mapObject.transform.position.x - mapGridSize.x, mapObject.transform.position.x + mapGridSize.x), 0,
                    Random.Range(mapObject.transform.position.z - mapGridSize.y, mapObject.transform.position.z + mapGridSize.y));
            positioningTries++;
        } 
        while (positioningTries < GlobalConstants.MAX_POSITIONING_TRIES &&
                 Vector3.Distance(baseObject.transform.position, position) < avoidanceDistance);

        return positioningTries <= GlobalConstants.MAX_POSITIONING_TRIES ? position : Vector3.zero;
    }
}
