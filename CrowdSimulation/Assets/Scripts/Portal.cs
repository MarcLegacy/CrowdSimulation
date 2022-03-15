using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Portal
{
    public AStarCell EntranceCellAreaA { get; }
    public AStarCell EntranceCellAreaB { get; }
    public AreaNode AreaA { get; }
    public AreaNode AreaB { get; }
    public List<AStarCell> AreaACells { get; }
    public List<AStarCell> AreaBCells { get; }

    public Portal(AreaNode areaA, AreaNode areaB, AStarCell cellAreaACell, AStarCell cellAreaBCell)
    {
        if (areaA == null || areaB == null)
        {
            Debug.LogWarning(nameof(AreaNode) + " == null");
            return;
        }
        if (cellAreaACell == null || cellAreaBCell == null)
        {
            Debug.LogWarning(nameof(AStarCell) + " == null");
            return;
        }

        AreaA = areaA;
        AreaB = areaB;
        AreaACells = new List<AStarCell>() {cellAreaACell};
        AreaBCells = new List<AStarCell>() {cellAreaBCell};
        EntranceCellAreaA = cellAreaACell;
        EntranceCellAreaB = cellAreaBCell;
    }
    public Portal(AreaNode areaA, AreaNode areaB, List<AStarCell> areaACells, List<AStarCell> areaBCells)
    {
        if (areaA == null || areaB == null)
        {
            Debug.LogWarning(nameof(AreaNode) + " == null");
            return;
        }

        if (areaACells.Count == 0 || areaBCells.Count == 0)
        {
            Debug.LogWarning(nameof(List<AStarCell>) + ".Count == 0");
            return;
        }
        AreaA = areaA;
        AreaB = areaB;
        AreaACells = areaACells;
        AreaBCells = areaBCells;
        EntranceCellAreaA = CalculateEntranceCell(areaACells);
        EntranceCellAreaB = CalculateEntranceCell(areaBCells);
    }

    public AStarCell GetEntranceCell(AreaNode areaNode)
    {
        if (areaNode == null)
        {
            Debug.LogWarning(nameof(AreaNode) + " == null");
            return null;
        }

        if (areaNode == AreaA)
        {
            return EntranceCellAreaA;
        }

        if (areaNode == AreaB)
        {
            return EntranceCellAreaB;
        }

        Debug.LogWarning(nameof(Portal) + " doesn't connect to " + nameof(AreaNode) + ": " + areaNode.GridPosition);
        return null;
    }

    public AreaNode GetAreaNode(AStarCell cell)
    {
        if (cell == null)
        {
            Debug.LogWarning(nameof(AStarCell) + " == null");
            return null;
        }

        if (AreaACells.Contains(cell))
        {
            return AreaA;
        }

        if (AreaBCells.Contains(cell))
        {
            return AreaB;
        }

        Debug.LogWarning(nameof(Portal) + " doesn't contain " + nameof(AStarCell) + ": " + cell.GridPosition);
        return null;
    }

    public List<AStarCell> GetCells(AreaNode areaNode)
    {
        if (areaNode == null)
        {
            Debug.LogWarning(nameof(AreaNode) + " == null");
            return null;
        }

        if (AreaA == areaNode)
        {
            return AreaACells;
        }

        if (AreaB == areaNode)
        {
            return AreaBCells;
        }

        Debug.LogWarning("Portal: (" + this + ") doesn't contain Area: (" + areaNode + ")");
        return null;
    }

    /// <summary> Operation is cheaper when the areaNode is given, but less prone to fail. </summary>
    public Vector3 GetCellWorldPosition(AStarCell cell, AreaNode areaNode = null)
    {
        if (cell == null)
        {
            Debug.LogWarning(nameof(AStarCell) + " == null");
            return Vector3.zero;
        }

        if (areaNode == null)
        {
            return GetAreaNode(cell)?.AStar.Grid.GetCellWorldPosition(cell.GridPosition) ?? Vector3.zero;
        }

        return areaNode.AStar.Grid.GetCellWorldPosition(cell.GridPosition);
    }

    /// <summary> Operation is cheaper when the areaNode is given, but less prone to fail. </summary>
    public Vector3 GetCellCenterWorldPosition(AStarCell cell, AreaNode areaNode = null)
    {
        if (cell == null)
        {
            Debug.LogWarning(nameof(AStarCell) + " == null");
            return Vector3.zero;
        }

        if (areaNode == null)
        {
            return GetAreaNode(cell)?.AStar.Grid.GetCellCenterWorldPosition(cell.GridPosition) ?? Vector3.zero;
        }

        return areaNode.AStar.Grid.GetCellCenterWorldPosition(cell.GridPosition);
    }

    public Vector3 GetEntranceCellWorldPosition(AreaNode areaNode)
    {
        if (areaNode == null)
        {
            Debug.LogWarning(nameof(AreaNode) + " == null");
            return Vector3.zero;
        }

        AStarCell cell = GetEntranceCell(areaNode);
        return cell != null ? areaNode.AStar.Grid.GetCellWorldPosition(cell.GridPosition) : Vector3.zero;
    }
    public Vector3 GetEntranceCellAreaAWorldPosition()
    {
        return AreaA.AStar.Grid.GetCellWorldPosition(EntranceCellAreaA.GridPosition);
    }
    public Vector3 GetEntranceCellAreaBWorldPosition()
    {
        return AreaB.AStar.Grid.GetCellWorldPosition(EntranceCellAreaB.GridPosition);
    }

    public Vector3 GetEntranceCellCenterWorldPosition(AreaNode areaNode)
    {
        if (areaNode == null)
        {
            Debug.LogWarning(nameof(AreaNode) + " == null");
            return Vector3.zero;
        }

        AStarCell cell = GetEntranceCell(areaNode);
        return cell != null ? areaNode.AStar.Grid.GetCellCenterWorldPosition(cell.GridPosition) : Vector3.zero;
    }

    public Vector3 GetPortalWorldPosition()
    {
        return (GetEntranceCellAreaACenterWorldPosition() + GetEntranceCellAreaBCenterWorldPosition()) * 0.5f;
    }

    public Vector3 GetEntranceCellAreaACenterWorldPosition()
    {
        return AreaA.AStar.Grid.GetCellCenterWorldPosition(EntranceCellAreaA.GridPosition);
    }
    public Vector3 GetEntranceCellAreaBCenterWorldPosition()
    {
        return AreaB.AStar.Grid.GetCellCenterWorldPosition(EntranceCellAreaB.GridPosition);
    }

    public bool ContainsArea(Portal otherPortal)
    {
        return ContainsArea(otherPortal.AreaA) || ContainsArea(otherPortal.AreaB);
    }
    public bool ContainsArea(List<AreaNode> areaNodes)
    {
        foreach (AreaNode areaNode in areaNodes)
        {
            return ContainsArea(areaNode);
        }

        return false;
    }
    public bool ContainsArea(AreaNode[] areaNodes)
    {
        foreach (AreaNode areaNode in areaNodes)
        {
            return ContainsArea(areaNode);
        }

        return false;
    }
    public bool ContainsArea(AreaNode areaNode)
    {
        return areaNode == AreaA || areaNode == AreaB;
    }

    public bool ContainsCell(Portal otherPortal)
    {
        return ContainsCell(otherPortal.AreaACells) || ContainsCell(otherPortal.AreaBCells);
    }
    public bool ContainsCell(List<AStarCell> cells)
    {
        foreach (AStarCell cell in cells)
        {
            return ContainsCell(cell);
        }

        return false;
    }
    public bool ContainsCell(AStarCell[] cells)
    {
        foreach (AStarCell cell in cells)
        {
            return ContainsCell(cell);
        }

        return false;
    }
    public bool ContainsCell(AStarCell cell)
    {
        return AreaACells.Contains(cell) || AreaBCells.Contains(cell);
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
