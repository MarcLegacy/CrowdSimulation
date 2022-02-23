using UnityEngine;

public class AStarCell
{
    public bool isWalkable;
    public readonly int x;
    public readonly int y;
    public int fCost;
    public int gCost;
    public int hCost;
    public AStarCell cameFromCell;

    private MyGrid<AStarCell> grid;

    public AStarCell(MyGrid<AStarCell> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        isWalkable = true;
    }

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(x, y);
    }

    public override string ToString()
    {
        return "g: " + gCost + "\nh: " + hCost + "\nf: " + fCost;
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
