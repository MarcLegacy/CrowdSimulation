using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

public enum PathingMethod
{
    FlowFieldOnly,
    AreaPathing,
    PortalPathing
}

public enum CellType
{
    Free,
    Unwalkable,
    Spawn,
    Target
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
    [SerializeField] private bool showCalculatedPortalLocations = false;
    [SerializeField] private bool showCalculatedPathParths = false;
    [SerializeField] private bool showExecutionTIme = false;
    [SerializeField] private PathingMethod pathingMethod = PathingMethod.FlowFieldOnly;

    private bool calculateFlowField;
    private Vector3 targetPosition = Vector3.zero;
    private UnitManager unitManager;
    private PortalManager portalManager;
    private List<List<AStarCell>> paths;
    private List<double> pathingTimes;
    private List<PortalNode> portalPathNodes;
    private List<List<Vector3>> calculatedPortalLocations;
    private List<List<Vector3>> calculatedPortalPaths;
    private List<Vector3> targetPositions;

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
            calculatedPortalLocations.Clear();
            calculatedPortalPaths.Clear();
        }
    }
    public FlowField FlowField { get; private set; }
    public AreaMap AreaMap { get; private set; }
    public AStar AStar { get; private set; }
    public HashSet<AreaNode> CheckedAreas { get; private set; }
    public PathingMethod PathingMethod => pathingMethod;
    public Dictionary<Vector2Int, CellType> CellsInfo { get; private set; }

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
        calculatedPortalLocations = new List<List<Vector3>>();
        calculatedPortalPaths = new List<List<Vector3>>();
        CellsInfo = new Dictionary<Vector2Int, CellType>(FlowField.Grid.GridArray.GetLength(0) * FlowField.Grid.GridArray.GetLength(1));
        targetPositions = new List<Vector3>();

        TargetPosition = baseObject.transform.position;

        if (showFlowFieldDebugText) FlowField.Grid.ShowDebugText();

        StartCoroutine(DelayedStart());
    }

    // Because for an unknown reason, the position of the colliders aren't yet set on the position of the gameObject in the same frame.
    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        SetCellData();

        if (targetPositions.Count == 0)
        {
            Debug.LogWarning("No GameObjects set to Layer: " + GlobalConstants.TARGETS_STRING);
        }
        else
        {
            FlowField.CalculateFlowField(FlowField.Grid.GetCell(targetPositions[0]));
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (pathingMethod == PathingMethod.FlowFieldOnly)
            {
                Stopwatch stopWatch = Stopwatch.StartNew();
                FlowField.CalculateFlowField(FlowField.Grid.GetCell(Utilities.GetMouseWorldPosition()));
                stopWatch.Stop();
                if (showExecutionTIme)
                {
                    Debug.Log("FlowField Execution Time: " + Math.Round(stopWatch.Elapsed.TotalMilliseconds, 2) + "ms");
                }
            }
            else
            {
                TargetPosition = Utilities.GetMouseWorldPosition();
            }
        }

        if (calculateFlowField)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            CalculateFlowFieldWithAreas();
            stopWatch.Stop();
            BenchmarkManager.GetInstance().flowFieldExecutionTimes.Add(Math.Round(stopWatch.Elapsed.TotalMilliseconds, 2));
            BenchmarkManager.GetInstance().pathingExecutionTimes.Add(pathingTimes.Sum());

            if (showExecutionTIme)
            {
                Debug.Log("Summed A* Execution Time: " + pathingTimes.Sum() + "ms");
                Debug.Log("FlowField Execution Time: " + Math.Round(stopWatch.Elapsed.TotalMilliseconds, 2) + "ms");
            }
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

        if (calculatedPortalLocations != null && showCalculatedPortalLocations)
        {
            foreach (List<Vector3> calculatedPortalLocation in calculatedPortalLocations)
            {
                Utilities.DrawGizmosPathLines(calculatedPortalLocation);
            }
        }

        if (calculatedPortalPaths != null && showCalculatedPathParths)
        {
            foreach (List<Vector3> calculatedPortalPath in calculatedPortalPaths)
            {
                Utilities.DrawGizmosPathLines(calculatedPortalPath);
            }
        }
    }

    public void StartPathing(Vector3 startPosition, Vector3 targetPosition, out bool success)
    {
        success = false;

        switch (pathingMethod)
        {
            case PathingMethod.AreaPathing:
                StartAreaPathing(startPosition, targetPosition, out success);
                break;
            case PathingMethod.PortalPathing:
                StartAreaPortalPathing(startPosition, targetPosition, out success);
                break;
            case PathingMethod.FlowFieldOnly:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void StartAreaPathing(Vector3 startPosition, Vector3 targetPosition, out bool success)
    {
        MyGrid<AreaNode> areaGrid = AreaMap.Grid;
        MyGrid<AStarCell> aStarGrid = AStar.Grid;
        success = false;

        Stopwatch stopWatch = Stopwatch.StartNew();

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
        stopWatch.Stop();
        pathingTimes.Add(Math.Round(stopWatch.Elapsed.TotalMilliseconds, 2));
    }

    private void StartAreaPortalPathing(Vector3 startPosition, Vector3 targetPosition, out bool success)
    {
        success = false;

        Stopwatch stopWatch = Stopwatch.StartNew();
        portalPathNodes = portalManager.FindPathNodes(startPosition, targetPosition);

        if (portalPathNodes == null) return;

        List<Vector3> portalLocations = new List<Vector3>{startPosition};

        foreach (PortalNode portalNode in portalPathNodes)
        {
            CheckedAreas.Add(portalNode.Portal.AreaA);
            CheckedAreas.Add(portalNode.Portal.AreaB);

            portalLocations.Add(portalNode.Portal.WorldPosition);
        }

        portalLocations.Add(targetPosition);

        calculatedPortalLocations.Add(portalLocations);

        this.targetPosition = targetPosition;
        calculateFlowField = true;

        success = true;
        stopWatch.Stop();
        pathingTimes.Add(Math.Round(stopWatch.Elapsed.TotalMilliseconds, 2));

        List<Vector3> path = new List<Vector3> { targetPosition };
        foreach (Vector3 pathLocation in GetPortalPaths(portalPathNodes))
        {
            path.Add(pathLocation);
        }
        path.Add(startPosition);
        calculatedPortalPaths.Add(path);
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



    private List<Vector3> GetPortalPaths(List<PortalNode> portalNodes)
    {
        List<Vector3> path = new List<Vector3>();

        for (int i = portalNodes.Count - 1; i >= 0; i--)
        {
            var neighborNodes = portalManager.NeighborList[portalNodes[i]];

            if (i > 0)
            {
                foreach (var pathLocation in neighborNodes[portalNodes[i - 1]])
                {
                    path.Add(pathLocation);
                }
            }
        }

        return path;
    }

    private void SetupCellInfo()
    {
        CellsInfo.Clear();
        int layerMask = LayerMask.GetMask(GlobalConstants.OBSTACLES_STRING, GlobalConstants.SPAWNS_STRING, GlobalConstants.TARGETS_STRING);

        for (int x = 0; x < FlowField.Grid.Width; x++)
        {
            for (int y = 0; y < FlowField.Grid.Height; y++)
            {
                Vector3 cellPosition = FlowField.Grid.GetCellCenterWorldPosition(x, y);
                Collider[] colliders =
                    Physics.OverlapBox(cellPosition, Vector3.one * CellSize * 0.5f, Quaternion.identity, layerMask);

                CellType cellType = CellType.Free;

                foreach (Collider collider in colliders)
                {
                    if (collider.gameObject.layer == LayerMask.NameToLayer(GlobalConstants.OBSTACLES_STRING))
                    {
                        cellType = CellType.Unwalkable;
                    }
                    if (collider.gameObject.layer == LayerMask.NameToLayer(GlobalConstants.SPAWNS_STRING))
                    {
                        cellType = CellType.Spawn;
                    }
                    if (collider.gameObject.layer == LayerMask.NameToLayer(GlobalConstants.TARGETS_STRING))
                    {
                        cellType = CellType.Target;
                    }
                }

                CellsInfo.Add(new Vector2Int(x, y), cellType);
            }
        }
    }

    private void SetCellData()
    {
        HeatMapManager heatMapManager = HeatMapManager.GetInstance();
        SetupCellInfo();

        foreach (KeyValuePair<Vector2Int, CellType> cellInfo in CellsInfo)
        {
            switch (cellInfo.Value)
            {
                case CellType.Unwalkable:
                    AStar.Grid.GetCell(cellInfo.Key).isWalkable = false;
                    FlowField.Grid.GetCell(cellInfo.Key).SetUnwalkable();
                    if (heatMapManager.ShowObstacleMap)
                    {
                        heatMapManager.ColorCell(cellInfo.Key, HeatMapColor.Black);
                    }

                    AreaMap.Grid.GetCell(cellInfo.Key.x / AreaSize, cellInfo.Key.y / AreaSize).AStar.Grid
                        .GetCell(cellInfo.Key.x % AreaSize, cellInfo.Key.y % AreaSize).isWalkable = false;
                    break;
                case CellType.Spawn:
                    unitManager.spawnLocations.Add(FlowField.Grid.GetCellCenterWorldPosition(cellInfo.Key));
                    break;
                case CellType.Target:
                    targetPositions.Add(FlowField.Grid.GetCellCenterWorldPosition(cellInfo.Key));
                    break;
            }
        }
    }
}
