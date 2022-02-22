using System.Collections;
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
        }
    }

    private void OnDrawGizmos()
    {
        if (showArrows)
        {
            if (flowField?.GetGrid() == null) return;

            FlowFieldCell[,] gridArray = flowField.GetGrid().GetGridArray();
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    if (gridArray[x, y].bestDirection != GridDirection.None)
                    {
                        Utilities.DrawArrow(flowField.GetGrid().GetCellCenterWorldPosition(x, y),
                            new Vector3(gridArray[x, y].bestDirection.vector2D.x, 0, gridArray[x, y].bestDirection.vector2D.y),
                            flowField.GetGrid().GetCellSize() * 0.5f, Color.black);
                    }
                }
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

        //AStar.CalculateCellCost(maskString);
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
