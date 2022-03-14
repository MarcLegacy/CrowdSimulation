using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalNode
{
    public Portal Portal { get; }
    public bool visited;
    public Portal cameFromNode;
    public int fCost;
    public int gCost;
    public int hCost;

    public PortalNode(Portal portal)
    {
        Portal = portal;

        ResetNode();
    }

    public void ResetNode()
    {
        gCost = int.MaxValue;
        CalculateFCost();
        cameFromNode = null;
        visited = false;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}
