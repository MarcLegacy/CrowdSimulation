using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 10f;
    [SerializeField] private GameObject mapObject;
    [SerializeField] private HeatMapManager heatMapManager;
    private MyGrid<int> grid;

    private void Start()
    {
        Vector3 originPosition =
            new Vector3(mapObject.transform.position.x - (mapObject.transform.localScale.x * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER),
                mapObject.transform.position.y,
                mapObject.transform.position.z - (mapObject.transform.localScale.z * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER));
        grid = new MyGrid<int>(gridWidth, gridHeight, cellSize, originPosition);



        //Mesh mesh = new Mesh();

        //Vector3[] vertices = new Vector3[4];
        //Vector2[] uv = new Vector2[4];
        //int[] triangles = new int[6];

        //vertices[0] = Grid.GetCellWorldPosition(0, 0);
        //vertices[1] = Grid.GetCellWorldPosition(0, 0) + new Vector3(0f, 0f, CellSize);
        //vertices[2] = Grid.GetCellWorldPosition(0, 0) + new Vector3(CellSize, 0f, CellSize);
        //vertices[3] = Grid.GetCellWorldPosition(0, 0) + new Vector3(CellSize, 0f, 0f);

        //triangles[0] = 0;
        //triangles[1] = 1;
        //triangles[2] = 2;
        //triangles[3] = 0;
        //triangles[4] = 2;
        //triangles[5] = 3;

        //mesh.vertices = vertices;
        ////mesh.uv = uv;
        //mesh.triangles = triangles;

        //GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
