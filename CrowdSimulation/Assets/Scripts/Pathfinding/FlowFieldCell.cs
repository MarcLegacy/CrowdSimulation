using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class FlowFieldCell
{
    public byte cost;
    public ushort bestCost;
    public readonly int x;
    public readonly int y;
    public GridDirection bestDirection;

    private MyGrid<FlowFieldCell> grid;

    public FlowFieldCell(MyGrid<FlowFieldCell> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;

        ResetCell();
    }

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(x, y);
    }

    public void ResetCell()
    {
        cost = 1;
        bestCost = ushort.MaxValue;
        bestDirection = GridDirection.None;
    }

    public override string ToString()
    {
        return cost + "\n" + bestCost;
    }
}
