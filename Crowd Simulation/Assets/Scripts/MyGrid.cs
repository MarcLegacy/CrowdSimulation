using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MyGrid
{
    private int width;
    private int height;
    private float cellSize;
    private int[,] gridArray;

    public MyGrid(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;

        gridArray = new int[width, height];
    }

    public int[,] GetGridArray()
    {
        return gridArray;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }
}
