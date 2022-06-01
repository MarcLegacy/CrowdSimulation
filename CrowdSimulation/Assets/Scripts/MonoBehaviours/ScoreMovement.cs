using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ScoreMovement : MonoBehaviour
{
    private EntityManager entityManager;
    private HashSet<Entity> validUnitEntities;
    private Dictionary<int, Vector3> entityVelocities;
    private HashSet<Entity> collidedEntities;
    private List<float> differences;
    private int unitsInGameAmount = 0;
    private double spawnEmptyTime = 0;
    private double targetReachedTime = 0;
    private List<float> differenceAverages;
    private List<float> differenceMaxes;
    private List<int> unitsMoving;
    private List<int> collidedEntitiesAmount;

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

        foreach (Entity entity in entityManager.GetAllEntities())
        {
            if (entityManager.HasComponent<UnitComponent>(entity)) unitsInGameAmount++;
        }

        UnityEngine.Random.InitState(0);
        StartCoroutine(ShowScores());
    }


    void Update()
    {
        if (targetReachedTime != 0) return;

        foreach (Entity entity in validUnitEntities)
        {
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

            MovementForcesComponent movementForcesComponent = entityManager.GetComponentData<MovementForcesComponent>(entity);

            // TODO: not correct
            if (!movementForcesComponent.tempAvoidanceDirection.Equals(float3.zero)) collidedEntities.Add(entity);
        }

        if (validUnitEntities.Count == unitsInGameAmount)
        {
            spawnEmptyTime = Time.realtimeSinceStartupAsDouble;
            Debug.Log("All Units left the Spawn after " + Math.Round(spawnEmptyTime, 2) + "s");

            unitsInGameAmount = 0;  // To make this trigger once
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
        yield return new WaitForSeconds(1f);

        if (targetReachedTime != 0) yield break;

        int numberOfMovingUnits = 0;

        foreach (Entity entity in validUnitEntities)
        {
            if (entityManager.GetComponentData<MoveComponent>(entity).currentSpeed > 0.0f) numberOfMovingUnits++;
        }

        if (validUnitEntities.Count != 0 && numberOfMovingUnits != 0)
        {
            double percentage = Math.Round(numberOfMovingUnits / (double)validUnitEntities.Count * 100f, 2);
            Debug.Log("Units moving: " + numberOfMovingUnits + " of " + validUnitEntities.Count + " (" + percentage + "%)");
            Debug.Log("Unit Velocity Differences: Average: " + Math.Round(differences.Average(), 2) + " Max: " + Math.Round(differences.Max(), 2));
            Debug.Log("Unit amount that collided: " + collidedEntities.Count);

            unitsMoving.Add(numberOfMovingUnits);
            differenceAverages.Add(differences.Average());
            differenceMaxes.Add(differences.Max());
            collidedEntitiesAmount.Add(collidedEntities.Count);
        }

        differences.Clear();
        collidedEntities.Clear();

        StartCoroutine(ShowScores());
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

            Debug.Log("----------------------------------------------------------------");
            Debug.Log("All Units left the Spawn after: " + Math.Round(spawnEmptyTime, 2) + "s");
            Debug.Log("The first unit reached the target at: " + Math.Round(targetReachedTime, 2) + "s");
            Debug.Log("Average Units moving: " + Math.Round(unitsMoving.Average(), 2));
            Debug.Log("Average Unit Velocity Difference: " + Math.Round(differenceAverages.Average(), 2));
            Debug.Log("Max Unit Velocity Difference: " + Math.Round(differenceMaxes.Max(), 2));
            Debug.Log("Average Unit amount collided: " + Math.Round(collidedEntitiesAmount.Average(), 2));
        }
    }
}
