using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PortalManager : MonoBehaviour
{
    private List<AStarCell> possiblePortalCells;
    private List<Portal> portals;

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
        possiblePortalCells = new List<AStarCell>();
        portals = new List<Portal>();

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
            if ((cell.X % PathingManager.GetInstance().CellSize == 0) ||
                (cell.X != PathingManager.GetInstance().AStar.Grid.Width && (cell.X + 1) % PathingManager.GetInstance().AreaSize == 0) ||
                (cell.Y % PathingManager.GetInstance().CellSize == 0) || (cell.Y != PathingManager.GetInstance().AStar.Grid.Height &&
                                                                          (cell.Y + 1) % PathingManager.GetInstance().AreaSize == 0))
            {
                if (!possiblePortalCells.Contains(cell))
                {
                    possiblePortalCells.Add(cell);
                }
            }
        }

        CreatePortals();
    }

    public void DrawPortal(Portal portal)
    {
        Vector3 posA = (portal.GetCellCenterWorldPosition(portal.AreaACells[0]) +
                        portal.GetCellCenterWorldPosition(portal.AreaBCells[0])) * 0.5f;
        Vector3 posB = (portal.GetCellCenterWorldPosition(portal.AreaACells[^1]) +
                        portal.GetCellCenterWorldPosition(portal.AreaBCells[^1])) * 0.5f;

        Debug.DrawLine(posA, portal.GetEntranceCellAreaACenterWorldPosition(), Color.red, 100f);
        Debug.DrawLine(portal.GetEntranceCellAreaACenterWorldPosition(), posB, Color.red, 100f);
        Debug.DrawLine(posB, portal.GetEntranceCellAreaBCenterWorldPosition(), Color.red, 100f);
        Debug.DrawLine(portal.GetEntranceCellAreaBCenterWorldPosition(), posA, Color.red, 100f);
    }

    private void CreatePortals()
    {
        MyGrid<AreaNode> areaGrid = PathingManager.GetInstance().AreaMap.Grid;

        foreach (AreaNode area in areaGrid.GridArray)
        {
            foreach (AreaNode neighborArea in areaGrid.GetNeighborCells(area.GridPosition, GridDirection.CardinalDirections))
            {
                CreateAndCombinePortals(area, neighborArea, Directions.North, Directions.South);
                CreateAndCombinePortals(area, neighborArea, Directions.East, Directions.West);
            }
        }

        foreach (Portal portal in portals)
        {
            DrawPortal(portal);
        }
    }

    private void CreateAndCombinePortals(AreaNode areaA, AreaNode areaB, Directions areaADirection, Directions areaBDirection)
    {
        MyGrid<FlowFieldCell> flowFieldGrid = PathingManager.GetInstance().FlowField.Grid;

        List<Portal> singlePortals = new List<Portal>();

        foreach (AStarCell cellA in areaA.GetBorderCells(areaADirection))
        {
            foreach (AStarCell cellB in areaA.GetBorderCells(areaBDirection))
            {
                Vector3 cellAWorldPos = areaA.AStarGrid.GetCellWorldPosition(cellA.GridPosition);
                FlowFieldCell flowFieldCellA = flowFieldGrid.GetCell(cellAWorldPos);
                FlowFieldCell flowFieldCellB = flowFieldGrid.GetCell(areaB.AStarGrid.GetCellWorldPosition(cellB.GridPosition));

                if (!flowFieldGrid.GetNeighborCells(cellAWorldPos, GridDirection.CardinalDirections).Contains(flowFieldCellB)) continue;

                if (flowFieldCellA.Cost == GlobalConstants.OBSTACLE_COST || flowFieldCellB.Cost == GlobalConstants.OBSTACLE_COST) continue;

                bool contains = false;

                foreach (Portal singlePortal in singlePortals)
                {
                    if (areaA.AStarGrid.GetNeighborCells(cellA.GridPosition, GridDirection.CardinalDirections).Contains(singlePortal.EntranceCellAreaA))
                    {
                        contains = true;
                    }
                }

                if (!contains && singlePortals.Count > 0)
                {
                    portals.Add(CombinePortals(singlePortals));
                    singlePortals.Clear();
                }

                Portal newPortal = new Portal(areaA, areaB, cellA, cellB);
                singlePortals.Add(newPortal);
            }
        }

        if (singlePortals.Count > 0)
        {
            portals.Add(CombinePortals(singlePortals));
        }
    }

    private Portal CombinePortals(List<Portal> singlePortals)
    {
        List<AStarCell> areaACells = new List<AStarCell>();
        List<AStarCell> areaBCells = new List<AStarCell>();
        foreach (Portal singlePortal in singlePortals)
        {
            areaACells.Add(singlePortal.EntranceCellAreaA);
            areaBCells.Add(singlePortal.EntranceCellAreaB);
        }

        return new Portal(singlePortals[0].AreaA, singlePortals[0].AreaB, areaACells, areaBCells);
    }
}
