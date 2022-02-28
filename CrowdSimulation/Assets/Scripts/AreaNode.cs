using UnityEngine;

public class AreaNode
{
    public int X { get; }
    public int Y { get; }
    public Vector2Int GridPosition => new Vector2Int(X, Y);
    public MyGrid<AStarCell> AStarGrid { get; private set; }

    public AreaNode(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void SetGrid(int width, int height, float cellSize, Vector3 originPosition)
    {
        AStarGrid = new MyGrid<AStarCell>(width, height, cellSize, originPosition, (g, x, y) => new AStarCell(g, x, y));
    }
}
