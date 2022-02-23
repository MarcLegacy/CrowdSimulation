using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingController : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 10f;
    [SerializeField] private GameObject mapObject;
    [SerializeField] private GameObject baseObject;
    [SerializeField] private bool showGridAndCost = false;
    [SerializeField] private bool showArrows = false;

    [HideInInspector] public FlowField flowField;
    [HideInInspector] public AStar AStar;

    private List<AStarCell> path;

    #region Singleton
    public static PathingController GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PathingController>();
        }
        return instance;
    }

    private static PathingController instance;
    #endregion

    private void Start()
    {
        //grid = new MyGrid<GridObject>(gridWidth, gridHeight, cellSize, transform.position, (g, x, y) => new GridObject(g, x, y));
        //grid = new MyGrid<int>(gridWidth, gridHeight, cellSize, originPosition);

        Vector3 originPosition =
            new Vector3(mapObject.transform.position.x - (mapObject.transform.localScale.x * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER),
                mapObject.transform.position.y,
                mapObject.transform.position.z - (mapObject.transform.localScale.z * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER));
        flowField = new FlowField(gridWidth, gridHeight, cellSize, originPosition);

        StartCoroutine(DelayedSetObstacleScores(flowField.GetGrid(), 0.1f));

        AStar = new AStar(gridWidth, gridHeight, cellSize, originPosition);
        StartCoroutine(DelayedCalculateCellCost(GlobalConstants.OBSTACLES_STRING, 0.1f));

        if (showGridAndCost) AStar.GetGrid().ShowDebug();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            flowField.CalculateFlowField(flowField.GetGrid().GetCell(Utilities.GetMouseWorldPosition()));

            path = AStar.FindPath(Vector3.zero, Utilities.GetMouseWorldPosition());
        }
    }

    private void OnDrawGizmos()
    {
        if (showArrows)
        {
            //if (flowField == null) return;

            //MyGrid<FlowFieldCell> grid = flowField.GetGrid();

            //if (grid == null) return;

            //for (int x = 0; x < grid.GetGridWidth(); x++)
            //{
            //    for (int y = 0; y < grid.GetGridHeight(); y++)
            //    {
            //        GridDirection gridDirection = grid.GetCell(x, y).bestDirection;

            //        if (gridDirection != GridDirection.None)
            //        {
            //            Utilities.DrawArrow(grid.GetCellCenterWorldPosition(x, y),
            //                new Vector3(gridDirection.vector2D.x, 0, gridDirection.vector2D.y), grid.GetCellSize() * 0.5f, Color.black);
            //        }
            //    }
            //}

            if (AStar == null) return;

            MyGrid<AStarCell> grid = AStar.GetGrid();

            if (path == null || grid == null) return;

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector2 gridDirection = path[i + 1].GetGridPosition() - path[i].GetGridPosition();

                Utilities.DrawArrow(grid.GetCellCenterWorldPosition(path[i].GetGridPosition()),
                    new Vector3(gridDirection.x, 0f, gridDirection.y), grid.GetCellSize() * 0.5f, Color.black);
            }
        }
    }

    // Because for an unknown reason, the code doesn't work inside this Start().
    IEnumerator DelayedSetObstacleScores(MyGrid<FlowFieldCell> grid, float delayedTime)
    {
        yield return new WaitForSeconds(delayedTime);

        flowField.CalculateFlowField(flowField.GetGrid().GetCell(baseObject.transform.position));
    }

    IEnumerator DelayedCalculateCellCost(string maskString, float delayedTime)
    {
        yield return new WaitForSeconds(delayedTime);

        AStar.SetUnWalkableCells(maskString);
    }

}

//public class GridObject
//{
//    private MyGrid<GridObject> grid;
//    private int value;
//    private int x;
//    private int y;

//    public GridObject(MyGrid<GridObject> grid, int x, int y)
//    {
//        this.grid = grid;
//        this.x = x;
//        this.y = y;
//    }

//    public void AddValue(int addedValue)
//    {
//        value += addedValue;
//        grid.TriggerCellChanged(x, y);
//    }

//    public override string ToString()
//    {
//        return value.ToString();
//    }
//}
