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
    private readonly int height;
    private readonly float cellSize;
    private readonly Vector3 originPosition;
    private readonly TGridObject[,] gridArray;

    public MyGrid(int width, int height, float cellSize, Vector3 originPosition,
        Func<MyGrid<TGridObject>, int, int, TGridObject> createGridObject) : this(width, height, cellSize, originPosition)
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }
    }

    public MyGrid(int width, int height, float cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];
    }

    public void ShowDebug()
    {
        TextMesh[,] debugTextArray = new TextMesh[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                debugTextArray[x, y] = Utilities.CreateWorldText(gridArray[x, y]?.ToString(), null, GetCellCenterWorldPosition(x, y), (int)cellSize * 2, Color.black, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetCellWorldPosition(x, y), GetCellWorldPosition(x, y + 1), Color.black, 100f);
                Debug.DrawLine(GetCellWorldPosition(x, y), GetCellWorldPosition(x + 1, y), Color.black, 100f);
            }
        }

        Debug.DrawLine(GetCellWorldPosition(0, height), GetCellWorldPosition(width, height), Color.black, 100f);
        Debug.DrawLine(GetCellWorldPosition(width, 0), GetCellWorldPosition(width, height), Color.black, 100f);

        OnGridObjectChanged += (sender, eventArgs) =>
        {
            debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
        };
    }

    public int GetGridWidth()
    {
        return width;
    }

    public int GetGridHeight()
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

    public Vector3 GetCellWorldPosition(Vector2Int gridPosition)
    {
        return GetCellWorldPosition(gridPosition.x, gridPosition.y);
    }
    public Vector3 GetCellWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, y) * cellSize + originPosition;
    }

    public Vector2Int GetCellGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        int y = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);

        return GetCellGridPosition(x, y);
    }
    public Vector2Int GetCellGridPosition(int x, int y)
    {
        return new Vector2Int(x, y);
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        return GetGridObject(GetCellGridPosition(worldPosition));
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

        Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": Trying to get a value on (" + x + ", " + y +
                         ") in a grid of size (" + gridArray.GetLength(0) + ", " + gridArray.GetLength(1) + ")");
        return default;
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        SetGridObject(GetCellGridPosition(worldPosition), value);
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
            OnGridObjectChanged?.Invoke(this, new OnGridValueChangedEventArgs {x = x, y = y});
        }
        else
        {
            Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": Trying to set a value on (" + x + ", " + y +
                             ") in a grid of size (" + gridArray.GetLength(0) + ", " + gridArray.GetLength(1) + ")");
        }
    }

    public Vector3 GetCellCenterWorldPosition(Vector3 worldPosition)
    {
        return GetCellCenterWorldPosition(GetCellGridPosition(worldPosition));
    }
    public Vector3 GetCellCenterWorldPosition(Vector2Int gridPosition)
    {
        return GetCellCenterWorldPosition(gridPosition.x, gridPosition.y);
    }
    public Vector3 GetCellCenterWorldPosition(int x, int y)
    {
        return GetCellWorldPosition(x, y) + GetCellCenter();
    }

    public Vector3 GetCellCenter()
    {
        return new Vector3(cellSize, 0, cellSize) * 0.5f;
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
    }
}
