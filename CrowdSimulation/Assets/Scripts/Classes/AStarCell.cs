using UnityEngine;

public class AStarCell
{
    public bool isWalkable;
    public bool visited;
    public AStarCell cameFromCell;
    private int fCost;
    private int gCost;
    private int hCost;

    private MyGrid<AStarCell> grid;

    public int X { get; }
    public int Y { get; }
    public int FCost
    {
        get => fCost;
        set
        {
            fCost = value;
            grid.TriggerCellChanged(X, Y);
        }
    }
    public int GCost
    {
        get => gCost;
        set
        {
            gCost = value;
            grid.TriggerCellChanged(X, Y);
        }
    }
    public int HCost
    {
        get => hCost;
        set
        {
            hCost = value;
            grid.TriggerCellChanged(X, Y);
        }
    }
    public Vector2Int GridPosition => new Vector2Int(X, Y);

    public AStarCell(MyGrid<AStarCell> grid, int x, int y)
    {
        this.grid = grid;
        X = x;
        Y = y;
        isWalkable = true;
    }

    public override string ToString()
    {
        return "g: " + gCost + "\nh: " + hCost + "\nf: " + fCost;
    }

    public void CalculateFCost()
    {
        FCost = gCost + hCost;
    }

    public void ResetCell()
    {
        GCost = int.MaxValue;
        CalculateFCost();
        cameFromCell = null;
        visited = false;
    }
}
