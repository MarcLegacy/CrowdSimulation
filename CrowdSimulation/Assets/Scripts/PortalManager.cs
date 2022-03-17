using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PortalManager : MonoBehaviour
{
    [SerializeField] private bool showPortals = false;
    [SerializeField] private bool showPortalConnections = false;
    [SerializeField] private bool showPortalPaths = false;

    private List<PortalNode> portalNodes;
    public Dictionary<PortalNode, Dictionary<PortalNode, List<Vector3>>> NeighborList { get; private set; }
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
        NeighborList = new Dictionary<PortalNode, Dictionary<PortalNode, List<Vector3>>>();

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        CreatePortals();

        CollectNeigbors();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void OnDrawGizmos()
    {
        if (portalNodes != null && showPortals)
        {
            foreach (PortalNode portalNode in portalNodes)
            {
                Utilities.DrawGizmosPortal(portalNode.Portal, Color.red);
            }
        }

        if (NeighborList != null && (showPortalConnections || showPortalPaths))
        {
            foreach (KeyValuePair<PortalNode, Dictionary<PortalNode, List<Vector3>>> currentPortal in NeighborList)
            {
                Vector3 positionA = currentPortal.Key.Portal.WorldPosition;
                foreach (KeyValuePair<PortalNode, List<Vector3>> neighbor in currentPortal.Value)
                {
                    Vector3 positionB = neighbor.Key.Portal.WorldPosition;

                    if (showPortalConnections)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(positionA, positionB);
                    }

                    if (showPortalPaths)
                    {
                        Utilities.DrawGizmosPathLines(neighbor.Value, Color.black);
                    }
                }
            }
        }
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

        foreach (KeyValuePair<PortalNode, Dictionary<PortalNode, List<Vector3>>> portalNode in NeighborList)
        {
            PortalNode currentPortalNode = portalNode.Key;
            currentPortalNode.ResetNode();

            if (currentPortalNode.Portal.ContainsArea(startArea))
            {
                List<Vector3> path = startArea.AStar.FindPath(startWorldPosition, currentPortalNode.Portal.GetEntranceCellWorldPosition(startArea));

                if (path == null || path.Count == 0) continue;

                currentPortalNode.gCost = path.Count;
                currentPortalNode.hCost = CalculateHCost(currentPortalNode.Portal.WorldPosition, targetWorldPosition);
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

            currentNode.visited = true;

            foreach (KeyValuePair<PortalNode, List<Vector3>> neighborNode in NeighborList[currentNode])
            {
                PortalNode currentNeighborNode = neighborNode.Key;

                if (currentNeighborNode.visited) continue;

                int tentativeGCost = currentNode.gCost + neighborNode.Value.Count;

                if (tentativeGCost < currentNeighborNode.gCost)
                {
                    currentNeighborNode.cameFromNode = currentNode;
                    currentNeighborNode.gCost = tentativeGCost;
                    currentNeighborNode.hCost = CalculateHCost(currentNeighborNode.Portal.WorldPosition, targetWorldPosition);
                    currentNeighborNode.CalculateFCost();

                    if (!openList.Contains(currentNeighborNode))
                    {
                        openList.Add(currentNeighborNode);

                        //Utilities.CreateWorldText(
                        //    currentNeighborNode.gCost + "\n" + currentNeighborNode.hCost + "\n" + currentNeighborNode.fCost, null,
                        //    currentNeighborNode.Portal.GetWorldPosition(), 20, Color.black);
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
            NeighborList.Add(currentPortalNode, newList);

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

                if (NeighborList.TryGetValue(currentPortalNode, out Dictionary<PortalNode, List<Vector3>> newNeighborList))
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
