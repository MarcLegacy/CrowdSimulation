using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

enum PathingMethod
{
    FlowFieldOnly,
    AreaPathing,
    PortalPathing
}

public class PathingManager : MonoBehaviour
{
    private const byte IGNORED_CELL = 254;

    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 10;
    [SerializeField] private int areaSize = 10;
    [SerializeField] private GameObject mapObject;
    [SerializeField] private GameObject baseObject;
    [SerializeField] private bool showFlowFieldDebugText = false;
    [SerializeField] private bool showFlowFieldGrid = false;
    [SerializeField] private bool showFlowFieldArrows = false;
    [SerializeField] private bool showAreaGrid = false;
    [SerializeField] private bool showAStarArrows = false;
    [SerializeField] private PathingMethod pathingMethod = PathingMethod.FlowFieldOnly;

    private bool calculateFlowField;
    private Vector3 targetPosition = Vector3.zero;
    private UnitManager unitManager;
    private PortalManager portalManager;
    private List<List<AStarCell>> paths;
    private List<double> pathingTimes;
    private List<PortalNode> portalPathNodes;
    private List<Vector3> calculatedPortalLocations;
    private List<Vector3> calculatedPathNodeLocations;

    public float CellSize => cellSize;
    public int AreaSize => areaSize;
    public Vector3 TargetPosition
    {
        get => targetPosition;
        set
        {
            FlowFieldCell cell = FlowField.Grid.GetCell(value);
            if (cell == null || cell.Cost == GlobalConstants.OBSTACLE_COST)
            {
                Debug.LogWarning("No valid targetPosition");
                return;
            }

            targetPosition = value;
            FlowField.ResetCells();
            CheckedAreas.Clear();
            paths.Clear();
            pathingTimes.Clear();
        }
    }
    public FlowField FlowField { get; private set; }
    public AreaMap AreaMap { get; private set; }
    public AStar AStar { get; private set; }
    public HashSet<AreaNode> CheckedAreas { get; private set; }

    #region Singleton
    public static PathingManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PathingManager>();
        }
        return instance;
    }

    private static PathingManager instance;
    #endregion

    private void Start()
    {
        Vector3 originPosition =
            new Vector3(mapObject.transform.position.x - (mapObject.transform.localScale.x * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER),
                mapObject.transform.position.y,
                mapObject.transform.position.z - (mapObject.transform.localScale.z * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER));

        FlowField = new FlowField(gridWidth, gridHeight, cellSize, originPosition);
        AStar = new AStar(gridWidth, gridHeight, cellSize, originPosition);
        AreaMap = new AreaMap(gridWidth / areaSize, gridHeight / areaSize, cellSize * areaSize, originPosition, areaSize, cellSize);
        CheckedAreas = new HashSet<AreaNode>();
        paths = new List<List<AStarCell>>();
        pathingTimes = new List<double>();
        unitManager = UnitManager.GetInstance();
        portalManager = PortalManager.GetInstance();

        TargetPosition = baseObject.transform.position;

        if (showFlowFieldDebugText) FlowField.Grid.ShowDebugText();

        unitManager.OnMaxUnitSpawned += OnMaxUnitsSpawned;

        StartCoroutine(DelayedStart());
    }

    // Because for an unknown reason, the position of the colliders aren't yet set on the position of the gameObject in the same frame.
    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        AStar.SetUnWalkableCells(GlobalConstants.OBSTACLES_STRING);
        FlowField.CalculateCostField(GlobalConstants.OBSTACLES_STRING);
        foreach (AreaNode area in AreaMap.Grid.GridArray)
        {
            area.AStar.SetUnWalkableCells(GlobalConstants.OBSTACLES_STRING);
        }

        FlowField.CalculateFlowField(FlowField.Grid.GetCell(baseObject.transform.position));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (pathingMethod == PathingMethod.FlowFieldOnly)
            {
                double startTimer = Time.realtimeSinceStartupAsDouble;
                FlowField.CalculateFlowField(FlowField.Grid.GetCell(Utilities.GetMouseWorldPosition()));
                Debug.Log("FlowField Execution Time: " + Math.Round((Time.realtimeSinceStartupAsDouble - startTimer) * 100000f) * 0.01 + "ms");
            }
            else
            {
                TargetPosition = Utilities.GetMouseWorldPosition();
            }
        }

        if (calculateFlowField)
        {
            if (pathingMethod == PathingMethod.PortalPathing)
            {
                calculatedPortalLocations = new List<Vector3>();
                foreach (PortalNode portalNode in portalPathNodes)
                {
                    calculatedPortalLocations.Add(portalNode.Portal.WorldPosition);

                    //foreach
                }
            }

            Debug.Log("Sum pathing time: " +  Math.Round(pathingTimes.Sum() * 100000f) * 0.01 + "ms");

            double startTimer = Time.realtimeSinceStartupAsDouble;
            CalculateFlowFieldWithAreas();
            Debug.Log("FlowField Execution Time: " + Math.Round((Time.realtimeSinceStartupAsDouble - startTimer) * 100000f) * 0.01 + "ms");
        }
    }

    private void OnDrawGizmos()
    {
        if (FlowField != null)
        {
            if (showFlowFieldGrid)
            {
                FlowField.Grid.ShowGrid(Color.black);
            }
            if (showFlowFieldArrows)
            {
                FlowField.DrawGizmosFlowFieldArrows();
            }
        }

        if (AreaMap != null && showAreaGrid)
        {
            AreaMap.Grid.ShowGrid(Color.red);
        }

        if (paths != null && showAStarArrows)
        {
            ShowAStarPaths();
        }
    }

    public void StartPathing(Vector3 startPosition, Vector3 targetPosition, out bool success)
    {
        success = false;

        if (pathingMethod == PathingMethod.AreaPathing)
        {
            StartAreaPathing(startPosition, targetPosition, out success);
        }
        else
        {
            StartAreaPortalPathing(startPosition, targetPosition, out success);
        }
    }

    private void StartAreaPathing(Vector3 startPosition, Vector3 targetPosition, out bool success)
    {
        MyGrid<AreaNode> areaGrid = AreaMap.Grid;
        MyGrid<AStarCell> aStarGrid = AStar.Grid;
        success = false;

        double startTimer = Time.realtimeSinceStartupAsDouble;

        List<AStarCell> path = AStar.FindPathNodes(startPosition, targetPosition);

        if (path == null) return;

        paths.Add(path);

        foreach (AStarCell aStarCell in path)
        {
            AreaNode areaNode = areaGrid.GetCell(aStarGrid.GetCellWorldPosition(aStarCell.GridPosition));

            CheckedAreas.Add(areaNode);
        }

        this.targetPosition = targetPosition;
        calculateFlowField = true;

        success = true;
        pathingTimes.Add(Time.realtimeSinceStartupAsDouble - startTimer);
    }

    private void StartAreaPortalPathing(Vector3 startPosition, Vector3 targetPosition, out bool success)
    {
        success = false;

        double startTimer = Time.realtimeSinceStartupAsDouble;

        portalPathNodes = portalManager.FindPathNodes(startPosition, targetPosition);

        if (portalPathNodes == null) return;

        foreach (PortalNode portalNode in portalPathNodes)
        {
            CheckedAreas.Add(portalNode.Portal.AreaA);
            CheckedAreas.Add(portalNode.Portal.AreaB);
        }

        this.targetPosition = targetPosition;
        calculateFlowField = true;

        success = true;
        pathingTimes.Add(Time.realtimeSinceStartupAsDouble - startTimer);
    }

    private void CalculateFlowFieldWithAreas()
    {
        calculateFlowField = false;

        FlowField.ResetCells();

        foreach (FlowFieldCell flowFieldCell in FlowField.Grid.GridArray)
        {
            if (CheckedAreas.Contains(AreaMap.Grid.GetCell(FlowField.Grid.GetCellWorldPosition(flowFieldCell.GridPosition))))
            {
                flowFieldCell.Cost = 1;
            }
            else
            {
                flowFieldCell.Cost = IGNORED_CELL;
            }
        }

        FlowField.CalculateIntegrationField(FlowField.Grid.GetCell(targetPosition));
        FlowField.CalculateVectorField();
    }

    private void ShowAStarPaths()
    {
        foreach (List<AStarCell> path in paths)
        {
            AStar.DrawPathArrows(path);
        }
    }

    private void OnMaxUnitsSpawned(object sender, UnitManager.OnMaxUnitsSpawnedEventArgs eventArgs)
    {
        if (!ObstacleSpawnManager.GetInstance().Benchmark) return;

        if (pathingMethod == PathingMethod.FlowFieldOnly)
        {
            StartCoroutine(SetBenchmarkPositionsNormalPathingCoroutine());
        }
        else
        {
            StartCoroutine(SetBenchmarkPositionsWithAreaPathingCoroutine());
        }
    }

    IEnumerator SetBenchmarkPositionsWithAreaPathingCoroutine()
    {
        MyGrid<FlowFieldCell> grid = FlowField.Grid;

        yield return new WaitForSeconds(1f);

        TargetPosition = grid.GetCellCenterWorldPosition(0, 0);

        yield return new WaitForSeconds(1f);

        TargetPosition = grid.GetCellCenterWorldPosition(grid.Width - 1, 0);

        yield return new WaitForSeconds(1f);

        TargetPosition = grid.GetCellCenterWorldPosition(grid.Width - 1, grid.Height - 1);

        yield return new WaitForSeconds(1f);

        TargetPosition = grid.GetCellCenterWorldPosition(0, grid.Height - 1);
    }

    IEnumerator SetBenchmarkPositionsNormalPathingCoroutine()
    {
        MyGrid<FlowFieldCell> grid = FlowField.Grid;

        yield return new WaitForSeconds(1f);

        double startTimer = Time.realtimeSinceStartupAsDouble;
        FlowField.CalculateFlowField(FlowField.Grid.GetCell(0, 0));
        Debug.Log("FlowField Execution Time: " + Math.Round((Time.realtimeSinceStartupAsDouble - startTimer) * 100000f) * 0.01 + "ms");

        yield return new WaitForSeconds(1f);

        startTimer = Time.realtimeSinceStartupAsDouble;
        FlowField.CalculateFlowField(FlowField.Grid.GetCell(grid.Width - 1, 0));
        Debug.Log("FlowField Execution Time: " + Math.Round((Time.realtimeSinceStartupAsDouble - startTimer) * 100000f) * 0.01 + "ms");

        yield return new WaitForSeconds(1f);

        startTimer = Time.realtimeSinceStartupAsDouble;
        FlowField.CalculateFlowField(FlowField.Grid.GetCell(grid.Width - 1, grid.Height - 1));
        Debug.Log("FlowField Execution Time: " + Math.Round((Time.realtimeSinceStartupAsDouble - startTimer) * 100000f) * 0.01 + "ms");

        yield return new WaitForSeconds(1f);

        startTimer = Time.realtimeSinceStartupAsDouble;
        FlowField.CalculateFlowField(FlowField.Grid.GetCell(0, grid.Height - 1));
        Debug.Log("FlowField Execution Time: " + Math.Round((Time.realtimeSinceStartupAsDouble - startTimer) * 100000f) * 0.01 + "ms");
    }
}
