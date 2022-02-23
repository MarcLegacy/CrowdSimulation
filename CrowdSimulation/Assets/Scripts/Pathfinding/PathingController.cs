using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingController : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 10f;
    [SerializeField] private int areaSize = 10;
    [SerializeField] private GameObject mapObject;
    [SerializeField] private GameObject baseObject;
    [SerializeField] private bool showFlowFieldDebugText = false;
    [SerializeField] private bool showFlowFieldGrid = false;
    [SerializeField] private bool showFlowFieldArrows = false;
    [SerializeField] private bool showAStarDebugText = false;
    [SerializeField] private bool showStarArrows = false;
    [SerializeField] private bool showAStarGrid = false;

    [HideInInspector] public FlowField flowField;
    [HideInInspector] public AStar AStar;

    private List<AStarCell> path;

    #region Singleton
    public static PathingController GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PathingController>();
        }
        return instance;
    }

    private static PathingController instance;
    #endregion

    private void Start()
    {
        Vector3 originPosition =
            new Vector3(mapObject.transform.position.x - (mapObject.transform.localScale.x * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER),
                mapObject.transform.position.y,
                mapObject.transform.position.z - (mapObject.transform.localScale.z * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER));

        flowField = new FlowField(gridWidth, gridHeight, cellSize, originPosition);
        StartCoroutine(DelayedSetObstacleScores(flowField.GetGrid(), 0.1f));

        AStar = new AStar(gridWidth / areaSize, gridHeight / areaSize, cellSize * areaSize, originPosition);
        StartCoroutine(DelayedSetUnWalkableCells(GlobalConstants.OBSTACLES_STRING, 0.1f));

        if (showFlowFieldDebugText) flowField.GetGrid().ShowDebugText();
        if (showAStarDebugText) AStar.GetGrid().ShowDebugText();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            double startTimer = Time.realtimeSinceStartupAsDouble;
            //flowField.CalculateFlowField(flowField.GetGrid().GetCell(Utilities.GetMouseWorldPosition()));

            path = AStar.FindPath(Vector3.zero, Utilities.GetMouseWorldPosition());
            Debug.Log("Execution Time: " + (Time.realtimeSinceStartupAsDouble - startTimer) + "s");
        }
    }

    private void OnDrawGizmos()
    {
        if (flowField != null)
        {
            MyGrid<FlowFieldCell> grid = flowField.GetGrid();

            if (showFlowFieldGrid)
            {
                grid.ShowGrid(Color.black);
            }
            if (showFlowFieldArrows)
            {
                for (int x = 0; x < grid.GetGridWidth(); x++)
                {
                    for (int y = 0; y < grid.GetGridHeight(); y++)
                    {
                        GridDirection gridDirection = grid.GetCell(x, y).bestDirection;

                        if (gridDirection != GridDirection.None)
                        {
                            Utilities.DrawArrow(grid.GetCellCenterWorldPosition(x, y),
                                new Vector3(gridDirection.vector2D.x, 0, gridDirection.vector2D.y), grid.GetCellSize() * 0.5f, Color.black);
                        }
                    }
                }
            }
        }

        if (AStar != null)
        {
            MyGrid<AStarCell> grid = AStar.GetGrid();

            if (showAStarGrid)
            {
                grid.ShowGrid(Color.red);
            }
            if (showStarArrows)
            {
                if (path == null) return;

                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector2 gridDirection = path[i + 1].GetGridPosition() - path[i].GetGridPosition();

                    Utilities.DrawArrow(grid.GetCellCenterWorldPosition(path[i].GetGridPosition()),
                        new Vector3(gridDirection.x, 0f, gridDirection.y), grid.GetCellSize() * 0.5f, Color.black);
                }
            }
        }

    }

    // Because for an unknown reason, the code doesn't work inside this Start().
    IEnumerator DelayedSetObstacleScores(MyGrid<FlowFieldCell> grid, float delayedTime)
    {
        yield return new WaitForSeconds(delayedTime);

        flowField.CalculateFlowField(flowField.GetGrid().GetCell(baseObject.transform.position));
    }

    IEnumerator DelayedSetUnWalkableCells(string maskString, float delayedTime)
    {
        yield return new WaitForSeconds(delayedTime);

        AStar.SetUnWalkableCells(maskString);
    }


}
