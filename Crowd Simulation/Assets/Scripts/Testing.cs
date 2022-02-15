using UnityEngine;

public class Testing : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 10f;
    public Vector3 originPosition = Vector3.zero;
    private MyGrid<int> grid;

    private void Start()
    {
        //grid = new MyGrid<GridObject>(gridWidth, gridHeight, cellSize, originPosition, (g, x, y) => new GridObject(g, x, y));
        grid = new MyGrid<int>(gridWidth, gridHeight, cellSize, originPosition);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2Int gridPosition = grid.GetGridPosition(Utilities.GetMouseWorldPosition());
            //GridObject gridObject = grid.GetGridObject(gridPosition);
            //gridObject?.AddValue(1);
            grid.SetGridObject(gridPosition, grid.GetGridObject(gridPosition) + 1);
        }
    }
}

public class GridObject
{
    private MyGrid<GridObject> grid;
    private int value;
    private int x;
    private int y;

    public GridObject(MyGrid<GridObject> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void AddValue(int addedValue)
    {
        value += addedValue;
        grid.TriggerGridObjectChanged(x, y);
    }

    public override string ToString()
    {
        return value.ToString();
    }
}
