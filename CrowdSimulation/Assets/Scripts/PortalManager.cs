using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PortalManager : MonoBehaviour
{
    private List<PortalNode> portalNodes;
    private Dictionary<PortalNode, Dictionary<PortalNode, List<Vector3>>> neighborList;
    private List<PortalNode> openList;

    #region Singleton
    public static PortalManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PortalManager>();
        }
        return instance;
    }

    private static PortalManager instance;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        portalNodes = new List<PortalNode>();
        neighborList = new Dictionary<PortalNode, Dictionary<PortalNode, List<Vector3>>>();

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        CreatePortals();

        //foreach (PortalNode portalNode in portalNodes)
        //{
        //    DrawPortal(portalNode.Portal);
        //}

        CollectNeigbors();

        foreach (KeyValuePair<PortalNode, Dictionary<PortalNode, List<Vector3>>> currentPortal in neighborList)
        {
            Vector3 positionA = currentPortal.Key.Portal.GetPortalWorldPosition();
            foreach (KeyValuePair<PortalNode, List<Vector3>> neighbor in currentPortal.Value)
            {
                Vector3 positionB = neighbor.Key.Portal.GetPortalWorldPosition();

                //Debug.DrawLine(positionA, positionB, Color.blue, 100f);

                //Utilities.DrawDebugPathLines(neighbor.Value);
            }
        }


    }

    // Update is called once per frame
    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    double startTimer = Time.realtimeSinceStartupAsDouble;

        //    Vector3 startPosition = new Vector3(0, 0f, 0f);
        //    Vector3 endPosition = Utilities.GetMouseWorldPosition();

        //    List<Vector3> path = new List<Vector3> { startPosition };

        //    foreach (PortalNode portalNode in FindPathNodes(startPosition, endPosition))
        //    {
        //        path.Add(portalNode.Portal.GetPortalWorldPosition());
        //        //Debug.DrawRay(portalNode.Portal.GetPortalWorldPosition(), Vector3.up, Color.red, 100f);
        //    }

        //    path.Add(endPosition);

        //    Utilities.DrawDebugPathLines(path, Color.blue);

        //    //Utilities.DrawDebugPathLines(PathingManager.GetInstance().AStar.FindPath(startPosition, endPosition), Color.red);

        //    Debug.Log("Pathing time: " + (Math.Round((Time.realtimeSinceStartupAsDouble - startTimer) * 100000f) * 0.01) + "ms");
        //}
    }

    public void DrawPortal(Portal portal)
    {
        Vector3 posA = (portal.GetCellCenterWorldPosition(portal.AreaACells[0]) + portal.GetCellCenterWorldPosition(portal.AreaBCells[0])) * 0.5f;
        Vector3 posB = (portal.GetCellCenterWorldPosition(portal.AreaACells[^1]) + portal.GetCellCenterWorldPosition(portal.AreaBCells[^1])) * 0.5f;

        Debug.DrawLine(posA, portal.GetEntranceCellAreaACenterWorldPosition(), Color.red, 100f);
        Debug.DrawLine(portal.GetEntranceCellAreaACenterWorldPosition(), posB, Color.red, 100f);
        Debug.DrawLine(posB, portal.GetEntranceCellAreaBCenterWorldPosition(), Color.red, 100f);
        Debug.DrawLine(portal.GetEntranceCellAreaBCenterWorldPosition(), posA, Color.red, 100f);
    }

    public List<PortalNode> FindPathNodes(Vector3 startWorldPosition, Vector3 targetWorldPosition)
    {
        AreaNode startArea = PathingManager.GetInstance().AreaMap.Grid.GetCell(startWorldPosition);
        AreaNode endArea = PathingManager.GetInstance().AreaMap.Grid.GetCell(targetWorldPosition);
        List<PortalNode> endNodes = new List<PortalNode>();

        if (startArea == null)
        {
            Debug.LogWarning("No " + nameof(AreaNode) + " found on " + startWorldPosition);
            return null;
        }

        if (endArea == null)
        {
            Debug.LogWarning("No " + nameof(AreaNode) + " found on " + targetWorldPosition);
            return null;
        }

        openList = new List<PortalNode>();

        foreach (KeyValuePair<PortalNode, Dictionary<PortalNode, List<Vector3>>> portalNode in neighborList)
        {
            PortalNode currentPortalNode = portalNode.Key;
            currentPortalNode.ResetNode();

            if (currentPortalNode.Portal.ContainsArea(startArea))
            {
                List<Vector3> path = startArea.AStar.FindPath(startWorldPosition, currentPortalNode.Portal.GetEntranceCellWorldPosition(startArea));

                if (path == null || path.Count == 0) continue;

                currentPortalNode.gCost = path.Count;
                currentPortalNode.hCost = CalculateHCost(currentPortalNode.Portal.GetPortalWorldPosition(), targetWorldPosition);
                currentPortalNode.CalculateFCost();

                openList.Add(currentPortalNode);
            }

            if (currentPortalNode.Portal.ContainsArea(endArea))
            {
                List<Vector3> path = endArea.AStar.FindPath(targetWorldPosition, currentPortalNode.Portal.GetEntranceCellWorldPosition(endArea));

                if (path == null || path.Count == 0) continue;

                endNodes.Add(currentPortalNode);
            }
        }

        while (openList.Count > 0)
        {
            PortalNode currentNode = GetLowestFCostPortalNode(openList);

            if (endNodes.Contains(currentNode))
            {
                return CalculatePath(currentNode);
            }

            openList.Remove(currentNode);
            //closedList.Add(currentCell);
            currentNode.visited = true;

            foreach (KeyValuePair<PortalNode, List<Vector3>> neighborNode in neighborList[currentNode])
            {
                PortalNode currentNeighborNode = neighborNode.Key;

                //if (closedList.Contains(neighborCell)) continue;
                if (currentNeighborNode.visited) continue;

                int tentativeGCost = currentNode.gCost + neighborNode.Value.Count;

                if (tentativeGCost < currentNeighborNode.gCost)
                {
                    currentNeighborNode.cameFromNode = currentNode;
                    currentNeighborNode.gCost = tentativeGCost;
                    currentNeighborNode.hCost = CalculateHCost(currentNeighborNode.Portal.GetPortalWorldPosition(), targetWorldPosition);
                    currentNeighborNode.CalculateFCost();

                    if (!openList.Contains(currentNeighborNode))
                    {
                        openList.Add(currentNeighborNode);

                        //Utilities.CreateWorldText(
                        //    currentNeighborNode.gCost + "\n" + currentNeighborNode.hCost + "\n" + currentNeighborNode.fCost, null,
                        //    currentNeighborNode.Portal.GetPortalWorldPosition(), 20, Color.black);
                    }
                }
            }
        }

        return null;
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
                Vector3 cellAWorldPos = areaA.AStar.Grid.GetCellWorldPosition(cellA.GridPosition);
                FlowFieldCell flowFieldCellA = flowFieldGrid.GetCell(cellAWorldPos);
                FlowFieldCell flowFieldCellB = flowFieldGrid.GetCell(areaB.AStar.Grid.GetCellWorldPosition(cellB.GridPosition));

                if (!flowFieldGrid.GetNeighborCells(cellAWorldPos, GridDirection.CardinalDirections).Contains(flowFieldCellB)) continue;

                if (flowFieldCellA.Cost == GlobalConstants.OBSTACLE_COST || flowFieldCellB.Cost == GlobalConstants.OBSTACLE_COST) continue;

                bool contains = false;

                foreach (Portal singlePortal in singlePortals)
                {
                    if (areaA.AStar.Grid.GetNeighborCells(cellA.GridPosition, GridDirection.CardinalDirections).Contains(singlePortal.EntranceCellAreaA))
                    {
                        contains = true;
                    }
                }

                if (!contains && singlePortals.Count > 0)
                {
                    portalNodes.Add(new PortalNode(CombinePortals(singlePortals)));
                    singlePortals.Clear();
                }

                Portal newPortal = new Portal(areaA, areaB, cellA, cellB);
                singlePortals.Add(newPortal);
            }
        }

        if (singlePortals.Count > 0)
        {
            portalNodes.Add(new PortalNode(CombinePortals(singlePortals)));
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
        foreach (PortalNode portalNodeA in portalNodes)
        {
            foreach (PortalNode portalNodeB in portalNodes)
            {
                if (portalNodeA == portalNodeB) continue;

                Portal portalA = portalNodeA.Portal;
                Portal portalB = portalNodeB.Portal;

                if (portalA.AreaA == portalB.AreaA && portalA.AreaB == portalB.AreaB && portalA.EntranceCellAreaA == portalB.EntranceCellAreaA &&
                    portalA.EntranceCellAreaB == portalB.EntranceCellAreaB)
                {
                    Debug.Log("Oui!");
                }
            }
        }
    }

    private void CollectNeigbors()
    {
        foreach (PortalNode currentPortalNode in portalNodes)
        {
            Dictionary<PortalNode, List<Vector3>> newList = new Dictionary<PortalNode, List<Vector3>>();
            neighborList.Add(currentPortalNode, newList);

            foreach (PortalNode possibleNeighborPortalNode in portalNodes)
            {
                if (currentPortalNode == possibleNeighborPortalNode) continue;

                Portal currentPortal = currentPortalNode.Portal;
                Portal possibleNeighborPortal = possibleNeighborPortalNode.Portal;

                List<Vector3> pathA = new List<Vector3>();
                List<Vector3> pathB = new List<Vector3>();
                List<Vector3> shortestPath = new List<Vector3>();

                if (possibleNeighborPortal.ContainsArea(currentPortal.AreaA))
                {
                    pathA = currentPortal.AreaA.AStar.FindPath(currentPortal.GetEntranceCellAreaAWorldPosition(),
                        possibleNeighborPortal.GetEntranceCellWorldPosition(currentPortal.AreaA));
                }

                if (possibleNeighborPortal.ContainsArea(currentPortal.AreaB))
                {
                    pathB = currentPortal.AreaB.AStar.FindPath(currentPortal.GetEntranceCellAreaBWorldPosition(),
                        possibleNeighborPortal.GetEntranceCellWorldPosition(currentPortal.AreaB));
                }

                if (pathA != null && pathA.Count != 0 && (pathB == null || pathB.Count == 0))
                {
                    shortestPath = pathA;
                }

                if (pathB != null && pathB.Count != 0 && (pathA == null || pathA.Count == 0))
                {
                    shortestPath = pathB;
                }

                if (shortestPath.Count == 0)
                {
                    shortestPath = pathB != null && pathA != null && pathA.Count < pathB.Count ? pathA : pathB;
                }

                if (shortestPath == null || shortestPath.Count == 0) continue;

                if (neighborList.TryGetValue(currentPortalNode, out Dictionary<PortalNode, List<Vector3>> newNeighborList))
                {
                    newNeighborList.Add(possibleNeighborPortalNode, shortestPath);
                }
            }
        }
    }

    private int CalculateHCost(Vector3 posA, Vector3 posB)
    {
        MyGrid<FlowFieldCell> flowFieldGrid = PathingManager.GetInstance().FlowField.Grid;
        FlowFieldCell cellA = flowFieldGrid.GetCell(posA);
        FlowFieldCell cellB = flowFieldGrid.GetCell(posB);

        return Mathf.Abs(cellA.X - cellB.X) + Mathf.Abs(cellA.Y - cellB.Y);
    }

    private PortalNode GetLowestFCostPortalNode(List<PortalNode> portalNodeList)
    {
        PortalNode lowestFCostNode = portalNodeList[0];
        for (int i = 1; i < portalNodeList.Count; i++)
        {
            if (portalNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = portalNodeList[i];
            }
        }

        return lowestFCostNode;
    }


    private List<PortalNode> CalculatePath(PortalNode endPortalNode)
    {
        List<PortalNode> path = new List<PortalNode> { endPortalNode };
        PortalNode currentNode = endPortalNode;

        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();

        return path;
    }
}
