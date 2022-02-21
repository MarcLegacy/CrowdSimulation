using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;

public class AStar
{
    public MyGrid<AStarCell> grid;

    public AStar(int width, int height, float cellSize, Vector3 originPosition)
    {
        grid = new MyGrid<AStarCell>(width, height, cellSize, originPosition, (g, x, y) => new AStarCell(g, x, y));
    }

    public MyGrid<AStarCell> GetGrid()
    {
        return grid;
    }

    public List<Vector3> FindPath(Vector3 currentPosition, Vector3 targetPosition)
    {
        return new List<Vector3>();
    }
}
