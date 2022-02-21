using UnityEngine;

public class AStarCell
{
    public int gCost;
    public int hCost;
    public int fCost;
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
}
