using UnityEngine;

public class FlowFieldCell
{
    public GridDirection bestDirection;

    private byte cost;
    private ushort bestCost;
    private MyGrid<FlowFieldCell> grid;

    // If the cost of the cell is set to an unwalkable cell, it cannot be changed
    public byte Cost
    {
        get => cost;
        set
        {
            if (cost == GlobalConstants.OBSTACLE_COST || value == GlobalConstants.OBSTACLE_COST) return;

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

    public void SetUnwalkable(bool isUnwalkable = true)
    {
        cost = isUnwalkable ? GlobalConstants.OBSTACLE_COST : byte.MinValue;

        grid.TriggerCellChanged(X, Y);
    }

    public override string ToString()
    {
        return cost + "\n" + bestCost;
    }
}
