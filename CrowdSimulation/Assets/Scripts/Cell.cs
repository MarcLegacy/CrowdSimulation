using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using UnityEngine;

public class Cell
{
    public Cell cameFromCell;
    public byte cost;
    public ushort bestCost;
    public GridDirection bestDirection;

    private MyGrid<Cell> grid;
    private int x;
    private int y;

    public Cell(MyGrid<Cell> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        cost = 1;
        bestCost = ushort.MaxValue;
        bestDirection = GridDirection.None;
    }

    public override string ToString()
    {
        return cost.ToString() + "\n" + bestCost.ToString();
    }

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(x, y);
    }
}
