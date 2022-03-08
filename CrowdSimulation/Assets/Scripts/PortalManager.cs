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

        MakePortals();
    }

    private void MakePortals()
    {
        MyGrid<AreaNode> areaGrid = PathingManager.GetInstance().AreaMap.Grid;
        MyGrid<FlowFieldCell> flowFieldGrid = PathingManager.GetInstance().FlowField.Grid;


        foreach (AreaNode area in areaGrid.GridArray)
        {
            foreach (AreaNode neighborArea in areaGrid.GetNeighborCells(area.GridPosition, GridDirection.CardinalDirections))
            {
                foreach (AStarCell cellA in area.GetBorderCells(Directions.North))
                {
                    foreach (AStarCell cellB in area.GetBorderCells(Directions.South))
                    {
                        List<Portal> singlePortals = new List<Portal>();
                        FlowFieldCell flowFieldCellA = flowFieldGrid.GetCell(area.AStarGrid.GetCellWorldPosition(cellA.GridPosition));
                        FlowFieldCell flowFieldCellB = flowFieldGrid.GetCell(neighborArea.AStarGrid.GetCellWorldPosition(cellB.GridPosition));
                        Vector3 cellAWorldPos = flowFieldGrid.GetCellWorldPosition(flowFieldCellA.GridPosition);
                        Vector3 cellBWorldPos = flowFieldGrid.GetCellWorldPosition(flowFieldCellB.GridPosition);

                        if (flowFieldGrid.GetNeighborCells(cellAWorldPos, GridDirection.CardinalDirections).Contains(flowFieldCellB))
                        {
                            if (flowFieldCellA.Cost != GlobalConstants.OBSTACLE_COST && flowFieldCellB.Cost != GlobalConstants.OBSTACLE_COST)
                            {
                                //Debug.DrawLine(flowFieldGrid.GetCellCenterWorldPosition(flowFieldCellA.GridPosition), flowFieldGrid.GetCellCenterWorldPosition(flowFieldCellB.GridPosition), Color.red, 100f);
                                Portal newPortal = new Portal(area, neighborArea, cellA, cellB);
                                singlePortals.Add(newPortal);
                                DrawPortal(newPortal);

                                //bool contains = false;

                                //foreach (Portal singlePortal in singlePortals)
                                //{
                                //    if (flowFieldGrid.GetNeighborCells(singlePortal.AreaACells[0].GridPosition, GridDirection.CardinalDirections).Contains(flowFieldCellA))
                                //    {
                                //        contains = true;
                                //    }
                                //}

                                //if (!contains)
                                //{
                                //    List<AStarCell> areaACells = new List<AStarCell>();
                                //    List<AStarCell> areaBCells = new List<AStarCell>();
                                //    foreach (Portal singlePortal in singlePortals)
                                //    {
                                //        areaACells.Add(singlePortal.AreaACells[0]);
                                //        areaBCells.Add(singlePortal.AreaBCells[0]);
                                //    }

                                //    Portal portal = new Portal(area, neighborArea, areaACells, areaBCells);
                                //    portals.Add(portal);
                                //    singlePortals.Clear();
                                //    DrawPortal(portal);
                                //}
                            }
                        }


                    }
                }

                foreach (AStarCell cellA in area.GetBorderCells(Directions.East))
                {
                    foreach (AStarCell cellB in area.GetBorderCells(Directions.West))
                    {
                        List<Portal> singlePortals = new List<Portal>();
                        FlowFieldCell flowFieldCellA = flowFieldGrid.GetCell(area.AStarGrid.GetCellWorldPosition(cellA.GridPosition));
                        FlowFieldCell flowFieldCellB = flowFieldGrid.GetCell(neighborArea.AStarGrid.GetCellWorldPosition(cellB.GridPosition));
                        Vector3 cellAWorldPos = flowFieldGrid.GetCellWorldPosition(flowFieldCellA.GridPosition);
                        Vector3 cellBWorldPos = flowFieldGrid.GetCellWorldPosition(flowFieldCellB.GridPosition);

                        if (flowFieldGrid.GetNeighborCells(cellAWorldPos, GridDirection.CardinalDirections).Contains(flowFieldCellB))
                        {
                            if (flowFieldCellA.Cost != GlobalConstants.OBSTACLE_COST && flowFieldCellB.Cost != GlobalConstants.OBSTACLE_COST)
                            {
                                //Debug.DrawLine(flowFieldGrid.GetCellCenterWorldPosition(flowFieldCellA.GridPosition), flowFieldGrid.GetCellCenterWorldPosition(flowFieldCellB.GridPosition), Color.red, 100f);
                                Portal portal = new Portal(area, neighborArea, cellA, cellB);
                                singlePortals.Add(portal);
                                DrawPortal(portal);
                            }
                        }
                    }
                }
            }
        }
    }

    public void DrawPortal(Portal portal)
    {
        Vector3 entranceCenter = (portal.AreaNodeA.AStarGrid.GetCellCenterWorldPosition(portal.GetEntranceCell(portal.AreaNodeA).GridPosition) +
                                  portal.AreaNodeB.AStarGrid.GetCellCenterWorldPosition(portal.GetEntranceCell(portal.AreaNodeB).GridPosition)) /
                                 2; 

        foreach (AStarCell areaACell in portal.AreaACells)
        {
            Debug.DrawLine(portal.AreaNodeA.AStarGrid.GetCellCenterWorldPosition(areaACell.GridPosition), entranceCenter, Color.red, 100f);
        }
        foreach (AStarCell areaBCell in portal.AreaBCells)
        {
            Debug.DrawLine(portal.AreaNodeB.AStarGrid.GetCellCenterWorldPosition(areaBCell.GridPosition), entranceCenter, Color.red, 100f);
        }
    }
}
