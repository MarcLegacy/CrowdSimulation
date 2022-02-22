using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class FlowField
{
    private readonly MyGrid<FlowFieldCell> grid;

    public FlowField(int width, int height, float cellSize, Vector3 originPosition)
    {
        grid = new MyGrid<FlowFieldCell>(width, height, cellSize, originPosition, (g, x, y) => new FlowFieldCell(g, x, y));
    }

    public MyGrid<FlowFieldCell> GetGrid()
    {
        return grid;
    }

    public void CalculateFlowField(FlowFieldCell destinationCell)
    {
        if (destinationCell == null)
        {
            Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": " + "Trying to calculate towards an invalid AStarCell!");
            return;
        }

        double startTimer = Time.realtimeSinceStartupAsDouble;
        ResetCells();
        CalculateCostField(GlobalConstants.OBSTACLES_STRING);
        CalculateIntegrationField(destinationCell);
        CalculateVectorField();
        Debug.Log("Execution Time: " + (Time.realtimeSinceStartupAsDouble - startTimer) + "s");
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

    private void CalculateCostField(string maskString)
    {
        foreach (FlowFieldCell cell in grid.GetCellsWithObjects(maskString))
        {
            cell.cost = byte.MaxValue;
            grid.TriggerCellChanged(cell.GetGridPosition().x, cell.GetGridPosition().y);
        }
    }

    private void CalculateIntegrationField(FlowFieldCell destinationCell)
    {
        destinationCell.cost = 0;
        destinationCell.bestCost = 0;

        Queue<FlowFieldCell> cellsToCheck = new Queue<FlowFieldCell>();

        cellsToCheck.Enqueue(destinationCell);

        while (cellsToCheck.Count > 0)
        {
            FlowFieldCell currentCell = cellsToCheck.Dequeue();
            List<FlowFieldCell> currentNeighborCells = grid.GetNeighborCells(currentCell.GetGridPosition(), GridDirection.CardinalDirections);

            foreach (FlowFieldCell currentNeighborCell in currentNeighborCells)
            {
                if (currentNeighborCell.cost == byte.MaxValue) continue;    // = obstacle

                if (currentNeighborCell.cost + currentCell.bestCost < currentNeighborCell.bestCost)
                {
                    currentNeighborCell.bestCost = (ushort) (currentNeighborCell.cost + currentCell.bestCost);
                    cellsToCheck.Enqueue(currentNeighborCell);
                }
            }

            grid.TriggerCellChanged(currentCell.GetGridPosition().x, currentCell.GetGridPosition().y);
        }
    }

    private void CalculateVectorField()
    {
        for (int x = 0; x < grid.GetGridWidth(); x++)
        {
            for (int y = 0; y < grid.GetGridHeight(); y++)
            {
                FlowFieldCell currentCell = grid.GetCell(x, y);
                List<FlowFieldCell> currentNeighborCells = grid.GetNeighborCells(currentCell.GetGridPosition(), GridDirection.AllDirections);
                int bestCost = currentCell.bestCost;

                foreach (FlowFieldCell currentNeighborCell in currentNeighborCells)
                {
                    if (currentNeighborCell.bestCost < bestCost)
                    {
                        bestCost = currentNeighborCell.bestCost;
                        currentCell.bestDirection =
                            GridDirection.GetDirection(currentNeighborCell.GetGridPosition() - currentCell.GetGridPosition());
                    }
                }
            }
        }
    }
}
