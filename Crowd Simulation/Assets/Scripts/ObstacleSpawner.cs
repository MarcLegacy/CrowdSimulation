using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject mapObject;
    public GameObject baseObject;
    public float avoidanceDistance = 10f;
    public int obstacleAmount = 10;
    public Vector2 obstacleScale = new Vector2(1f, 10f);
    public Color colorA;
    public Color colorB;

    private const float MAPDISTANCE = 5f;
    private const int MAXPOSITIONINGTRIES = 5;

    private void Start()
    {
        for (int i = 0; i < obstacleAmount; i++)
        {
            PositionObstacle(CreateObstacle());
        }
    }

    private GameObject CreateObstacle()
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.transform.localScale =
            new Vector3(Random.Range(obstacleScale.x, obstacleScale.y), 1f, Random.Range(obstacleScale.x, obstacleScale.y));
        obstacle.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        obstacle.GetComponent<MeshRenderer>().materials[0].color = new Color(Random.Range(colorA.r, colorB.r), Random.Range(colorA.g, colorB.g),
            Random.Range(colorA.b, colorB.b));

        return obstacle;
    }

    private void PositionObstacle(GameObject obstacle)
    {
        Vector2 mapGridSize = new Vector2(mapObject.transform.localScale.x * MAPDISTANCE, mapObject.transform.localScale.z * MAPDISTANCE);
        obstacle.transform.position = baseObject.transform.position;
        int positioningTries = 0;

        while (Vector3.Distance(baseObject.transform.position, obstacle.transform.position) < avoidanceDistance && positioningTries < MAXPOSITIONINGTRIES)
        {
            obstacle.transform.position =
                new Vector3(Random.Range(mapObject.transform.position.x - mapGridSize.x, mapObject.transform.position.x + mapGridSize.x), 0,
                    Random.Range(mapObject.transform.position.z - mapGridSize.y, mapObject.transform.position.z + mapGridSize.y));
            positioningTries++;
        }

        if (positioningTries >= MAXPOSITIONINGTRIES)
        {
            Destroy(obstacle);
        }
    }
}
