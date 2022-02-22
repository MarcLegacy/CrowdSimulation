using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMap : MonoBehaviour
{
    private const int MAX_VALUE = 100;

    private MyGrid<int> grid;

    public void SetGrid(MyGrid<int> grid)
    {
        this.grid = grid;
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
                Vector3 quadSize = new Vector3(1, 1) * grid.GetCellSize();
                int gridValue = grid.GetCell(x, y);
                float gridValueNormalized = (float) gridValue / MAX_VALUE;
                Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);

                Utilities.AddToMeshArrays(vertices, uv, triangles, index, grid.GetCellWorldPosition(x, y) + quadSize * 0.5f, 0f, quadSize, Vector2.zero, Vector2.zero);
            }
        }

        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
}
