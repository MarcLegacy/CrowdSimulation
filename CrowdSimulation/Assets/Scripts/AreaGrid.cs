using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AreaGrid
{
    private readonly MyGrid<AreaNode> grid;

    public AreaGrid(int width, int height, float cellSize, Vector3 originPosition, int areaSize, float areaCellSize)
    {
        grid = new MyGrid<AreaNode>(width, height, cellSize, originPosition, (g, x, y) => new AreaNode(g, x, y));

        for (int x = 0; x < grid.GetGridWidth(); x++)
        {
            for (int y = 0; y < grid.GetGridHeight(); y++)
            {
                grid.GetCell(x, y).SetGrid(areaSize, areaSize, areaCellSize, grid.GetCellWorldPosition(x, y));
            }
        }
    }

    public MyGrid<AreaNode> GetGrid()
    {
        return grid;
    }
}
