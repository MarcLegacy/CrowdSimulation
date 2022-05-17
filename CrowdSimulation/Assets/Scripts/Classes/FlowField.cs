// Flowfield Algorithm made by following Turbo Makes Games's tutorial "Tutorial - Flow Field Pathfinding in Unity": https://www.youtube.com/watch?v=tSe6ZqDKB0Y

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FlowField
{
    public event EventHandler<OnGridDirectionChangedEventArgs> OnGridDirectionChanged;
    public class OnGridDirectionChangedEventArgs : EventArgs
    {
        public MyGrid<FlowFieldCell> grid;
    }

    private const int MAX_INTEGRATION_COST = 200;

    public MyGrid<FlowFieldCell> Grid { get; }

    public FlowField(int width, int height, float cellSize, Vector3 originPosition)
    {
        Grid = new MyGrid<FlowFieldCell>(width, height, cellSize, originPosition, (g, x, y) => new FlowFieldCell(g, x, y));
    }

    public void CalculateFlowField(Vector3 targetWorldPosition)
    {
        CalculateFlowField(Grid.GetCell(targetWorldPosition));
    }
    public void CalculateFlowField(Vector2Int targetGridPosition)
    {
        CalculateFlowField(Grid.GetCell(targetGridPosition));
    }
    public void CalculateFlowField(int x, int y)
    {
        CalculateFlowField(Grid.GetCell(x, y));
    }
    public void CalculateFlowField(FlowFieldCell targetCell)
    {
        if (targetCell == null)
        {
            Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": " + "Trying to calculate towards an invalid AStarCell!");
            return;
        }

        ResetCells();
        CalculateIntegrationField(targetCell);
        CalculateVectorField();
    }

    public void DrawGizmosFlowFieldArrows()
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                GridDirection gridDirection = Grid.GetCell(x, y).bestDirection;

                if (gridDirection != GridDirection.None)
                {
                    Utilities.DrawGizmosArrow(Grid.GetCellCenterWorldPosition(x, y),
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

        OnGridDirectionChanged?.Invoke(this, new OnGridDirectionChangedEventArgs { grid = Grid });
    }
}
