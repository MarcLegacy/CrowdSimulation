using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum PathMapLength
{
    Quarter,
    Half,
    ThreeQuarters,
    Whole
}

public class TestScript : MonoBehaviour
{
    public int runTestAmount = 4;
    public PathMapLength pathMapLength = PathMapLength.Whole;

    private PathingManager pathingManager;

    // Start is called before the first frame update
    void Start()
    {
        pathingManager = PathingManager.GetInstance();

        StartCoroutine(RunAStar());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RunAStar()
    {
        yield return new WaitForSeconds(1);

        MyGrid<FlowFieldCell> flowFieldGrid = pathingManager.FlowField.Grid;
        Stopwatch stopWatch = new Stopwatch();
        Vector2Int gridPathTarget = new Vector2Int(0, 0);
        List<double> aStarExecutionTimes = new List<double>(runTestAmount);
        List<double> flowFieldExecutionTimes = new List<double>(runTestAmount);

        pathingManager.AStar.FindPath(flowFieldGrid.GetCellCenterWorldPosition(0, 0),
            flowFieldGrid.GetCellCenterWorldPosition(gridPathTarget));
        GC.Collect();

        switch (pathMapLength)
        {
            case PathMapLength.Quarter:
                gridPathTarget = new Vector2Int((int)((flowFieldGrid.Width - 1) * 0.25), (int)((flowFieldGrid.Height - 1) * 0.25));
                break;
            case PathMapLength.Half:
                gridPathTarget = new Vector2Int((int)((flowFieldGrid.Width - 1) * 0.5), (int)((flowFieldGrid.Height - 1) * 0.5));
                break;
            case PathMapLength.ThreeQuarters:
                gridPathTarget = new Vector2Int((int)((flowFieldGrid.Width - 1) * 0.75), (int)((flowFieldGrid.Height - 1) * 0.75));
                break;
            case PathMapLength.Whole:
                gridPathTarget = new Vector2Int(flowFieldGrid.Width - 1, flowFieldGrid.Height - 1);
                break;
        }

        for (int i = 0; i < runTestAmount; i++)
        {
            stopWatch.Restart();
            List<Vector3> path = pathingManager.AStar.FindPath(flowFieldGrid.GetCellCenterWorldPosition(0, 0),
                flowFieldGrid.GetCellCenterWorldPosition(gridPathTarget));
            stopWatch.Stop();
            Debug.Log("A* Execution Time: " + Math.Round(stopWatch.Elapsed.TotalMilliseconds, 2) + "ms | PathLength: " + path.Count);
            aStarExecutionTimes.Add(stopWatch.Elapsed.TotalMilliseconds);
        }

        for (int i = 0; i < runTestAmount; i++)
        {
            stopWatch.Restart();
            pathingManager.FlowField.CalculateFlowField(flowFieldGrid.GetCellCenterWorldPosition(gridPathTarget));
            stopWatch.Stop();
            Debug.Log("FlowField Execution Time: " + Math.Round(stopWatch.Elapsed.TotalMilliseconds, 2) + "ms");
            flowFieldExecutionTimes.Add(stopWatch.Elapsed.TotalMilliseconds);
        }

        Debug.Log("Average A* Execution Time: " + Math.Round(aStarExecutionTimes.Average(), 2) + "ms");
        Debug.Log("Average FlowField Execution Time: " + Math.Round(flowFieldExecutionTimes.Average(), 2) + "ms");
    }
}
