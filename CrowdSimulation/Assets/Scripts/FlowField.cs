using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class FlowField
{
    private const string MASKSTRING = "Obstacles";

    public MyGrid<Cell> grid;

    public FlowField(int width, int height, float cellSize, Vector3 originPosition)
    {
        grid = new MyGrid<Cell>(width, height, cellSize, originPosition, (g, x, y) => new Cell(g, x, y));
    }

    public void ShowDebug()
    {
        grid.ShowDebug();
    }

    public void CalculateFlowField(Cell destinationCell)
    {
        CreateCostField(MASKSTRING);
        CreateIntegrationField(destinationCell);
        CreateVectorField();
    }

    private void CreateCostField(string maskString)
    {
        Cell[,] gridArray = grid.GetGridArray();
        int terrainMask = LayerMask.GetMask(maskString);

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                Vector3 cellPosition = grid.GetCellCenterPosition(x, y);
                Collider[] obstacles =
                    Physics.OverlapBox(cellPosition, Vector3.one * grid.GetCellSize() * 0.5f, Quaternion.identity, terrainMask);

                if (obstacles.GetLength(0) > 0)
                {
                    gridArray[x, y].cost = byte.MaxValue;
                    grid.TriggerGridObjectChanged(x, y);
                }
            }
        }
    }

    private void CreateIntegrationField(Cell destinationCell)
    {
        destinationCell.cost = 0;
        destinationCell.bestCost = 0;

        Queue<Cell> cellsToCheck = new Queue<Cell>();

        cellsToCheck.Enqueue(destinationCell);

        while (cellsToCheck.Count > 0)
        {
            Cell currentCell = cellsToCheck.Dequeue();
            List<Cell> currentNeighborCells = GetNeighborCells(currentCell, GridDirection.CardinalDirections);
            foreach (Cell currentNeighborCell in currentNeighborCells)
            {
                if (currentNeighborCell.cost == byte.MaxValue) continue;

                if (currentNeighborCell.cost + currentCell.bestCost < currentNeighborCell.bestCost)
                {
                    currentNeighborCell.bestCost = (ushort) (currentNeighborCell.cost + currentCell.bestCost);
                    cellsToCheck.Enqueue(currentNeighborCell);
                }
            }

            grid.TriggerGridObjectChanged(currentCell.GetGridPosition().x, currentCell.GetGridPosition().y);
        }
    }

    private void CreateVectorField()
    {
        Cell[,] gridArray = grid.GetGridArray();

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                Cell currentCell = gridArray[x, y];
                List<Cell> currentNeighborCells = GetNeighborCells(currentCell, GridDirection.AllDirections);

                int bestCost = currentCell.bestCost;

                foreach (Cell currentNeighborCell in currentNeighborCells)
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

    public void DrawArrowField()
    {
        Cell[,] gridArray = grid.GetGridArray();

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                if (gridArray[x, y].bestDirection != GridDirection.None)
                {
                    Utilities.DrawArrow(grid.GetCellCenterPosition(x, y),
                        new Vector3(gridArray[x, y].bestDirection.vector.x, 0, gridArray[x, y].bestDirection.vector.y), grid.GetCellSize() * 0.5f,
                        Color.black);
                }
            }
        }
    }

    public Cell GetCell(Vector3 worldPosition)
    {
        return grid.GetGridObject(worldPosition);
    }

    public Cell GetCell(Vector2Int gridPosition)
    {
        return grid.GetGridObject(gridPosition);
    }

    public Cell GetCell(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<Cell> GetNeighborCells(Cell cell, List<GridDirection> directions)
    {
        List<Cell> neighborCells = new List<Cell>();

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPosition = cell.GetGridPosition() + direction;
            if (neighborPosition.x >= 0 && neighborPosition.x < grid.GetWidth() && neighborPosition.y >= 0 &&
                neighborPosition.y < grid.GetHeight())
            {
                neighborCells.Add(grid.GetGridObject(neighborPosition));
            }
        }

        return neighborCells;
    }
}
