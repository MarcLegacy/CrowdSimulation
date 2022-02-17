using System.Collections;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 10f;
    public GameObject mapObject;
    public GameObject baseObject;
    public bool showDebug = false;

    public FlowField flowField;

    private void Start()
    {
        //grid = new MyGrid<GridObject>(gridWidth, gridHeight, cellSize, transform.position, (g, x, y) => new GridObject(g, x, y));
        //grid = new MyGrid<int>(gridWidth, gridHeight, cellSize, originPosition);

        //grid.ShowDebug();
        Vector3 originPosition = new Vector3(mapObject.transform.position.x - (mapObject.transform.localScale.x * 5f),
            mapObject.transform.position.y, mapObject.transform.position.z - (mapObject.transform.localScale.z * 5f));
        flowField = new FlowField(gridWidth, gridHeight, cellSize, originPosition);

        if (showDebug) flowField.ShowDebug();

        StartCoroutine(DelayedSetObstacleScores(flowField.grid, 0.1f));
    }

    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector2Int gridPosition = grid.GetGridPosition(Utilities.GetMouseWorldPosition());
        //    GridObject gridObject = grid.GetGridObject(gridPosition);
        //    gridObject?.AddValue(1);
        //    //grid.SetGridObject(gridPosition, grid.GetGridObject(gridPosition) + 1);
        //}
    }

    // Because for an unknown reason, the code doesn't work inside this Start().
    IEnumerator DelayedSetObstacleScores(MyGrid<Cell> grid, float delayedTime)
    {
        yield return new WaitForSeconds(delayedTime);

        flowField.CalculateFlowField(flowField.GetCell(baseObject.transform.position));
        flowField.DrawArrowField();
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
//        grid.TriggerGridObjectChanged(x, y);
//    }

//    public override string ToString()
//    {
//        return value.ToString();
//    }
//}
