using UnityEngine;

public class Portal
{
    private AStarCell entranceAreaA;
    private AStarCell entranceAreaB;

    public AreaNode AreaNodeA { get; }
    public AreaNode AreaNodeB { get; }
    public AStarCell[] AreaACells { get; }
    public AStarCell[] AreaBCells { get; }

    public Portal(AreaNode areaNodeA, AreaNode areaNodeB, AStarCell[] areaAcells, AStarCell[] areaBcells)
    {
        AreaNodeA = areaNodeA;
        AreaNodeB = areaNodeB;
        AreaACells = areaAcells;
        AreaBCells = areaBcells;

        entranceAreaA = CalculateEntranceCell(areaAcells);
        entranceAreaB = CalculateEntranceCell(areaBcells);
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

    private AStarCell CalculateEntranceCell(AStarCell[] cells)
    {
        Vector2Int totalGridPos = new Vector2Int();
        AStarCell closestCell = null;
        float closestDistance = float.MaxValue;

        foreach (AStarCell cell in cells)
        {
            totalGridPos += cell.GridPosition;
        }

        Vector2 averagePos = totalGridPos / cells.Length;

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
