using System.Collections;
using UnityEngine;

public class PathingController : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 10f;
    public GameObject mapObject;
    public GameObject baseObject;
    public bool showGridAndCost = false;
    public bool showArrows = false;

    public FlowField flowField;

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

        //grid.ShowDebug();
        Vector3 originPosition = new Vector3(mapObject.transform.position.x - (mapObject.transform.localScale.x * 5f),
            mapObject.transform.position.y, mapObject.transform.position.z - (mapObject.transform.localScale.z * 5f));
        flowField = new FlowField(gridWidth, gridHeight, cellSize, originPosition);

        if (showGridAndCost) flowField.GetGrid().ShowDebug();

        StartCoroutine(DelayedSetObstacleScores(flowField.GetGrid(), 0.1f));
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
        //flowField.DrawArrowField();
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
