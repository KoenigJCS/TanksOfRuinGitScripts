using System.Collections.Generic;
using UnityEngine;
using Settworks.Hexagons;
using System;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding inst;

    void Awake()
    {
        inst = this;
    }

    public List<HexCoord> FindPath(HexCoord startHex, HexCoord targetHex, I_Unit unit)
    {
        Debug.Log($"[Pathfinding] Starting pathfinding from {startHex} to {targetHex} for unit {unit.name}");

        // Validate start and target hexes
        if (!TileManager.inst.IsOnTile(startHex))
        {
            Debug.LogError($"[Pathfinding] Start hex {startHex} is not on the grid.");
            return new List<HexCoord>();
        }

        if (!TileManager.inst.IsOnTile(targetHex))
        {
            Debug.LogError($"[Pathfinding] Target hex {targetHex} is not on the grid.");
            return new List<HexCoord>();
        }

        List<Node> openList = new List<Node>();
        HashSet<HexCoord> closedSet = new HashSet<HexCoord>();

        Node startNode = new Node(startHex, null, 0, CalculateDistance(startHex, targetHex));
        openList.Add(startNode);
        Debug.Log($"[Pathfinding] Added start node {startHex} to open list with fCost {startNode.fCost}");

        int loopCounter = 0;
        int maxLoops = 1000;

        while (openList.Count > 0)
        {
            loopCounter++;
            if (loopCounter > maxLoops)
            {
                Debug.LogError("[Pathfinding] Pathfinding exceeded maximum loop iterations.");
                break;
            }

            // Find the node with the lowest fCost
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedSet.Add(currentNode.hexCoord);
            Debug.Log($"[Pathfinding] Processing node {currentNode.hexCoord}, fCost: {currentNode.fCost}, hCost: {currentNode.hCost}");

            if (currentNode.hexCoord == targetHex)
            {
                Debug.Log("[Pathfinding] Path found!");
                return RetracePath(startNode, currentNode);
            }

            foreach (HexCoord neighborHex in currentNode.hexCoord.Neighbors())
            {
                Debug.Log($"[Pathfinding] Evaluating neighbor {neighborHex} of {currentNode.hexCoord}");

                if (!TileManager.inst.IsOnTile(neighborHex))
                {
                    Debug.Log($"[Pathfinding] Neighbor {neighborHex} is not on the grid.");
                    continue;
                }

                I_Tile neighborTile = TileManager.inst.GetTile(neighborHex);
                if (neighborTile == null)
                {
                    Debug.LogError($"[Pathfinding] Neighbor tile at {neighborHex} is null.");
                    continue;
                }

                // Check if the tile is occupied by another unit
                if (neighborTile.unitOnTile != null && neighborTile.unitOnTile != unit)
                {
                    Debug.Log($"[Pathfinding] Neighbor tile at {neighborHex} is occupied by {neighborTile.unitOnTile.name}.");
                    continue;
                }

                if (closedSet.Contains(neighborHex))
                {
                    Debug.Log($"[Pathfinding] Neighbor {neighborHex} has already been evaluated.");
                    continue;
                }

                float tentativeGCost = currentNode.gCost + neighborTile.moveCost;
                Debug.Log($"[Pathfinding] Tentative gCost for neighbor {neighborHex}: {tentativeGCost}");

                Node neighborNode = openList.Find(n => n.hexCoord == neighborHex);

                if (neighborNode == null)
                {
                    float hCost = CalculateDistance(neighborHex, targetHex);
                    neighborNode = new Node(neighborHex, currentNode, tentativeGCost, hCost);
                    openList.Add(neighborNode);
                    Debug.Log($"[Pathfinding] Added neighbor {neighborHex} to open list with fCost {neighborNode.fCost}");
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.parent = currentNode;
                    Debug.Log($"[Pathfinding] Updated neighbor {neighborHex} in open list with new gCost {tentativeGCost} and fCost {neighborNode.fCost}");
                }
            }

            Debug.Log($"[Pathfinding] Open list now contains {openList.Count} nodes.");
            Debug.Log($"[Pathfinding] Closed set now contains {closedSet.Count} nodes.");
        }

        Debug.LogWarning($"[Pathfinding] No path found from {startHex} to {targetHex} for unit {unit.name}.");
        return new List<HexCoord>();
    }

    /// <summary>
    /// Correctly calculates the distance between two hex coordinates using axial coordinates.
    /// </summary>
    float CalculateDistance(HexCoord a, HexCoord b)
    {
        int deltaQ = Mathf.Abs(a.q - b.q);
        int deltaR = Mathf.Abs(a.r - b.r);

        return (deltaQ + deltaR);
    }

    List<HexCoord> RetracePath(Node startNode, Node endNode)
    {
        List<HexCoord> path = new List<HexCoord>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.hexCoord);
            currentNode = currentNode.parent;
            if (currentNode == null)
            {
                Debug.LogError("[Pathfinding] Path retracing encountered a null parent node.");
                break;
            }
        }
        path.Reverse();
        Debug.Log($"[Pathfinding] Path retraced: {string.Join(" -> ", path)}");
        return path;
    }

    class Node
    {
        public HexCoord hexCoord;
        public Node parent;
        public float gCost;
        public float hCost;
        public float fCost { get { return gCost + hCost; } }

        public Node(HexCoord hexCoord, Node parent, float gCost, float hCost)
        {
            this.hexCoord = hexCoord;
            this.parent = parent;
            this.gCost = gCost;
            this.hCost = hCost;
        }
    }
}