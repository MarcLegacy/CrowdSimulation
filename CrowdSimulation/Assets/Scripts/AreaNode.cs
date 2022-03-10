using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AreaNode
{
    private List<AStarCell> borderCells;

    public int X { get; }
    public int Y { get; }
    public Vector2Int GridPosition => new Vector2Int(X, Y);
    public AStar AStar { get; private set; }


    public AreaNode(int x, int y)
    {
        X = x;
        Y = y;

        borderCells = new List<AStarCell>();
    }

    public void SetGrid(int width, int height, float cellSize, Vector3 originPosition)
    {
        AStar = new AStar(width, height, cellSize, originPosition);
        MyGrid<AStarCell> aStarGrid = AStar.Grid;

        foreach (AStarCell cell in aStarGrid.GridArray)
        {
            if (cell.X == 0 || cell.X == aStarGrid.Width - 1 || cell.Y == 0 || cell.Y == aStarGrid.Height - 1)
            {
                borderCells.Add(cell);
            }
        }
    }

    public List<AStarCell> GetBorderCells(Directions direction)
    {
        MyGrid<AStarCell> aStarGrid = AStar.Grid;
        List<AStarCell> sideBorderCells = new List<AStarCell>();
        switch (direction)
        {
            case Directions.North:
                foreach (AStarCell borderCell in borderCells)
                {
                    if (borderCell.Y == aStarGrid.Height - 1)
                    {
                        sideBorderCells.Add(borderCell);
                    }
                }

                return sideBorderCells;
            case Directions.East:
                foreach (AStarCell borderCell in borderCells)
                {
                    if (borderCell.X == aStarGrid.Width - 1)
                    {
                        sideBorderCells.Add(borderCell);
                    }
                }

                return sideBorderCells;
            case Directions.South:
                foreach (AStarCell borderCell in borderCells)
                {
                    if (borderCell.Y == 0)
                    {
                        sideBorderCells.Add(borderCell);
                    }
                }

                return sideBorderCells;
            case Directions.West:
                foreach (AStarCell borderCell in borderCells)
                {
                    if (borderCell.X == 0)
                    {
                        sideBorderCells.Add(borderCell);
                    }
                }

                return sideBorderCells;
            case Directions.NorthEast:
                foreach (AStarCell borderCell in borderCells)
                {
                    if (borderCell.Y == aStarGrid.Height - 1 || borderCell.X == aStarGrid.Width - 1)
                    {
                        sideBorderCells.Add(borderCell);
                    }
                }

                return sideBorderCells;
            case Directions.SouthEast:
                foreach (AStarCell borderCell in borderCells)
                {
                    if (borderCell.Y == 0 || borderCell.X == aStarGrid.Width - 1)
                    {
                        sideBorderCells.Add(borderCell);
                    }
                }

                return sideBorderCells;
            case Directions.SouthWest:
                foreach (AStarCell borderCell in borderCells)
                {
                    if (borderCell.Y == 0 || borderCell.X == 0)
                    {
                        sideBorderCells.Add(borderCell);
                    }
                }

                return sideBorderCells;
            case Directions.NorthWest:
                foreach (AStarCell borderCell in borderCells)
                {
                    if (borderCell.Y == aStarGrid.Height - 1 || borderCell.X == 0)
                    {
                        sideBorderCells.Add(borderCell);
                    }
                }

                return sideBorderCells;
            case Directions.All:
                return borderCells;
            default:
                Debug.LogWarning("Passed in invalid " + Directions.None);
                return null;
        }
    }
}
