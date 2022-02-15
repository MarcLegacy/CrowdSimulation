using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 10f;
    public Vector3 originPosition = Vector3.zero;
    private int[,] gridArray;
    private MyGrid grid;

    private void Start()
    {
        grid = new MyGrid(gridWidth, gridHeight, cellSize, originPosition);

        gridArray = grid.GetGridArray();
    }
}
