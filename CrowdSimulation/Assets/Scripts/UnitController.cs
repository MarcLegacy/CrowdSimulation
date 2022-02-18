using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    private const float MAPOFFSET = 3f;

    public GameObject unitObject;
    public GameObject mapObject;
    public int totalUnitsSpawned;
    public int totalUnitsPerSpawn;
    public float unitMoveSpeed;
    public Testing testing;

    private List<GameObject> unitsInGame;

    private void Start()
    {
        unitsInGame = new List<GameObject>();

        //SpawnUnits();
        InvokeRepeating("SpawnUnits", 1f, 1f);
    }

    private void Update()
    {
        if (testing.flowField == null) return;

        foreach (GameObject unit in unitsInGame)
        {
            Cell currentCell = testing.flowField.GetCell(unit.transform.position);
            Vector3 moveDirection = new Vector3(currentCell.bestDirection.vector.x, 0, currentCell.bestDirection.vector.y);
            Rigidbody rigidBody = unit.GetComponent<Rigidbody>();
            rigidBody.velocity = moveDirection * unitMoveSpeed;
        }
    }

    private void SpawnUnits()
    {
        if (unitsInGame.Count >= totalUnitsSpawned) return;

        for (int i = 0; i < totalUnitsPerSpawn; i++)
        {
            GameObject unit = Instantiate(unitObject);
            unitsInGame.Add(unit);
            unit.transform.parent = transform;
            unit.transform.position = new Vector3(Random.Range(mapObject.transform.position.x - (mapObject.transform.localScale.x * 5f) + MAPOFFSET,
                mapObject.transform.position.x + (mapObject.transform.localScale.x * 5f)) - MAPOFFSET, 1f,
                mapObject.transform.position.z + (mapObject.transform.localScale.z * 5f) - MAPOFFSET);
        }
    }
}
