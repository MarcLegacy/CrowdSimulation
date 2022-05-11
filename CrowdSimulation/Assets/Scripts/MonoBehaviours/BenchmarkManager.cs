using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class BenchmarkManager : MonoBehaviour
{
    [SerializeField] private int maxBenchmarkRuns = 1;

    [ReadOnly] public List<double> flowFieldExecutionTimes;
    [ReadOnly] public List<double> pathingExecutionTimes;

    private PathingManager pathingManager;
    private UnitManager unitManager;
    private int totalBenchmarkRuns;

    #region Singleton
    public static BenchmarkManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<BenchmarkManager>();
        }
        return instance;
    }

    private static BenchmarkManager instance;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        pathingManager = PathingManager.GetInstance();
        unitManager = UnitManager.GetInstance();

        flowFieldExecutionTimes = new List<double>();
        pathingExecutionTimes = new List<double>();

        //Random.InitState(3);

        unitManager.OnMaxUnitSpawned += OnMaxUnitsSpawned;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMaxUnitsSpawned(object sender, UnitManager.OnMaxUnitsSpawnedEventArgs eventArgs)
    {
        if (pathingManager.PathingMethod == PathingMethod.FlowFieldOnly)
        {
            StartCoroutine(SetBenchmarkPositionsNormalPathingCoroutine());
        }
        else
        {
            StartCoroutine(SetBenchmarkPositionsWithAreaPathingCoroutine());
        }
    }

    IEnumerator SetBenchmarkPositionsWithAreaPathingCoroutine()
    {
        MyGrid<FlowFieldCell> grid = pathingManager.FlowField.Grid;
        totalBenchmarkRuns++;

        yield return new WaitForSeconds(1f);

        PortalManager.GetInstance().FindPathNodes(Vector3.zero, Vector3.zero);  // This makes sure that this one will take the heavy reload instead of the actual test.

        GC.Collect();
        pathingManager.TargetPosition = grid.GetCellCenterWorldPosition(0, 0);

        yield return new WaitForSeconds(1f);

        GC.Collect();
        pathingManager.TargetPosition = grid.GetCellCenterWorldPosition(grid.Width - 1, 0);

        yield return new WaitForSeconds(1f);

        GC.Collect();
        pathingManager.TargetPosition = grid.GetCellCenterWorldPosition(grid.Width - 1, grid.Height - 1);

        yield return new WaitForSeconds(1f);

        GC.Collect();
        pathingManager.TargetPosition = grid.GetCellCenterWorldPosition(0, grid.Height - 1);

        if (totalBenchmarkRuns < maxBenchmarkRuns)
        {
            StartCoroutine(SetBenchmarkPositionsWithAreaPathingCoroutine());
        }
        else
        {
            Debug.Log("Benchmark Results:");
            Debug.Log("Average Summed Pathing Time: " + Math.Round(pathingExecutionTimes.Average() * 100f) * 0.01 + "ms");
            Debug.Log("Average FlowField Time: " + Math.Round(flowFieldExecutionTimes.Average() * 100f) * 0.01 + "ms");
        }
    }

    IEnumerator SetBenchmarkPositionsNormalPathingCoroutine()
    {
        FlowField flowField = pathingManager.FlowField;
        MyGrid<FlowFieldCell> flowFieldGrid = flowField.Grid;
        Stopwatch stopwatch = new Stopwatch();
        totalBenchmarkRuns++;

        yield return new WaitForSeconds(1f);

        GC.Collect();
        stopwatch.Start();
        flowField.CalculateFlowField(flowFieldGrid.GetCell(0, 0));
        stopwatch.Stop();
        flowFieldExecutionTimes.Add(Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2));

        yield return new WaitForSeconds(1f);

        GC.Collect();
        stopwatch.Restart();
        flowField.CalculateFlowField(flowFieldGrid.GetCell(flowFieldGrid.Width - 1, 0));
        stopwatch.Stop();
        flowFieldExecutionTimes.Add(Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2));

        yield return new WaitForSeconds(1f);

        GC.Collect();
        stopwatch.Restart();
        flowField.CalculateFlowField(flowFieldGrid.GetCell(flowFieldGrid.Width - 1, flowFieldGrid.Height - 1));
        stopwatch.Stop();
        flowFieldExecutionTimes.Add(Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2));

        yield return new WaitForSeconds(1f);

        GC.Collect();
        stopwatch.Restart();
        flowField.CalculateFlowField(flowFieldGrid.GetCell(0, flowFieldGrid.Height - 1));
        stopwatch.Stop();
        flowFieldExecutionTimes.Add(Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2));

        if (totalBenchmarkRuns < maxBenchmarkRuns)
        {
            StartCoroutine(SetBenchmarkPositionsNormalPathingCoroutine());
        }
        else
        {
            Debug.Log("Benchmark Results:");
            Debug.Log("Average FlowField Time: " + Math.Round(flowFieldExecutionTimes.Average() * 100f) * 0.01 + "ms");
        }
    }
}
