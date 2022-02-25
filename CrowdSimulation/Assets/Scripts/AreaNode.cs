using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AreaNode
{
    private readonly MyGrid<AreaNode> grid;
    private MyGrid<AStarCell> aStarGrid;
    public readonly int x;
    public readonly int y;

    public AreaNode(MyGrid<AreaNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void SetGrid(int width, int height, float cellSize, Vector3 originPosition)
    {
        aStarGrid = new MyGrid<AStarCell>(width, height, cellSize, originPosition);
    }

    public MyGrid<AStarCell> GetGrid()
    {
        return aStarGrid;
    }

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(x, y);
    }
}
