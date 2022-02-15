// Script made by following Code Monkey's tutorial for making a grid system
using System;
using System.Reflection;
using UnityEngine;

public class MyGrid<TGridObject>
{
    public event EventHandler<OnGridValueChangedEventArgs> OnGridObjectChanged;
    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private readonly int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public MyGrid(int width, int height, float cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        bool showDebug = true;
        if (showDebug)
        {
            ShowDebug();
        }
    }

    public MyGrid(int width, int height, float cellSize, Vector3 originPosition, Func<MyGrid<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        bool showDebug = true;
        if (showDebug)
        {
            ShowDebug();
        }
    }

    public void ShowDebug()
    {
        TextMesh[,] debugTextArray = new TextMesh[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                debugTextArray[x, y] = Utilities.CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, 0, cellSize) * 0.5f, (int)cellSize * 2, Color.black, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.black, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.black, 100f);

                Utilities.DrawArrow(GetWorldPosition(x, y) + new Vector3(cellSize, 0, cellSize) * 0.5f, Vector3.right, cellSize * 0.5f, Color.black);
            }
        }

        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.black, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.black, 100f);

        OnGridObjectChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
        {
            debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
        };
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public TGridObject[,] GetGridArray()
    {
        return gridArray;
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return GetWorldPosition(gridPosition.x, gridPosition.y);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, y) * cellSize + originPosition;
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        Vector2Int gridPosition = new Vector2Int
        {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize), 
            y = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize)
        };

        return gridPosition;
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        return GetGridObject(GetGridPosition(worldPosition));
    }

    public TGridObject GetGridObject(Vector2Int gridPosition)
    {
        return GetGridObject(gridPosition.x, gridPosition.y);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": Trying to set a value on (" + x + ", " + y +
                             ") in a grid of size (" + gridArray.GetLength(0) + ", " + gridArray.GetLength(1) + ")");
            return default(TGridObject);
        }
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        SetGridObject(GetGridPosition(worldPosition), value);
    }

    public void SetGridObject(Vector2Int gridPosition, TGridObject value)
    {
        SetGridObject(gridPosition.x, gridPosition.y, value);
    }

    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            gridArray[x, y] = value;
            OnGridObjectChanged?.Invoke(this, new OnGridValueChangedEventArgs() {x = x, y = y});
        }
        else
        {
            Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": Trying to set a value on (" + x + ", " + y +
                             ") in a grid of size (" + gridArray.GetLength(0) + ", " + gridArray.GetLength(1) + ")");
        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridValueChangedEventArgs() { x = x, y = y });
    }
}
