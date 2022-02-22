using UnityEngine;

public class AStarCell
{
    public int fCost;
    public int gCost;
    public int hCost;
    public AStarCell cameFromCell;

    private MyGrid<AStarCell> grid;
    private readonly int x;
    private readonly int y;

    public AStarCell(MyGrid<AStarCell> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(x, y);
    }

    public override string ToString()
    {
        return fCost + "\n" + gCost + "\n" + hCost;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void ResetCell()
    {
        gCost = int.MaxValue;
        CalculateFCost();
        cameFromCell = null;
    }
}
