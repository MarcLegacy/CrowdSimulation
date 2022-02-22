using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;

public class AStar
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private readonly MyGrid<AStarCell> grid;
    private List<AStarCell> openList;
    private List<AStarCell> closedList;

    public AStar(int width, int height, float cellSize, Vector3 originPosition)
    {
        grid = new MyGrid<AStarCell>(width, height, cellSize, originPosition, (g, x, y) => new AStarCell(g, x, y));
    }

    public MyGrid<AStarCell> GetGrid()
    {
        return grid;
    }

    public List<AStarCell> FindPath(int startX, int startY, int endX, int endY)
    {
        AStarCell startCell = grid.GetCell(startX, startY);
        AStarCell endCell = grid.GetCell(endX, endY);
        openList = new List<AStarCell> {startCell};
        closedList = new List<AStarCell>();

        ResetCells();
        startCell.gCost = 0;
        startCell.hCost = CalculateHCost(startCell, endCell);
        startCell.CalculateFCost();

        while (openList.Count > 0)
        {
            AStarCell currentCell = GetLowestFCostCell(openList);
            if (currentCell == endCell)
            {
                return CalculatePath(endCell);
            }

            openList.Remove(currentCell);
            closedList.Add(currentCell);

            foreach (AStarCell neighborCell in grid.GetNeighborCells(currentCell.GetGridPosition(),
                GridDirection.CardinalAndIntercardinalDirections))
            {
                if (closedList.Contains(neighborCell)) continue;

                int tentativeGCost = currentCell.gCost + CalculateHCost(currentCell, neighborCell);
                if (tentativeGCost < neighborCell.gCost)
                {
                    neighborCell.cameFromCell = currentCell;
                    neighborCell.gCost = tentativeGCost;
                    neighborCell.hCost = CalculateHCost(neighborCell, endCell);
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

    public void CalculateGCost(string maskString)
    {
        foreach (AStarCell cell in grid.GetCellsWithObjects(maskString))
        {
            cell.gCost = byte.MaxValue;
            grid.TriggerCellChanged(cell.GetGridPosition().x, cell.GetGridPosition().y);
        }
    }

    private void ResetCells()
    {
        for (int x = 0; x < grid.GetGridWidth(); x++)
        {
            for (int y = 0; y < grid.GetGridHeight(); y++)
            {
                grid.GetCell(x, y).ResetCell();
            }
        }
    }

    private int CalculateHCost(AStarCell a, AStarCell b)
    {
        int xDistance = Mathf.Abs(a.GetGridPosition().x - b.GetGridPosition().x);
        int yDistance = Mathf.Abs(a.GetGridPosition().y - b.GetGridPosition().y);
        int remaining = Mathf.Abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private AStarCell GetLowestFCostCell(List<AStarCell> aStarCellList)
    {
        AStarCell lowestFCostNode = aStarCellList[0];
        for (int i = 1; i < aStarCellList.Count; i++)
        {
            if (aStarCellList[i].fCost < lowestFCostNode.fCost)
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
