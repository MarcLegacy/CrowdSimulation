using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private int[,] gridArray;
    private MyGrid grid;

    private void Start()
    {
        grid = new MyGrid(20, 10, 10f);

        gridArray = grid.GetGridArray();
    }

    //void OnDrawGizmos()
    //{
    //    for (int x = 0; x < gridArray.GetLength(0); x++)
    //    {
    //        for (int y = 0; y < gridArray.GetLength(1); y++)
    //        {
    //            Handles.Label(grid.GetWorldPosition(x, y), "Text");
    //        }
    //    }
    //}
}
