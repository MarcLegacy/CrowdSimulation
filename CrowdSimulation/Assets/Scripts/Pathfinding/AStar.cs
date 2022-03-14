// Script made by following Code Monkey's tutorial for making A* pathfinding

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class AStar
{
    //private const int MOVE_STRAIGHT_COST = 10;
    //private const int MOVE_DIAGONAL_COST = 14;

    private List<AStarCell> openList;
    //private List<AStarCell> closedList;

    public MyGrid<AStarCell> Grid { get; }

    public List<AStarCell> WalkableCells
    {
        get
        {
            List<AStarCell> walkableCells = new List<AStarCell>();

            foreach (AStarCell cell in Grid.GridArray)
            {
                if (cell.isWalkable)
                {
                    walkableCells.Add(cell);
                }
            }

            return walkableCells;
        }
    }

    public AStar(int width, int height, float cellSize, Vector3 originPosition)
    {
        Grid = new MyGrid<AStarCell>(width, height, cellSize, originPosition, (g, x, y) => new AStarCell(g, x, y));
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        return TransformPathNodesToWorldPositions(FindPathNodes(startWorldPosition, endWorldPosition));
    }
    public List<Vector3> FindPath(Vector2Int startGridPosition, Vector2Int endGridPosition)
    {
        return TransformPathNodesToWorldPositions(FindPathNodes(startGridPosition, endGridPosition));
    }
    public List<Vector3> FindPath(int startX, int startY, int endX, int endY)
    {
        return TransformPathNodesToWorldPositions(FindPathNodes(startX, startY, endX, endY));
    }
    public List<AStarCell> FindPathNodes(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        return FindPathNodes(Grid.GetCellGridPosition(startWorldPosition), Grid.GetCellGridPosition(endWorldPosition));
    }
    public List<AStarCell> FindPathNodes(Vector2Int startGridPosition, Vector2Int endGridPosition)
    {
        return FindPathNodes(startGridPosition.x, startGridPosition.y, endGridPosition.x, endGridPosition.y);
    }
    public List<AStarCell> FindPathNodes(int startX, int startY, int endX, int endY)
    {
        AStarCell startCell = Grid.GetCell(startX, startY);
        AStarCell endCell = Grid.GetCell(endX, endY);

        if (startCell == null)
        {
            Debug.LogWarning("No valid start position");
            return null;
        }

        if (endCell == null)
        {
            Debug.LogWarning("No valid end position");
            return null;
        }

        openList = new List<AStarCell> {startCell};
        //closedList = new List<AStarCell>();

        ResetCells();
        startCell.GCost = 0;
        startCell.HCost = CalculateHCost(startCell, endCell);
        startCell.CalculateFCost();

        while (openList.Count > 0)
        {
            AStarCell currentCell = GetLowestFCostCell(openList);
            if (currentCell == endCell)
            {
                return CalculatePath(endCell);
            }

            openList.Remove(currentCell);
            //closedList.Add(currentCell);
            currentCell.visited = true;

            foreach (AStarCell neighborCell in Grid.GetNeighborCells(currentCell.GridPosition, GridDirection.CardinalDirections))
            {
                //if (closedList.Contains(neighborCell)) continue;
                if (neighborCell.visited) continue;

                if (!neighborCell.isWalkable)
                {
                    //closedList.Add(neighborCell);
                    neighborCell.visited = true;
                    continue;
                }

                int tentativeGCost = currentCell.GCost + CalculateHCost(currentCell, neighborCell);
                if (tentativeGCost < neighborCell.GCost)
                {
                    neighborCell.cameFromCell = currentCell;
                    neighborCell.GCost = tentativeGCost;
                    neighborCell.HCost = CalculateHCost(neighborCell, endCell);
                    neighborCell.CalculateFCost();

                    if (!openList.Contains(neighborCell))
                    {
                        openList.Add(neighborCell);
                    }
                }
            }
        }

        return null;
    }

    public void DrawPathArrows(List<AStarCell> path)
    {
        if (path == null)
        {
            Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": " + "path == null");
            return;
        }

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 gridDirection = path[i + 1].GridPosition- path[i].GridPosition;

            Utilities.DrawGizmosArrow(Grid.GetCellCenterWorldPosition(path[i].GridPosition),
                new Vector3(gridDirection.x, 0f, gridDirection.y), Grid.CellSize * 0.5f, Color.black);
        }
    }

    public void SetUnWalkableCells(string maskString)
    {
        foreach (AStarCell cell in Grid.GetCellsWithObjects(maskString))
        {
            cell.isWalkable = false;
        }
    }

    public void ResetCells(bool includeObstacles = false)
    {
        foreach (AStarCell cell in Grid.GridArray)
        {
            if (!includeObstacles && !cell.isWalkable) continue;

            cell.ResetCell();
        }
    }

    /// <summary> Returns the cell center world positions </summary>
    public List<Vector3> TransformPathNodesToWorldPositions(List<AStarCell> pathNodes)
    {
        if (pathNodes == null || pathNodes.Count == 0) return null;

        List<Vector3> worldPositions = new List<Vector3>(pathNodes.Count);

        foreach (AStarCell cell in pathNodes)
        {
            worldPositions.Add(Grid.GetCellCenterWorldPosition(cell.GridPosition));
        }

        return worldPositions;
    }

    private int CalculateHCost(AStarCell a, AStarCell b)
    {
        int xDistance = Mathf.Abs(a.X - b.X);
        int yDistance = Mathf.Abs(a.Y - b.Y);

        // New
        return xDistance + yDistance;

        // Old
        //int remaining = Mathf.Abs(xDistance - yDistance);

        //return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private AStarCell GetLowestFCostCell(List<AStarCell> aStarCellList)
    {
        AStarCell lowestFCostNode = aStarCellList[0];
        for (int i = 1; i < aStarCellList.Count; i++)
        {
            if (aStarCellList[i].FCost < lowestFCostNode.FCost)
            {
                lowestFCostNode = aStarCellList[i];
            }
        }

        return lowestFCostNode;
    }

    private List<AStarCell> CalculatePath(AStarCell endCell)
    {
        List<AStarCell> path = new List<AStarCell> {endCell};
        AStarCell currentCell = endCell;

        while (currentCell.cameFromCell != null)
        {
            path.Add(currentCell.cameFromCell);
            currentCell = currentCell.cameFromCell;
        }
        path.Reverse();

        return path;
    }

    
}
