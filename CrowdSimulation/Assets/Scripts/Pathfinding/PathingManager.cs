using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PathingManager : MonoBehaviour
{
    private const byte IGNORED_CELL = 254;

    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 10f;
    [SerializeField] private int areaSize = 10;
    [SerializeField] private GameObject mapObject;
    [SerializeField] private GameObject baseObject;

    [SerializeField] private bool showFlowFieldDebugText = false;
    [SerializeField] private bool showFlowFieldGrid = false;
    [SerializeField] private bool showFlowFieldArrows = false;

    private bool calculateFlowField;
    private Vector3 targetPosition = Vector3.zero;

    [HideInInspector] public Vector3 TargetPosition
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
        }
    }
    [HideInInspector] public FlowField FlowField { get; private set; }
    [HideInInspector] public AreaMap AreaMap { get; private set; }
    [HideInInspector] public AStar AStar { get; private set; }
    [HideInInspector] public HashSet<AreaNode> CheckedAreas { get; private set; }

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
        TargetPosition = baseObject.transform.position;

        if (showFlowFieldDebugText) FlowField.Grid.ShowDebugText();

        StartCoroutine(DelayedStart());
    }

    // Because for an unknown reason, the position of the colliders aren't yet set on the position of the gameObject in the same frame.
    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        AStar.SetUnWalkableCells(GlobalConstants.OBSTACLES_STRING);
        FlowField.CalculateCostField(GlobalConstants.OBSTACLES_STRING);

        FlowField.CalculateFlowField(FlowField.Grid.GetCell(baseObject.transform.position));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //double startTimer = Time.realtimeSinceStartupAsDouble;
            //FlowField.CalculateFlowField(FlowField.Grid.GetCell(Utilities.GetMouseWorldPosition()));


            TargetPosition = Utilities.GetMouseWorldPosition();
            //Debug.Log("FlowField Execution Time: " + (Time.realtimeSinceStartupAsDouble - startTimer) * 1000 + "ms");
        }

        if (calculateFlowField)
        {
            Debug.Log("Calculating FlowField");

            double startTimer = Time.realtimeSinceStartupAsDouble;

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

            Debug.Log("CheckedAreas.Count: " + CheckedAreas.Count);
            Debug.Log("FlowField Execution Time: " + (Time.realtimeSinceStartupAsDouble - startTimer) * 1000 + "ms");
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
                FlowField.DrawFlowFieldArrows();
            }
        }

        if (AreaMap != null)
        {
            MyGrid<AreaNode> grid = AreaMap.Grid;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    grid.GetCell(x, y).AStarGrid.ShowGrid(Color.black);
                    //Debug.Log(grid.GetCell(x, y).AStarGrid.GetCell(0, 0).GridPosition);
                }
            }

            //AStar.Grid.ShowGrid(Color.black);
            AreaMap.Grid.ShowGrid(Color.red);
        }

        //if (paths != null && !showFlowFieldArrows)
        //{
        //    AStar.DrawPathArrows(paths);
        //}
    }

    public void StartPathing(Vector3 startPosition, Vector3 targetPosition)
    {
        StartPathing(startPosition, targetPosition, out bool _);
    }

    public void StartPathing(Vector3 startPosition, Vector3 targetPosition, out bool success)
    {
        MyGrid<AreaNode> areaGrid = AreaMap.Grid;
        MyGrid<AStarCell> aStarGrid = AStar.Grid;
        success = false;

        Debug.Log("Pathing Started!");

        double startTimer = Time.realtimeSinceStartupAsDouble;

        List<AStarCell> path = AStar.FindPath(startPosition, targetPosition);

        if (path == null) return;

        foreach (AStarCell aStarCell in path)
        {
            AreaNode areaNode = areaGrid.GetCell(aStarGrid.GetCellWorldPosition(aStarCell.GridPosition));

            CheckedAreas.Add(areaNode);
        }

        this.targetPosition = targetPosition;
        calculateFlowField = true;

        success = true;
        Debug.Log("Pathing Execution Time: " + (Time.realtimeSinceStartupAsDouble - startTimer) * 1000 + "ms");
    }
}
