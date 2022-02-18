using UnityEngine;

public class Cell
{
    public byte cost;
    public ushort bestCost;
    public GridDirection bestDirection;

    private MyGrid<Cell> grid;
    private readonly int x;
    private readonly int y;

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
        return cost + "\n" + bestCost;
    }

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(x, y);
    }
}
