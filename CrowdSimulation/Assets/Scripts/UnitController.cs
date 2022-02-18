using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    private const float MAP_OFFSET = 3f;

    public GameObject unitObject;
    public int totalUnitsSpawned;
    public int totalUnitsPerSpawn;
    public float unitMoveSpeed;
    public PathingController pathingController;

    private List<GameObject> unitsInGame;

    private void Start()
    {
        unitsInGame = new List<GameObject>();

        //SpawnUnits();
        InvokeRepeating("SpawnUnits", 1f, 1f);
    }

    private void FixedUpdate()
    {
        if (pathingController.flowField == null) return;

        foreach (GameObject unit in unitsInGame)
        {
            Rigidbody rigidBody = unit.GetComponent<Rigidbody>();
            Cell currentCell = pathingController.flowField.GetCell(unit.transform.position);

            Vector3 moveDirection = currentCell != null
                ? new Vector3(currentCell.bestDirection.vector2D.x, 0, currentCell.bestDirection.vector2D.y)
                : Vector3.zero;

            rigidBody.velocity = moveDirection * unitMoveSpeed;
        }
    }

    private void SpawnUnits()
    {
        if (unitsInGame.Count >= totalUnitsSpawned) return;

        MyGrid<Cell> grid = PathingController.GetInstance().flowField.grid;

        for (int i = 0; i < totalUnitsPerSpawn; i++)
        {
            GameObject unit = Instantiate(unitObject);
            unitsInGame.Add(unit);
            unit.transform.parent = transform;
            unit.transform.position = Utilities.GetRandomPosition(grid.GetCellCenterWorldPosition(0, grid.GetGridHeight() - 1),
                grid.GetCellCenterWorldPosition(grid.GetGridWidth() - 1, grid.GetGridHeight() - 1));
        }
    }
}
