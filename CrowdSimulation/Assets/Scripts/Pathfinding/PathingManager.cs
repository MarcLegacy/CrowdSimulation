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

    [HideInInspector] public FlowField flowField;
    [HideInInspector] public AreaMap areaMap;
    [HideInInspector] public AStar aStar;

    private List<AreaNode> checkedAreas;

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

        flowField = new FlowField(gridWidth, gridHeight, cellSize, originPosition);
        aStar = new AStar(gridWidth, gridHeight, cellSize, originPosition);
        areaMap = new AreaMap(gridWidth / areaSize, gridHeight / areaSize, cellSize * areaSize, originPosition, areaSize, cellSize);
        checkedAreas = new List<AreaNode>();

        if (showFlowFieldDebugText) flowField.Grid.ShowDebugText();

        StartCoroutine(DelayedStart());
    }

    // Because for an unknown reason, the position of the colliders aren't yet set on the position of the gameObject immediatelly
    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        //flowField.CalculateFlowField(flowField.Grid.GetCell(pathingTargetPosition));
        aStar.SetUnWalkableCells(GlobalConstants.OBSTACLES_STRING);
        flowField.CalculateCostField(GlobalConstants.OBSTACLES_STRING);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

        }
    }

    private void OnDrawGizmos()
    {
        if (flowField != null)
        {
            if (showFlowFieldGrid)
            {
                flowField.Grid.ShowGrid(Color.black);
            }
            if (showFlowFieldArrows)
            {
                flowField.DrawFlowFieldArrows();
            }
        }

        if (areaMap != null)
        {
            MyGrid<AreaNode> grid = areaMap.Grid;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    grid.GetCell(x, y).AStarGrid.ShowGrid(Color.black);
                    //Debug.Log(grid.GetCell(x, y).AStarGrid.GetCell(0, 0).GridPosition);
                }
            }

            //aStar.Grid.ShowGrid(Color.black);
            areaMap.Grid.ShowGrid(Color.red);
        }

        //if (paths != null && !showFlowFieldArrows)
        //{
        //    aStar.DrawPathArrows(paths);
        //}
    }

    public void StartPathing(Vector3 startPosition, Vector3 targetPosition)
    {
        StartPathing(startPosition, targetPosition, out bool _);
    }

    public void StartPathing(Vector3 startPosition, Vector3 targetPosition, out bool success)
    {
        MyGrid<AreaNode> areaGrid = areaMap.Grid;
        MyGrid<AStarCell> aStarGrid = aStar.Grid;
        MyGrid<FlowFieldCell> flowFieldGrid = flowField.Grid;
        //List<AreaNode> areas = new List<AreaNode>();
        success = false;

        Debug.Log("Pathing Started!");

        double startTimer = Time.realtimeSinceStartupAsDouble;

        aStar.ResetCells();
        flowField.ResetCells();

        List<AStarCell> path = aStar.FindPath(startPosition, targetPosition);

        if (path == null) return;

        //foreach (AStarCell aStarCell in path)
        //{
        //    paths.Add(aStarCell);
        //}

        foreach (AStarCell aStarCell in path)
        {
            AreaNode areaNode = areaGrid.GetCell(aStarGrid.GetCellWorldPosition(aStarCell.GridPosition));

            if (!checkedAreas.Contains(areaNode))
            {
                checkedAreas.Add(areaNode);
            }
        }

        foreach (FlowFieldCell flowFieldCell in flowField.Grid.GridArray)
        {
            if (flowFieldCell.Cost != byte.MaxValue)
            {
                flowFieldCell.Cost = IGNORED_CELL;
            }
        }

        foreach (AreaNode area in checkedAreas)
        {
            foreach (AStarCell aStarCell in area.AStarGrid.GridArray)
            {
                FlowFieldCell flowFieldCell = flowFieldGrid.GetCell(area.AStarGrid.GetCellWorldPosition(aStarCell.GridPosition));
                if (flowFieldCell.Cost != byte.MaxValue)
                {
                    flowFieldCell.Cost = 1;
                }
            }
        }

        flowField.CalculateIntegrationField(flowFieldGrid.GetCell(targetPosition));
        flowField.CalculateVectorField();
        success = true;
        Debug.Log("Execution Time: " + (Time.realtimeSinceStartupAsDouble - startTimer) + "s");
    }

    public void SetTargetPosition()
    { 
        flowField.ResetCells();
        checkedAreas.Clear();
    }
}
