using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ScoreMovement : MonoBehaviour
{
    private EntityManager entityManager;
    private HashSet<Entity> validUnitEntities;
    private Dictionary<int, Vector3> entityVelocities;

    // Start is called before the first frame update
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        validUnitEntities = new HashSet<Entity>();
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitGridIndexSystem>().OnSpawnLeft += OnSpawnLeft;
        entityVelocities = new Dictionary<int, Vector3>();

        StartCoroutine(ShowScores());
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Entity entity in validUnitEntities)
        {
            Vector3 velocity = entityManager.GetComponentData<MoveComponent>(entity).velocity;
            int entityIndex = entity.Index;

            if (!entityVelocities.ContainsKey(entityIndex))
            {
                entityVelocities.Add(entityIndex, velocity);
            }
            else
            {
                float difference = math.abs(velocity.x - entityVelocities[entityIndex].x) + math.abs(velocity.z - entityVelocities[entityIndex].z);

                Debug.Log("Old Velocity: " + entityVelocities[entityIndex] + " New Velocity: " + velocity + " Difference: " + difference);

                entityVelocities[entityIndex] = velocity;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (Entity entity in validUnitEntities)
        {
            Vector3 position = entityManager.GetComponentData<Translation>(entity).Value;
            Gizmos.DrawRay(position, Vector3.up * 5.0f);
        }
    }

    IEnumerator ShowScores()
    {
        yield return new WaitForSeconds(1f);

        int numberOfMovingUnits = 0;

        foreach (Entity entity in validUnitEntities)
        {
            if (entityManager.GetComponentData<MoveComponent>(entity).currentSpeed > 0.0f) numberOfMovingUnits++;
        }

        if (validUnitEntities.Count != 0 && numberOfMovingUnits != 0)
        {
            double percentage = Math.Round(numberOfMovingUnits / (double)validUnitEntities.Count * 100f, 2);
            Debug.Log("Units moving: " + numberOfMovingUnits + " of " + validUnitEntities.Count + " (" + percentage + "%)");
        }

        StartCoroutine(ShowScores());
    }

    private void OnSpawnLeft(object sender, UnitGridIndexSystem.OnSpawnLeftEventArgs eventArgs)
    {
        foreach (Entity entity in eventArgs.entities)
        {
            validUnitEntities.Add(entity);
        }
    }
}
