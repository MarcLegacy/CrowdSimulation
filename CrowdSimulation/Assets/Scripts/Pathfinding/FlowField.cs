// Script made by following Code Turbo Makes Games's tutorial for making a Flowfield.
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class FlowField
{
    private const int MAX_INTEGRATION_COST = 200;

    public MyGrid<FlowFieldCell> Grid { get; }

    public FlowField(int width, int height, float cellSize, Vector3 originPosition)
    {
        Grid = new MyGrid<FlowFieldCell>(width, height, cellSize, originPosition, (g, x, y) => new FlowFieldCell(g, x, y));
    }

    public void CalculateFlowField(Vector3 worldPosition)
    {
        CalculateFlowField(Grid.GetCell(worldPosition));
    }
    public void CalculateFlowField(Vector2Int gridPosition)
    {
        CalculateFlowField(Grid.GetCell(gridPosition));
    }
    public void CalculateFlowField(int x, int y)
    {
        CalculateFlowField(Grid.GetCell(x, y));
    }
    public void CalculateFlowField(FlowFieldCell destinationCell)
    {
        if (destinationCell == null)
        {
            Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": " + "Trying to calculate towards an invalid AStarCell!");
            return;
        }

        ResetCells();
        CalculateIntegrationField(destinationCell);
        CalculateVectorField();
    }

    public void DrawFlowFieldArrows()
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                GridDirection gridDirection = Grid.GetCell(x, y).bestDirection;

                if (gridDirection != GridDirection.None)
                {
                    Utilities.DrawArrow(Grid.GetCellCenterWorldPosition(x, y),
                        new Vector3(gridDirection.vector2D.x, 0, gridDirection.vector2D.y), Grid.CellSize * 0.5f, Color.black);
                }
            }
        }
    }

    public void ResetCells(bool includeObstacles = false)
    {
        foreach (FlowFieldCell cell in Grid.GridArray)
        {
            if (includeObstacles && cell.Cost == GlobalConstants.OBSTACLE_COST)
            {
                cell.SetUnwalkable(false);
            }

            cell.ResetCell();
        }
    }
    

    public void CalculateCostField(string maskString)
    {
        foreach (FlowFieldCell cell in Grid.GetCellsWithObjects(maskString))
        {
            cell.SetUnwalkable();
        }
    }

    public void CalculateIntegrationField(FlowFieldCell destinationCell)
    {
        destinationCell.Cost = 0;
        destinationCell.BestCost = 0;

        Queue<FlowFieldCell> cellsToCheck = new Queue<FlowFieldCell>();

        cellsToCheck.Enqueue(destinationCell);

        while (cellsToCheck.Count > 0)
        {
            FlowFieldCell currentCell = cellsToCheck.Dequeue();
            List<FlowFieldCell> currentNeighborCells = Grid.GetNeighborCells(currentCell.GridPosition, GridDirection.CardinalDirections);

            foreach (FlowFieldCell currentNeighborCell in currentNeighborCells)
            {
                if (currentNeighborCell.Cost >= MAX_INTEGRATION_COST) continue;    // = everything that should be ignored

                if (currentNeighborCell.Cost + currentCell.BestCost < currentNeighborCell.BestCost)
                {
                    currentNeighborCell.BestCost = (ushort) (currentNeighborCell.Cost + currentCell.BestCost);
                    cellsToCheck.Enqueue(currentNeighborCell);
                }
            }
        }
    }

    public void CalculateVectorField()
    {
        foreach (FlowFieldCell cell in Grid.GridArray)
        {
            List<FlowFieldCell> currentNeighborCells = Grid.GetNeighborCells(cell.GridPosition, GridDirection.CardinalAndIntercardinalDirections);
            int bestCost = cell.BestCost;

            foreach (FlowFieldCell currentNeighborCell in currentNeighborCells)
            {
                if (currentNeighborCell.BestCost < bestCost)
                {
                    bestCost = currentNeighborCell.BestCost;
                    cell.bestDirection =
                        GridDirection.GetDirection(currentNeighborCell.GridPosition - cell.GridPosition);
                }
            }
        }
    }
}
