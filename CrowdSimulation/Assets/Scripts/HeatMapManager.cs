using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeatMapColor
{
    None,
    Black,
    Red,
    Yellow,
    Green
}

public class HeatMapManager : MonoBehaviour
{
    private const int MAX_VALUE = 100;

    [SerializeField] private bool showUnitHeatMap = false;
    [SerializeField] private bool showObstacleMap = false;

    private bool updateMesh = false;
    private MyGrid<int> grid;
    private Mesh mesh;
    private int maxUnitsOnCell = 0;

    #region Singleton
    public static HeatMapManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<HeatMapManager>();
        }
        return instance;
    }

    private static HeatMapManager instance;
    #endregion

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Start()
    {
        MyGrid<FlowFieldCell> flowFieldGrid = PathingManager.GetInstance().flowField.GetGrid();
        grid = new MyGrid<int>(flowFieldGrid.GetGridWidth(), flowFieldGrid.GetGridHeight(), flowFieldGrid.GetCellSize(),
            flowFieldGrid.GetGridOriginPosition());

        UpdateHeatMap();
        grid.OnCellValueChanged += GridOnCellValueValueChanged;

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        if (showObstacleMap)
        {
            for (int x = 0; x < grid.GetGridWidth(); x++)
            {
                for (int y = 0; y < grid.GetGridHeight(); y++)
                {
                    grid.SetCell(x, y, -1);
                }
            }

            List<Vector2Int> gridPositions = grid.GetGridPositionsWithObjects(GlobalConstants.OBSTACLES_STRING);
            foreach (Vector2Int gridPosition in gridPositions)
            {
                grid.SetCell(gridPosition, 0);
            }
        }
    }

    private void Update()
    {
        if (showUnitHeatMap)
        {
            ShowUnitHeatMap();
        }
    }

    private void LateUpdate()
    {
        if (updateMesh)
        {
            updateMesh = false;
            UpdateHeatMap();
        }
    }

    public MyGrid<int> GetGrid()
    {
        return grid;
    }

    public void SetGrid(MyGrid<int> grid)
    {
        this.grid = grid;

    }

    public int GetColorIndex(HeatMapColor color)
    {
        return color switch
        {
            HeatMapColor.None => -1,
            HeatMapColor.Black => 0,
            HeatMapColor.Red => 2,
            HeatMapColor.Yellow => 50,
            HeatMapColor.Green => 100,
            _ => -1
        };
    }

    public void ColorCell(Vector3 worldPosition, HeatMapColor color)
    {
        grid.SetCell(worldPosition, GetColorIndex(color));
    }
    public void ColorCell(Vector2Int gridPosition, HeatMapColor color)
    {
        grid.SetCell(gridPosition, GetColorIndex(color));
    }
    public void ColorCell(int x, int y, HeatMapColor color)
    {
        grid.SetCell(x, y, GetColorIndex(color));
    }

    private void GridOnCellValueValueChanged(object sender, MyGrid<int>.OnCellValueChangedEventArgs eventArgs)
    {
        //UpdateHeatMap();
        updateMesh = true;
    }

    private void UpdateHeatMap()
    {
        Utilities.CreateEmptyMeshArrays(grid.GetGridWidth() * grid.GetGridHeight(), out Vector3[] vertices, out Vector2[] uv,
            out int[] triangles);

        for (int x = 0; x < grid.GetGridWidth(); x++)
        {
            for (int y = 0; y < grid.GetGridHeight(); y++)
            {
                int gridValue = grid.GetCell(x, y);

                if (gridValue < 0 || gridValue > MAX_VALUE) continue;

                int index = x * grid.GetGridHeight() + y;
                Vector3 quadSize = new Vector3(1, 0, 1) * grid.GetCellSize();
                float gridValueNormalized = (float) gridValue / MAX_VALUE;
                Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);

                Utilities.AddToMeshArrays(vertices, uv, triangles, index, grid.GetCellWorldPosition(x, y) + quadSize * 0.5f, 0f, quadSize, gridValueUV, gridValueUV);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    private void ShowUnitHeatMap()
    {
        List<GameObject> units = UnitManager.GetInstance().unitsInGame;
        int[,] gridArray = new int[grid.GetGridWidth(), grid.GetGridHeight()];

        foreach (GameObject unit in units)
        {
            Vector2Int gridPosition = grid.GetCellGridPosition(unit.transform.position);

            if (gridPosition.x < gridArray.GetLength(0) - 1 && gridPosition.y < gridArray.GetLength(1) - 1)
            {
                gridArray[gridPosition.x, gridPosition.y]++;
            }
        }

        for (int x = 0; x < grid.GetGridWidth(); x++)
        {
            for (int y = 0; y < grid.GetGridWidth(); y++)
            {
                if (grid.GetCell(x, y) == 0) continue; // = obstacle

                if (gridArray[x, y] > 0)
                {
                    if (gridArray[x, y] > maxUnitsOnCell)
                    {
                        maxUnitsOnCell = gridArray[x, y];
                    }

                    grid.SetCell(x, y, Mathf.Clamp(ConvertToColorNumber(gridArray[x, y]), 2, 100));
                }
                else if (gridArray[x, y] == 0) // nothing on the cell, means we don't want to make a vertice for it.
                {
                    grid.SetCell(x, y, -1);
                }
            }
        }
    }

    private int ConvertToColorNumber(int amount)
    {
        return MAX_VALUE - amount * MAX_VALUE / maxUnitsOnCell;
    }


}
