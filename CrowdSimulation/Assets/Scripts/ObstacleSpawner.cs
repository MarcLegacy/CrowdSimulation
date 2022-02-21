using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject mapObject;
    public GameObject baseObject;
    public float avoidanceDistance = 10f;
    public int obstacleAmount = 10;
    public Vector2 obstacleScale = new Vector2(1f, 10f);
    public Color colorA;
    public Color colorB;

    private const float SCALE_TO_SIZE_MULTIPLIER = 5f;

    private void Start()
    {
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
        obstacle.layer = 6;
    }

    private Vector3 FindRandomPosition()
    {
        Vector2 mapGridSize = new Vector2(mapObject.transform.localScale.x * SCALE_TO_SIZE_MULTIPLIER,
            mapObject.transform.localScale.z * SCALE_TO_SIZE_MULTIPLIER);
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
