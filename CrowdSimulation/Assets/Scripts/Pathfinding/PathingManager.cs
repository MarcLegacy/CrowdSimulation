using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathingManager : MonoBehaviour
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

    [HideInInspector] public FlowField flowField;
    [HideInInspector] public AreaGrid areaGrid;


    private List<AStarCell> path; 
    private HeatMapManager heatMapManager;

    #region Singleton
    public static PathingManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PathingManager>();
        }
        return instance;
    }

    private static PathingManager instance;
    #endregion

    private void Start()
    {
        Vector3 originPosition =
            new Vector3(mapObject.transform.position.x - (mapObject.transform.localScale.x * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER),
                mapObject.transform.position.y,
                mapObject.transform.position.z - (mapObject.transform.localScale.z * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER));

        flowField = new FlowField(gridWidth, gridHeight, cellSize, originPosition);
        areaGrid = new AreaGrid(gridWidth / areaSize, gridHeight / areaSize, cellSize * areaSize, originPosition, areaSize, cellSize);

        if (showFlowFieldDebugText) flowField.GetGrid().ShowDebugText();

        StartCoroutine(DelayedStart());
    }

    // Because for an unknown reason, the position of the colliders aren't yet set on the position of the gameObject immediatelly
    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        flowField.CalculateFlowField(flowField.GetGrid().GetCell(baseObject.transform.position));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            double startTimer = Time.realtimeSinceStartupAsDouble;
            //flowField.CalculateFlowField(flowField.GetGrid().GetCell(Utilities.GetMouseWorldPosition()));

            Debug.Log("Execution Time: " + (Time.realtimeSinceStartupAsDouble - startTimer) + "s");
        }
    }

    private void OnDrawGizmos()
    {
        if (flowField != null)
        {
            if (showFlowFieldGrid)
            {
                flowField.GetGrid().ShowGrid(Color.black);
            }
            if (showFlowFieldArrows)
            {
                flowField.DrawFlowFieldArrows();
            }
        }

        if (areaGrid != null)
        {
            MyGrid<AreaNode> grid = areaGrid.GetGrid();

            for (int x = 0; x < grid.GetGridWidth(); x++)
            {
                for (int y = 0; y < grid.GetGridHeight(); y++)
                {
                    grid.GetCell(x, y).GetGrid().ShowGrid(Color.black);
                }
            }

            areaGrid.GetGrid().ShowGrid(Color.red);
        }
        
    }
}
