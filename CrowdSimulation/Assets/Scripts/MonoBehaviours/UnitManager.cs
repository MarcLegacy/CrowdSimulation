using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Random = UnityEngine.Random;
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

    [HideInInspector] public List<Vector3> spawnLocations;

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
        spawnLocations = new List<Vector3>();

        StartCoroutine(SpawnUnitCoroutine());
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

                pathingManager.StartPathing(unit.transform.position, pathingManager.TargetPosition, out bool success);

                if (success) continue;

                UnitsInGame.Remove(unit);

                if (UnitsInGame.Count == 0)
                {
                    Debug.LogError("No paths found!");
                }
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
        if (spawnLocations.Count == 0)
        {
            Debug.LogWarning("No GameObjects set to Layer: " + GlobalConstants.SPAWNS_STRING);
            return;
        }

        //MyGrid<FlowFieldCell> m_grid = PathingManager.GetInstance().FlowField.Grid;
        //int layerMask = LayerMask.GetMask(GlobalConstants.OBSTACLES_STRING);

        for (int i = 0; i < numUnitsPerSpawn; i++)
        {
            //int positioningTries = 0;
            Vector3 newPosition = spawnLocations[Random.Range(0, spawnLocations.Count - 1)];

            //do
            //{
            //    newPosition = Utilities.GetRandomPositionInBox(m_grid.GetCellCenterWorldPosition(0, m_grid.Height- 1),
            //        m_grid.GetCellCenterWorldPosition(m_grid.Width- 1, m_grid.Height- 1));

            //    positioningTries++;
            //} 
            //while (positioningTries < GlobalConstants.MAX_POSITIONING_TRIES && m_pathingManager.FlowField.Grid.GetCell(newPosition).Cost == byte.MaxValue);

            //if (positioningTries >= GlobalConstants.MAX_POSITIONING_TRIES) continue;

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
