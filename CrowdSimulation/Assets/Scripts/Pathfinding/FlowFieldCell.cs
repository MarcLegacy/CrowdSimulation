using System;
using UnityEngine;

public class FlowFieldCell
{
    public GridDirection bestDirection;

    private byte cost;
    private ushort bestCost;
    private MyGrid<FlowFieldCell> grid;

    public byte Cost
    {
        get => cost;
        set
        {
            cost = value;
            grid.TriggerCellChanged(X, Y);
        }
    }
    public ushort BestCost
    {
        get => bestCost;
        set
        {
            bestCost = value;
            grid.TriggerCellChanged(X, Y);
        }
    }
    public int X { get; }
    public int Y { get; }
    public Vector2Int GridPosition => new Vector2Int(X, Y);

    public FlowFieldCell(MyGrid<FlowFieldCell> grid, int x, int y)
    {
        this.grid = grid;
        X = x;
        Y = y;

        ResetCell();
    }

    public void ResetCell()
    {
        Cost = 1;
        BestCost = ushort.MaxValue;
        bestDirection = GridDirection.None;
    }

    public override string ToString()
    {
        return cost + "\n" + bestCost;
    }
}
