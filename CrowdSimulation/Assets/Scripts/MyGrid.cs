// Script made by following Code Monkey's tutorial for making a Grid system

using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class MyGrid<TGridObject>
{
    public event EventHandler<OnCellValueChangedEventArgs> OnCellValueChanged;
    public class OnCellValueChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private Dictionary<Vector2Int, List<TGridObject>> cardinalNeighborList;
    private Dictionary<Vector2Int, List<TGridObject>> cardinalAndInterCardinalNeighborList;

    public int Width { get; }
    public int Height { get; }
    public float CellSize { get; }
    public Vector3 OriginPosition { get; }
    public TGridObject[,] GridArray { get; }

    public MyGrid(int width, int height, float cellSize, Vector3 originPosition,
        Func<MyGrid<TGridObject>, int, int, TGridObject> createGridObject) : this(width, height, cellSize, originPosition)
    {
        for (int x = 0; x < GridArray.GetLength(0); x++)
        {
            for (int y = 0; y < GridArray.GetLength(1); y++)
            {
                GridArray[x, y] = createGridObject(this, x, y);
            }
        }

        CollectNeighborCells();
    }
    public MyGrid(int width, int height, float cellSize, Vector3 originPosition,
        Func<int, int, TGridObject> createGridObject) : this(width, height, cellSize, originPosition)
    {
        for (int x = 0; x < GridArray.GetLength(0); x++)
        {
            for (int y = 0; y < GridArray.GetLength(1); y++)
            {
                GridArray[x, y] = createGridObject(x, y);
            }
        }

        CollectNeighborCells();
    }
    public MyGrid(int width, int height, float cellSize, Vector3 originPosition)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
        OriginPosition = originPosition;

        GridArray = new TGridObject[width, height];

        cardinalNeighborList = new Dictionary<Vector2Int, List<TGridObject>>();
        cardinalAndInterCardinalNeighborList = new Dictionary<Vector2Int, List<TGridObject>>();
    }

    public void ShowDebugText()
    {
        TextMesh[,] debugTextArray = new TextMesh[Width, Height];

        for (int x = 0; x < GridArray.GetLength(0); x++)
        {
            for (int y = 0; y < GridArray.GetLength(1); y++)
            {
                debugTextArray[x, y] = Utilities.CreateWorldText(GridArray[x, y]?.ToString(), PathingManager.GetInstance().transform,
                    GetCellCenterWorldPosition(x, y), (int) CellSize * 2, Color.black, TextAnchor.MiddleCenter);
            }
        }

        OnCellValueChanged += (sender, eventArgs) =>
        {
            debugTextArray[eventArgs.x, eventArgs.y].text = GridArray[eventArgs.x, eventArgs.y]?.ToString();
        };
    }

    public void ShowGrid(Color? color = null)
    {
        if (color == null) color = Color.black;

        Gizmos.color = (Color) color;
        
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Gizmos.DrawLine(GetCellWorldPosition(x, y), GetCellWorldPosition(x, y + 1));
                Gizmos.DrawLine(GetCellWorldPosition(x, y), GetCellWorldPosition(x + 1, y));
            }
        }

        Gizmos.DrawLine(GetCellWorldPosition(0, Height), GetCellWorldPosition(Width, Height));
        Gizmos.DrawLine(GetCellWorldPosition(Width, 0), GetCellWorldPosition(Width, Height));
    }

    public Vector3 GetCellWorldPosition(Vector2Int gridPosition)
    {
        return GetCellWorldPosition(gridPosition.x, gridPosition.y);
    }
    public Vector3 GetCellWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, y) * CellSize + OriginPosition;
    }

    public Vector2Int GetCellGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - OriginPosition).x / CellSize);
        int y = Mathf.FloorToInt((worldPosition - OriginPosition).z / CellSize);

        return GetCellGridPosition(x, y);
    }
    public Vector2Int GetCellGridPosition(int x, int y)
    {
        return new Vector2Int(x, y);
    }

    public TGridObject GetCell(Vector3 worldPosition)
    {
        return GetCell(GetCellGridPosition(worldPosition));
    }
    public TGridObject GetCell(Vector2Int gridPosition)
    {
        return GetCell(gridPosition.x, gridPosition.y);
    }
    public TGridObject GetCell(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            return GridArray[x, y];
        }

        Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": Trying to get a value on (" + x + ", " + y +
                         ") in a Grid of size (" + GridArray.GetLength(0) + ", " + GridArray.GetLength(1) + ")");
        return default;
    }

    public void SetCell(Vector3 worldPosition, TGridObject value)
    {
        SetCell(GetCellGridPosition(worldPosition), value);
    }
    public void SetCell(Vector2Int gridPosition, TGridObject value)
    {
        SetCell(gridPosition.x, gridPosition.y, value);
    }
    public void SetCell(int x, int y, TGridObject value)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            GridArray[x, y] = value;
            OnCellValueChanged?.Invoke(this, new OnCellValueChangedEventArgs {x = x, y = y});
        }
        else
        {
            Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": Trying to set a value on (" + x + ", " + y +
                             ") in a Grid of size (" + GridArray.GetLength(0) + ", " + GridArray.GetLength(1) + ")");
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
        return new Vector3(CellSize, 0, CellSize) * 0.5f;
    }

    public List<TGridObject> GetNeighborCells(Vector3 worldPosition, List<GridDirection> directions)
    {
        return GetNeighborCells(GetCellGridPosition(worldPosition), directions);
    }
    public List<TGridObject> GetNeighborCells(Vector2Int gridPosition, List<GridDirection> directions)
    {
        return GetNeighborCells(gridPosition.x, gridPosition.y, directions);
    }
    public List<TGridObject> GetNeighborCells(int x, int y, List<GridDirection> directions)
    {
        if (directions.Contains(GridDirection.NorthEast))
        {
            return cardinalAndInterCardinalNeighborList[new Vector2Int(x, y)];
        }

        return cardinalNeighborList[new Vector2Int(x, y)];
    }

    public List<TGridObject> GetCellsWithObjects(string maskString)
    {
        List<TGridObject> cells = new List<TGridObject>();

        foreach(Vector2Int gridPosition in GetGridPositionsWithObjects(maskString))
        {
            cells.Add(GetCell(gridPosition.x, gridPosition.y));
        }

        return cells;
    }

    public List<Vector2Int> GetGridPositionsWithObjects(string maskString)
    {
        List<Vector2Int> gridPositions = new List<Vector2Int>();
        int layerMask = LayerMask.GetMask(maskString);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector3 cellPosition = GetCellCenterWorldPosition(x, y);
                Collider[] obstacles =
                    Physics.OverlapBox(cellPosition, Vector3.one * CellSize * 0.5f, Quaternion.identity, layerMask);

                if (obstacles.Length > 0)
                {
                    gridPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        return gridPositions;
    }

    public void TriggerCellChanged(int x, int y)
    {
        OnCellValueChanged?.Invoke(this, new OnCellValueChangedEventArgs { x = x, y = y });
    }

    private void CollectNeighborCells()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                List<TGridObject> cardinalNeighborCells = new List<TGridObject>();
                List<TGridObject> interCardinalNeighborCells = new List<TGridObject>();

                foreach (GridDirection direction in GridDirection.CardinalAndIntercardinalDirections)
                {
                    Vector2Int neighborPosition = new Vector2Int(x, y) + direction;
                    if (neighborPosition.x >= 0 && neighborPosition.x < Width && neighborPosition.y >= 0 &&
                        neighborPosition.y < Height)
                    {
                        if (GridDirection.CardinalDirections.Contains(direction))
                        {
                            cardinalNeighborCells.Add(GetCell(neighborPosition));
                        }

                        interCardinalNeighborCells.Add(GetCell(neighborPosition));

                    }

                }

                cardinalNeighborList.Add(new Vector2Int(x, y), cardinalNeighborCells);
                cardinalAndInterCardinalNeighborList.Add(new Vector2Int(x, y), interCardinalNeighborCells);
            }
        }
    }
}
