using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class UnitManager : MonoBehaviour
{
    public event EventHandler<OnMaxUnitsSpawnedEventArgs> OnMaxUnitSpawned;
    public class OnMaxUnitsSpawnedEventArgs : EventArgs { }

    [SerializeField] private GameObject unitObject;
    [SerializeField] private GameObject baseObject;
    [SerializeField] private int maxUnitsSpawned = 1000;
    [SerializeField] private int numUnitsPerSpawn = 100;
    [SerializeField] private float unitMoveSpeed = 10f;
    [SerializeField] [ReadOnly] private List<GameObject> unitsInGame;

    private PathingManager pathingManager;

    public int NumUnitsPerSpawn => numUnitsPerSpawn;
    public int MaxUnitsSpawned => maxUnitsSpawned;
    public List<GameObject> UnitsInGame => unitsInGame;

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

        StartCoroutine(SpawnUnitCoroutine());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

        }
    }

    private void FixedUpdate()
    {
        if (pathingManager.FlowField == null) return;

        MyGrid<FlowFieldCell> flowFieldGrid = pathingManager.FlowField.Grid;

        for (int i = UnitsInGame.Count - 1; i >= 0; i--)
        {
            GameObject unit = UnitsInGame[i];

            if (flowFieldGrid.GetCellGridPosition(unit.transform.position) ==
                flowFieldGrid.GetCellGridPosition(pathingManager.TargetPosition)) continue;

            if (flowFieldGrid.GetCell(unit.transform.position)?.bestDirection == GridDirection.None)
            {
                if (pathingManager.CheckedAreas.Contains(pathingManager.AreaMap.Grid.GetCell(unit.transform.position))) continue;

                pathingManager.StartAreaPathing(unit.transform.position, pathingManager.TargetPosition, out bool success);

                if (success) continue;

                UnitsInGame.Remove(unit);
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
        MyGrid<FlowFieldCell> grid = PathingManager.GetInstance().FlowField.Grid;
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
            while (positioningTries < GlobalConstants.MAX_POSITIONING_TRIES && pathingManager.FlowField.Grid.GetCell(newPosition).Cost == byte.MaxValue);

            //

            if (positioningTries >= GlobalConstants.MAX_POSITIONING_TRIES) continue;

            GameObject unit = Instantiate(unitObject);
            UnitsInGame.Add(unit);
            unit.transform.parent = transform;
            unit.transform.position = newPosition;
            unit.layer = LayerMask.NameToLayer(GlobalConstants.UNITS_STRING);
        }
    }

    IEnumerator SpawnUnitCoroutine()
    {
        yield return new WaitForSeconds(1f);

        SpawnUnits();

        if (UnitsInGame.Count + numUnitsPerSpawn > maxUnitsSpawned)
        {
            OnMaxUnitSpawned?.Invoke(this, new OnMaxUnitsSpawnedEventArgs());
            yield break;
        }

        StartCoroutine(SpawnUnitCoroutine());
    }
}
