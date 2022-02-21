using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class ObstacleDetector : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 10f;
    public GameObject mapObject;
    public bool showDebug = false;

    private MyGrid<int> grid;

    private void Start()
    {
        Vector3 originPosition = new Vector3(mapObject.transform.position.x - (mapObject.transform.localScale.x * 5f),
            mapObject.transform.position.y, mapObject.transform.position.z - (mapObject.transform.localScale.z * 5f));
        grid = new MyGrid<int>(gridWidth, gridHeight, cellSize, originPosition);

        if (showDebug) grid.ShowDebug();

        StartCoroutine(DelayedSetObstacleScores(grid, GlobalConstants.OBSTACLES_STRING, 0.1f));
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Because for an unknown reason, the code doesn't work inside this Start().
    IEnumerator DelayedSetObstacleScores(MyGrid<int> grid, string maskString, float delayedTime)
    {
        yield return new WaitForSeconds(delayedTime);

        SetObstacleScores(grid, maskString);
    }

    private void SetObstacleScores(MyGrid<int> grid, string maskString)
    {
        int[,] gridArray = grid.GetGridArray();
        int terrainMask = LayerMask.GetMask(maskString);

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                Vector3 cellPosition = grid.GetCellCenterWorldPosition(x, y);
                Collider[] obstacles =
                    Physics.OverlapBox(cellPosition, Vector3.one * grid.GetCellSize() * 0.5f, Quaternion.identity, terrainMask);

                if (obstacles.GetLength(0) > 0)
                {
                    grid.SetCell(x, y, 255);
                }
            }
        }
    }
}
