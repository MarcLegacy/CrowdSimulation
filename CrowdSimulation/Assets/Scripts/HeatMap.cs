using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMap : MonoBehaviour
{
    private const int MAX_VALUE = 100;

    private MyGrid<int> grid;
    private Mesh mesh;

    public void SetGrid(MyGrid<int> grid)
    {
        this.grid = grid;
        mesh = new Mesh();
        UpdateHeatMap();

        grid.OnCellValueChanged += GridOnCellValueValueChanged;
    }

    private void GridOnCellValueValueChanged(object sender, MyGrid<int>.OnCellValueChangedEventArgs eventArgs)
    {
        UpdateHeatMap();
    }

    private void UpdateHeatMap()
    {
        Utilities.CreateEmptyMeshArrays(grid.GetGridWidth() * grid.GetGridHeight(), out Vector3[] vertices, out Vector2[] uv,
            out int[] triangles);

        for (int x = 0; x < grid.GetGridWidth(); x++)
        {
            for (int y = 0; y < grid.GetGridHeight(); y++)
            {
                int index = x * grid.GetGridHeight() + y;
                Vector3 quadSize = new Vector3(1, 0, 1) * grid.GetCellSize();
                int gridValue = grid.GetCell(x, y);
                float gridValueNormalized = (float) gridValue / MAX_VALUE;
                Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);

                Utilities.AddToMeshArrays(vertices, uv, triangles, index, grid.GetCellWorldPosition(x, y) + quadSize * 0.5f, 0f, quadSize, gridValueUV, gridValueUV);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
