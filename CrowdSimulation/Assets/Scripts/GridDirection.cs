// Script made by following Turbo Makes Games tutorial for making a Flow Field
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridDirection
{
    public readonly Vector2Int vector2D;

    public static readonly GridDirection None = new GridDirection(0, 0);
    public static readonly GridDirection North = new GridDirection(0, 1);
    public static readonly GridDirection East = new GridDirection(1, 0);
    public static readonly GridDirection South = new GridDirection(0, -1);
    public static readonly GridDirection West = new GridDirection(-1, 0);
    public static readonly GridDirection NorthEast = new GridDirection(1, 1);
    public static readonly GridDirection SouthEast = new GridDirection(1, -1);
    public static readonly GridDirection SouthWest = new GridDirection(-1, -1);
    public static readonly GridDirection NorthWest = new GridDirection(-1, 1);

    private GridDirection(int x, int y)
    {
        vector2D = new Vector2Int(x, y);
    }

    public static implicit operator Vector2Int(GridDirection direction)
    {
        return direction.vector2D;
    }

    public static GridDirection GetDirection(Vector2Int vector2D)
    {
        //return CardinalAndIntercardinalDirections.DefaultIfEmpty(None).FirstOrDefault(direction => direction == vector2D);
        // Looping through seems to be a lot more efficient.
        foreach (GridDirection gridDirection in CardinalAndIntercardinalDirections)
        {
            if (gridDirection.vector2D == vector2D) return gridDirection;
        }

        return None;
    }

    public static readonly List<GridDirection> CardinalDirections = new List<GridDirection>
    {
        North,
        East,
        South,
        West
    };

    public static readonly List<GridDirection> CardinalAndIntercardinalDirections = new List<GridDirection>
    {
        North,
        East,
        South,
        West,
        NorthEast,
        SouthEast,
        SouthWest,
        NorthWest
    };

    public static readonly List<GridDirection> AllDirections = new List<GridDirection>
    {
        None,
        North,
        East,
        South,
        West,
        NorthEast,
        SouthEast,
        SouthWest,
        NorthWest
    };
}
