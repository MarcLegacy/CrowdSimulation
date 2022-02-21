using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class UnitController : MonoBehaviour
{
    public GameObject unitObject;
    public int totalUnitsSpawned = 1000;
    public int numUnitsPerSpawn = 100;
    public float unitMoveSpeed = 10f;
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
            FlowFieldCell currentCell = pathingController.flowField.GetGrid().GetCell(unit.transform.position);

            Vector3 moveDirection = currentCell != null
                ? new Vector3(currentCell.bestDirection.vector2D.x, 0, currentCell.bestDirection.vector2D.y)
                : Vector3.zero;

            rigidBody.velocity = moveDirection * unitMoveSpeed;
        }
    }

    private void SpawnUnits()
    {
        if (unitsInGame.Count + numUnitsPerSpawn > totalUnitsSpawned) return;

        MyGrid<FlowFieldCell> grid = PathingController.GetInstance().flowField.GetGrid();
        int layerMask = LayerMask.GetMask(GlobalConstants.OBSTACLES_STRING);

        for (int i = 0; i < numUnitsPerSpawn; i++)
        {
            int positioningTries = 0;
            Vector3 newPosition;

            do
            {
                newPosition = Utilities.GetRandomPositionInBox(grid.GetCellCenterWorldPosition(0, grid.GetGridHeight() - 1),
                    grid.GetCellCenterWorldPosition(grid.GetGridWidth() - 1, grid.GetGridHeight() - 1));

                positioningTries++;
            } 
            while (positioningTries < GlobalConstants.MAX_POSITIONING_TRIES && Physics.OverlapSphere(newPosition, 0.25f, layerMask).Length > 0);

            if (positioningTries >= GlobalConstants.MAX_POSITIONING_TRIES) continue;

            GameObject unit = Instantiate(unitObject);
            unitsInGame.Add(unit);
            unit.transform.parent = transform;
            unit.transform.position = newPosition;
        }
    }
}
