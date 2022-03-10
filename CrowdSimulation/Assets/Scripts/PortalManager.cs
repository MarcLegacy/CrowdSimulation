using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PortalManager : MonoBehaviour
{
    private List<AStarCell> possiblePortalCells;
    private List<Portal> portals;
    private Dictionary<Portal, Dictionary<Portal, int>> neighborList;

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
        neighborList = new Dictionary<Portal, Dictionary<Portal, int>>();

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        CreatePortals();

        //foreach (Portal portal in portals)
        //{
        //    DrawPortal(portal);
        //}

        CollectNeigbors();

        foreach (KeyValuePair<Portal, Dictionary<Portal, int>> currentPortal in neighborList)
        {
            Vector3 positionA = currentPortal.Key.GetEntranceCellCenterWorldPosition();
            foreach (KeyValuePair<Portal, int> neighbor in currentPortal.Value)
            {
                Vector3 positionB = neighbor.Key.GetEntranceCellCenterWorldPosition();

                Debug.DrawLine(positionA, positionB, Color.blue, 100f);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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

    private void CheckUniquePortals()
    {
        foreach (Portal portalA in portals)
        {
            foreach (Portal portalB in portals)
            {
                if (portalA == portalB) continue;

                if (portalA.AreaA == portalB.AreaA && portalA.AreaB == portalB.AreaB && portalA.EntranceCellAreaA == portalB.EntranceCellAreaA && portalA.EntranceCellAreaB == portalB.EntranceCellAreaB)
                {
                    Debug.Log("Oui!");
                }
            }
        }
    }

    private void CollectNeigbors()
    {
        foreach (Portal currentPortal in portals)
        {
            Dictionary<Portal, int> newNeighborList = new Dictionary<Portal, int>();
            foreach (Portal possibleNeighborPortal in portals)
            {
                if (currentPortal == possibleNeighborPortal) continue;

                if (possibleNeighborPortal.ContainsArea(currentPortal.AreaA))
                {
                    newNeighborList.Add(possibleNeighborPortal, 0);

                    //currentPortal.AreaA.AStarGrid.p
                }

                if (possibleNeighborPortal.ContainsArea(currentPortal.AreaB))
                {
                    newNeighborList.Add(possibleNeighborPortal, 0);

                    //currentPortal.
                }
            }

            neighborList.Add(currentPortal, newNeighborList);
        }
    }
}
