using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Math = System.Math;

public class ScoreMovement : MonoBehaviour
{
    private const float ACCEPTABLE_COLLISION_RADIUS = 0.75f;
    private const float UNIT_COLLISION_RADIUS = 0.5f;   // Can't seem to find the radius from the PhysicsCollider attached.

    [SerializeField] private bool checkCollisions = false;
    [SerializeField] private float refreshTime = 1.0f;

    private EntityManager entityManager;
    private HashSet<Entity> validUnitEntities;
    private Dictionary<int, Vector3> entityVelocities;
    private HashSet<Entity> collidedEntities;
    private HashSet<Entity> collidedWithObstacles;
    private List<float> differences;
    private int unitsInGameAmount = 0;
    private double spawnEmptyTime = 0;
    private double targetReachedTime = 0;
    private List<float> differenceAverages;
    private List<float> differenceMaxes;
    private List<int> unitsMoving;
    private List<int> collidedEntitiesAmount;
    private List<int> collidedWithObstaclesAmount;
    private float4 colorBlue = new float4(0, 0, 1, 1);
    private float4 colorRed = new float4(1, 0, 0, 1);
    private int frameCounter = 0;
    private float timeCounter = 0.0f;
    private float lastFramerate = 0.0f;
    private List<float> savedFPS;


    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        validUnitEntities = new HashSet<Entity>();
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitGridIndexSystem>().OnSpawnLeft += OnSpawnLeft;
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitGridIndexSystem>().OnTargetReached += OnTargetReached;
        entityVelocities = new Dictionary<int, Vector3>();
        differences = new List<float>();
        differenceAverages = new List<float>();
        differenceMaxes = new List<float>();
        unitsMoving = new List<int>();
        collidedEntities = new HashSet<Entity>();
        collidedEntitiesAmount = new List<int>();
        collidedWithObstacles = new HashSet<Entity>();
        collidedWithObstaclesAmount = new List<int>();
        savedFPS = new List<float>();

        foreach (Entity entity in entityManager.GetAllEntities())
        {
            if (entityManager.HasComponent<UnitComponent>(entity)) unitsInGameAmount++;
        }

        UnityEngine.Random.InitState(0);
        StartCoroutine(ShowScores());
    }


    void Update()
    {
        if (targetReachedTime == 0)
        {
            foreach (Entity entity in validUnitEntities)
            {
                CheckVelocity(entity);
                if (checkCollisions) CheckCollision(entity);
            }

            CheckSpawnTime();

            CountFPS();
        }
    }

    void OnDrawGizmos()
    {
        //if (entityManager != null && validUnitEntities != null)
        //{
        //    Gizmos.color = Color.red;

        //    foreach (Entity entity in validUnitEntities)
        //    {
        //        Vector3 position = entityManager.GetComponentData<Translation>(entity).Value;
        //        Gizmos.DrawRay(position, Vector3.up * 5.0f);
        //    }
        //}
    }

    IEnumerator ShowScores()
    {
        yield return new WaitForSeconds(refreshTime);

        if (targetReachedTime != 0) yield break;

        int numberOfMovingUnits = 0;

        savedFPS.Add(lastFramerate);

        foreach (Entity entity in validUnitEntities)
        {
            if (entityManager.GetComponentData<MoveComponent>(entity).currentSpeed > 0.0f) numberOfMovingUnits++;
        }

        if (validUnitEntities.Count != 0 && numberOfMovingUnits != 0)
        {
            double percentage = Math.Round(numberOfMovingUnits / (double)validUnitEntities.Count * 100f, 2);
            Debug.Log("Units moving: " + numberOfMovingUnits + " of " + validUnitEntities.Count + " (" + percentage + "%)");
            Debug.Log("Unit Velocity Differences: Average: " + Math.Round(differences.Average(), 2) + " Max: " + Math.Round(differences.Max(), 2));
            if (checkCollisions)
            {
                Debug.Log("Unit amount that collided with each other: " + collidedEntities.Count);  // Both units that collided with each other will be counted.
                Debug.Log("Unit amount that collided with obstacles: " + collidedWithObstacles.Count);

                collidedEntitiesAmount.Add(collidedEntities.Count);
                collidedWithObstaclesAmount.Add(collidedWithObstacles.Count);
            }

            unitsMoving.Add(numberOfMovingUnits);
            differenceAverages.Add(differences.Average());
            differenceMaxes.Add(differences.Max());

        }

        differences.Clear();
        collidedEntities.Clear();
        collidedWithObstacles.Clear();

        StartCoroutine(ShowScores());
    }

    private void CheckVelocity(Entity entity)
    {
        if (!entityManager.HasComponent<Translation>(entity)) return;

        Vector3 newVelocity = entityManager.GetComponentData<MoveComponent>(entity).velocity;

        int entityIndex = entity.Index;

        if (!entityVelocities.ContainsKey(entityIndex))
        {
            entityVelocities.Add(entityIndex, newVelocity);
        }
        else
        {
            Vector3 oldVelocity = entityVelocities[entityIndex];

            float difference = math.distance(oldVelocity, newVelocity) / Time.deltaTime;

            differences.Add(difference);

            entityVelocities[entityIndex] = newVelocity;
        }
    }

    private void CheckSpawnTime()
    {
        if (validUnitEntities.Count == unitsInGameAmount)
        {
            spawnEmptyTime = Time.realtimeSinceStartupAsDouble;
            Debug.Log("All Units left the Spawn after " + Math.Round(spawnEmptyTime) + "s");

            unitsInGameAmount = 0;  // To make this trigger once
        }
    }

    private void CheckCollision(Entity entity)
    {
        if (!entityManager.HasComponent<Translation>(entity)) return;

        GridIndexComponent gridIndexComponent = entityManager.GetComponentData<GridIndexComponent>(entity);
        var indexMap = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitGridIndexSystem>().indexMap;
        Vector3 position = entityManager.GetComponentData<Translation>(entity).Value;
        float4 color = colorBlue;

        for (int i = 0; i < 9; i++)
        {
            int2 gridPosition = new int2(-1 + i / 3, -1 + i % 3) + gridIndexComponent.gridPosition; // This makes sure that it also looks to the neighboring cells

            if (indexMap.TryGetFirstValue(gridPosition, out Entity unitEntity, out NativeMultiHashMapIterator<int2> iterator))
            {
                do
                {
                    if (entity == unitEntity) continue;

                    Vector3 unitPosition = entityManager.GetComponentData<Translation>(unitEntity).Value;

                    if (math.distance(position, unitPosition) < ACCEPTABLE_COLLISION_RADIUS)
                    {
                        collidedEntities.Add(entity);
                        color = colorRed;
                    }
                } while (indexMap.TryGetNextValue(out unitEntity, ref iterator));
            }
        }

        if (Physics.CheckSphere(position, UNIT_COLLISION_RADIUS, LayerMask.GetMask(GlobalConstants.OBSTACLES_STRING)))
        {
            collidedWithObstacles.Add(entity);
            color = colorRed;
        }

        entityManager.AddComponentData(entity, new URPMaterialPropertyBaseColor { Value = color });
    }

    private void CountFPS()
    {
        if (timeCounter < refreshTime)
        {
            timeCounter += Time.deltaTime;
            frameCounter++;
        }
        else
        {
            lastFramerate = frameCounter / timeCounter;
            frameCounter = 0;
            timeCounter = 0.0f;
        }
    }

    private void OnSpawnLeft(object sender, UnitGridIndexSystem.OnSpawnLeftEventArgs eventArgs)
    {
        foreach (Entity entity in eventArgs.entities)
        {
            validUnitEntities.Add(entity);
        }
    }

    private void OnTargetReached(object sender, UnitGridIndexSystem.OnTargetReachedEventArgs eventArgs)
    {
        if (targetReachedTime == 0)
        {
            targetReachedTime = Time.realtimeSinceStartupAsDouble;

            double percentage = Math.Round(unitsMoving.Average() / validUnitEntities.Count * 100f, 2);

            Debug.Log("----------------------------------------------------------------");
            Debug.Log("All Units left the Spawn after: " + Math.Round(spawnEmptyTime) + "s");
            Debug.Log("The first unit reached the target at: " + Math.Round(targetReachedTime) + "s");
            Debug.Log("Average Units moving: " + Math.Round(unitsMoving.Average()) + " of " + validUnitEntities.Count + " (" + percentage + "%)");
            Debug.Log("Average Unit Velocity Difference: " + Math.Round(differenceAverages.Average(), 2));
            Debug.Log("Max Unit Velocity Difference: " + Math.Round(differenceMaxes.Max(), 2));
            Debug.Log("Average FPS: " + Math.Round(savedFPS.Average(), 2));
            if (checkCollisions)
            {
                Debug.Log("Average Unit amount that collided with each other: " + Math.Round(collidedEntitiesAmount.Average()));
                Debug.Log("Average Unit amount that collided with obstacles: " + Math.Round(collidedWithObstaclesAmount.Average()));
            }
        }
    }
}
