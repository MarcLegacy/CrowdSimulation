using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    private List<AStarCell> possiblePortalCells;

    private AreaMap areaMap;
    private AStar aStar;

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

    // Start is called before the first frame update
    void Start()
    {
        areaMap = PathingManager.GetInstance().AreaMap;
        aStar = PathingManager.GetInstance().AStar;

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        SetupPossiblePortalCells();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetupPossiblePortalCells()
    {
        foreach (AStarCell cell in PathingManager.GetInstance().AStar.Grid.GridArray)
        {
            if (cell.X % PathingManager.GetInstance().CellSize == 0)
            {
                HeatMapManager.GetInstance().ColorCell(cell.GridPosition, HeatMapColor.Green);
            }
            if (cell.X != PathingManager.GetInstance().AStar.Grid.Width && (cell.X + 1) % PathingManager.GetInstance().AreaSize == 0)
            {
                HeatMapManager.GetInstance().ColorCell(new Vector2Int(cell.X, cell.Y), HeatMapColor.Green);
            }
            if (cell.Y % PathingManager.GetInstance().CellSize == 0)
            {
                HeatMapManager.GetInstance().ColorCell(cell.GridPosition, HeatMapColor.Green);
            }
            if (cell.Y != PathingManager.GetInstance().AStar.Grid.Height && (cell.Y + 1) % PathingManager.GetInstance().AreaSize == 0)
            {
                HeatMapManager.GetInstance().ColorCell(new Vector2Int(cell.X, cell.Y), HeatMapColor.Green);
            }
        }
    }
}
