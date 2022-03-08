using System.Collections.Generic;
using UnityEngine;

public class Portal
{
    private AStarCell entranceAreaA;
    private AStarCell entranceAreaB;

    public AreaNode AreaNodeA { get; }
    public AreaNode AreaNodeB { get; }
    public List<AStarCell> AreaACells { get; }
    public List<AStarCell> AreaBCells { get; }

    public Portal(AreaNode areaNodeA, AreaNode areaNodeB, AStarCell areaACell, AStarCell areaBCell)
    {
        AreaNodeA = areaNodeA;
        AreaNodeB = areaNodeB;
        AreaACells = new List<AStarCell>() {areaACell};
        AreaBCells = new List<AStarCell>() {areaBCell};
        entranceAreaA = areaACell;
        entranceAreaB = areaBCell;
    }

    public Portal(AreaNode areaNodeA, AreaNode areaNodeB, List<AStarCell> areaACells, List<AStarCell> areaBCells)
    {
        AreaNodeA = areaNodeA;
        AreaNodeB = areaNodeB;
        AreaACells = areaACells;
        AreaBCells = areaBCells;
        entranceAreaA = CalculateEntranceCell(areaACells);
        entranceAreaB = CalculateEntranceCell(areaBCells);
    }

    public AStarCell GetEntranceCell(AreaNode areaNode)
    {
        if (areaNode == AreaNodeA)
        {
            return entranceAreaA;
        }

        if (areaNode == AreaNodeB)
        {
            return entranceAreaB;
        }

        Debug.LogWarning("This portal does not connect to this area!");
        return null;
    }

    private AStarCell CalculateEntranceCell(List<AStarCell> cells)
    {
        Vector2Int totalGridPos = new Vector2Int();
        AStarCell closestCell = null;
        float closestDistance = float.MaxValue;

        foreach (AStarCell cell in cells)
        {
            totalGridPos += cell.GridPosition;
        }

        Vector2 averagePos = totalGridPos / cells.Count;

        foreach (AStarCell cell in cells)
        {
            float distance = Vector2.Distance(cell.GridPosition, averagePos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCell = cell;
            }
        }

        return closestCell;
    }
}
