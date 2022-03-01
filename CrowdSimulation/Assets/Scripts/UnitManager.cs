using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private GameObject unitObject;
    [SerializeField] private GameObject baseObject;
    [SerializeField] private int totalUnitsSpawned = 1000;
    [SerializeField] private int numUnitsPerSpawn = 100;
    [SerializeField] private float unitMoveSpeed = 10f;

    public List<GameObject> unitsInGame;

    private Vector3 targetPosition;
    private PathingManager pathingManager;

    #region Singleton
    public static UnitManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<UnitManager>();
        }
        return instance;
    }

    private static UnitManager instance;
    #endregion

    private void Start()
    {
        unitsInGame = new List<GameObject>();
        pathingManager = PathingManager.GetInstance();

        targetPosition = baseObject.transform.position;

        //SpawnUnits();
        InvokeRepeating("SpawnUnits", 1f, 1f);
    }

    private void FixedUpdate()
    {
        if (pathingManager.flowField == null) return;

        MyGrid<FlowFieldCell> flowFieldGrid = pathingManager.flowField.Grid;

        foreach (GameObject unit in unitsInGame)
        {
            if (flowFieldGrid.GetCellGridPosition(unit.transform.position) ==
                flowFieldGrid.GetCellGridPosition(targetPosition)) continue;

            if (flowFieldGrid.GetCell(unit.transform.position).bestDirection == GridDirection.None)
            {
                pathingManager.StartPathing(unit.transform.position, targetPosition, out bool success);

                if (success) continue;

                unitsInGame.Remove(unit);
            }
            else
            {
                Rigidbody rigidBody = unit.GetComponent<Rigidbody>();
                FlowFieldCell currentCell = flowFieldGrid.GetCell(unit.transform.position);

                Vector3 moveDirection = currentCell != null
                    ? new Vector3(currentCell.bestDirection.vector2D.x, 0, currentCell.bestDirection.vector2D.y)
                    : Vector3.zero;

                rigidBody.velocity = moveDirection * unitMoveSpeed;
            }
        }
    }

    private void SpawnUnits()
    {
        if (unitsInGame.Count + numUnitsPerSpawn > totalUnitsSpawned) return;

        MyGrid<FlowFieldCell> grid = PathingManager.GetInstance().flowField.Grid;
        int layerMask = LayerMask.GetMask(GlobalConstants.OBSTACLES_STRING);

        for (int i = 0; i < numUnitsPerSpawn; i++)
        {
            int positioningTries = 0;
            Vector3 newPosition;

            do
            {
                newPosition = Utilities.GetRandomPositionInBox(grid.GetCellCenterWorldPosition(0, grid.Height- 1),
                    grid.GetCellCenterWorldPosition(grid.Width- 1, grid.Height- 1));

                positioningTries++;
            } 
            while (positioningTries < GlobalConstants.MAX_POSITIONING_TRIES && pathingManager.flowField.Grid.GetCell(newPosition).Cost == byte.MaxValue);

            //

            if (positioningTries >= GlobalConstants.MAX_POSITIONING_TRIES) continue;

            GameObject unit = Instantiate(unitObject);
            unitsInGame.Add(unit);
            unit.transform.parent = transform;
            unit.transform.position = newPosition;
            unit.layer = LayerMask.NameToLayer(GlobalConstants.UNITS_STRING);
        }
    }
}
