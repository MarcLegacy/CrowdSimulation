using UnityEngine;

public class AreaMap
{
    public MyGrid<AreaNode> Grid { get; }

    public AreaMap(int width, int height, float cellSize, Vector3 originPosition, int areaSize, float areaCellSize)
    {
        Grid = new MyGrid<AreaNode>(width, height, cellSize, originPosition, (x, y) => new AreaNode(x, y));

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                Grid.GetCell(x, y).SetGrid(areaSize, areaSize, areaCellSize, Grid.GetCellWorldPosition(x, y));
            }
        }
    }
}
