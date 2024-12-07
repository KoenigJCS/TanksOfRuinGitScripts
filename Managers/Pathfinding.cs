// NOTICE: Entire class written by Tommy Smith
// NOTICE: At the bottom of this class is the previous unused pathfinding algorithm
// NOTICE: The commented code at the bottom should still be considered
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

    public List<HexCoord> FindPath(HexCoord startHex, HexCoord targetHex, I_Unit unit, bool playerTeam)
    {
        Debug.Log($"[Pathfinding] Finding path from {startHex} to {targetHex} for unit {unit.name}");

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
        Dictionary<HexCoord, Node> allNodes = new Dictionary<HexCoord, Node>();
        HashSet<HexCoord> closedSet = new HashSet<HexCoord>();

        Node startNode = new Node(startHex, null, 0, HexCoord.Distance(startHex, targetHex));
        openList.Add(startNode);
        allNodes.Add(startHex, startNode);

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

            if (currentNode.hexCoord == targetHex)
            {
                Debug.Log("[Pathfinding] Path found!");
                return RetracePath(startNode, currentNode);
            }

            foreach (HexCoord neighborHex in currentNode.hexCoord.Neighbors())
            {
                if (!TileManager.inst.IsOnTile(neighborHex))
                {
                    continue;
                }

                I_Tile neighborTile = TileManager.inst.GetTile(neighborHex);
                if (neighborTile == null)
                {
                    continue;
                }
                
                bool isOccupiedByEnemy = neighborTile.unitOnTile != null && neighborTile.unitOnTile.playerControlled == unit.playerControlled && neighborTile.unitOnTile != unit;

                if (isOccupiedByEnemy && neighborHex == targetHex)
                {
                    continue;
                }

                if (neighborTile.unitOnTile != null && neighborTile.unitOnTile.playerControlled != playerTeam && neighborTile.unitOnTile != unit)
                {
                    continue;
                }

                if (closedSet.Contains(neighborHex))
                {
                    continue;
                }

                float moveCost = neighborTile.moveCost;
                if (moveCost >= 100)
                {
                    continue;
                }

                Node neighborNode;
                if (allNodes.TryGetValue(neighborHex, out neighborNode))
                {
                    // Node already discovered
                }
                else
                {
                    float hCost = HexCoord.Distance(neighborHex, targetHex);
                    neighborNode = new Node(neighborHex, null, float.MaxValue, hCost);
                    allNodes.Add(neighborHex, neighborNode);
                }

                Node parentNode = currentNode.parent != null ? currentNode.parent : currentNode;
                /*if (LineOfSight(parentNode.hexCoord, neighborHex, unit))
                {
                    float newGCost = parentNode.gCost + HexCoord.Distance(parentNode.hexCoord, neighborHex);
                    if (newGCost < neighborNode.gCost)
                    {
                        neighborNode.gCost = newGCost;
                        neighborNode.parent = parentNode;
                        neighborNode.hCost = HexCoord.Distance(neighborHex, targetHex);

                        if (!openList.Contains(neighborNode))
                        {
                            openList.Add(neighborNode);
                        }
                    }
                }
                else
                {
                    float newGCost = currentNode.gCost + HexCoord.Distance(currentNode.hexCoord, neighborHex);
                    if (newGCost < neighborNode.gCost)
                    {
                        neighborNode.gCost = newGCost;
                        neighborNode.parent = currentNode;
                        neighborNode.hCost = HexCoord.Distance(neighborHex, targetHex);

                        if (!openList.Contains(neighborNode))
                        {
                            openList.Add(neighborNode);
                        }
                    }
                }*/
                float newGCost = currentNode.gCost + HexCoord.Distance(currentNode.hexCoord, neighborHex);
                if (newGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = newGCost;
                    neighborNode.parent = currentNode;
                    neighborNode.hCost = HexCoord.Distance(neighborHex, targetHex);

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        Debug.LogWarning($"[Pathfinding] No path found from {startHex} to {targetHex} for unit {unit.name}.");
        return new List<HexCoord>();
    }
    
    // alternate function; Calculates the distance between two hex coordinates.
    // float HexCoord.Distance(HexCoord a, HexCoord b)
    // {
    //     int deltaQ = Mathf.Abs(a.q - b.q);
    //     int deltaR = Mathf.Abs(a.r - b.r);

    //     return (deltaQ + deltaR);
    // }

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
    
    public bool LineOfSight(HexCoord from, HexCoord to, I_Unit unit, bool playerTeam)
    {
        List<HexCoord> line = GetHexesOnLine(from, to);
        foreach (HexCoord hex in line)
        {
            // Skip the starting hex
            if (hex == from)
                continue;

            I_Tile tile = TileManager.inst.GetTile(hex);
            if (tile == null)
            {
                Debug.Log($"[LineOfSight] Tile at {hex} is null.");
                return false;
            }

            if (tile.moveCost >= 100)
            {
                Debug.Log($"[LineOfSight] Tile at {hex} is impassable (moveCost >= 100).");
                return false;
            }

            if (tile.unitOnTile != null && playerTeam != tile.unitOnTile.playerControlled && tile.unitOnTile != unit)
            {
                Debug.Log($"[LineOfSight] Tile at {hex} is occupied by {tile.unitOnTile.name}.");
                return false;
            }
        }
        Debug.Log($"[LineOfSight] Line of sight clear from {from} to {to}.");
        return true;
    }

    
    private List<HexCoord> GetHexesOnLine(HexCoord start, HexCoord end)
    {
        int N = HexCoord.Distance(start, end);
        List<HexCoord> results = new List<HexCoord>();
        FractionalHex a = new FractionalHex(start.q, start.r, -start.q - start.r);
        FractionalHex b = new FractionalHex(end.q, end.r, -end.q - end.r);

        for (int i = 0; i <= N; i++)
        {
            float t = (N == 0) ? 0f : (1.0f / N) * i;
            FractionalHex lerpHex = FractionalHex.Lerp(a, b, t);
            HexCoord roundedHex = lerpHex.Round();
            results.Add(roundedHex);
        }
        return results;
    }

    
    struct FractionalHex
    {
        public float q;
        public float r;
        public float s;

        public FractionalHex(float q, float r, float s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
        }

        public HexCoord Round()
        {
            int qInt = Mathf.RoundToInt(q);
            int rInt = Mathf.RoundToInt(r);
            int sInt = Mathf.RoundToInt(s);

            float qDiff = Mathf.Abs(qInt - q);
            float rDiff = Mathf.Abs(rInt - r);
            float sDiff = Mathf.Abs(sInt - s);

            if (qDiff > rDiff && qDiff > sDiff)
            {
                qInt = -rInt - sInt;
            }
            else if (rDiff > sDiff)
            {
                rInt = -qInt - sInt;
            }
            else
            {
                sInt = -qInt - rInt;
            }

            return new HexCoord(qInt, rInt);
        }

        public static FractionalHex Lerp(FractionalHex a, FractionalHex b, float t)
        {
            return new FractionalHex(
                a.q + ((b.q - a.q) * t),
                a.r + ((b.r - a.r) * t),
                a.s + ((b.s - a.s) * t)
            );
        }
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
    
    public Dictionary<HexCoord, int> FindReachableTiles(HexCoord startHex, int maxCost, I_Unit unit, bool playerTeam)
    {
        Dictionary<HexCoord, int> reachableTiles = new Dictionary<HexCoord, int>();
        PriorityQueue<ReachableNode> frontier = new PriorityQueue<ReachableNode>();
        frontier.Enqueue(new ReachableNode(startHex, 0));

        reachableTiles[startHex] = 0;

        while (frontier.Count > 0)
        {
            ReachableNode currentNode = frontier.Dequeue();
            HexCoord currentHex = currentNode.hexCoord;

            if (currentNode.cost > maxCost)
            {
                continue;
            }

            foreach (HexCoord neighborHex in currentHex.Neighbors())
            {
                if (!TileManager.inst.IsOnTile(neighborHex))
                {
                    continue;
                }

                I_Tile neighborTile = TileManager.inst.GetTile(neighborHex);
                if (neighborTile == null)
                {
                    continue;
                }

                if (neighborTile.unitOnTile != null && neighborTile.unitOnTile.playerControlled != playerTeam && neighborTile.unitOnTile != unit)
                {
                    continue;
                }

                int moveCost = neighborTile.moveCost;
                if (moveCost >= 100)
                {
                    continue;
                }

                int newCost = currentNode.cost + moveCost;
                if (newCost > maxCost)
                {
                    continue;
                }

                if (!reachableTiles.ContainsKey(neighborHex) || newCost < reachableTiles[neighborHex])
                {
                    reachableTiles[neighborHex] = newCost;
                    frontier.Enqueue(new ReachableNode(neighborHex, newCost));
                }
            }
        }

        return reachableTiles;
    }

    public class ReachableNode : IComparable<ReachableNode>
    {
        public HexCoord hexCoord;
        public int cost;

        public ReachableNode(HexCoord hexCoord, int cost)
        {
            this.hexCoord = hexCoord;
            this.cost = cost;
        }

        public int CompareTo(ReachableNode other)
        {
            return this.cost.CompareTo(other.cost);
        }
    }

    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> data;

        public PriorityQueue()
        {
            this.data = new List<T>();
        }

        public void Enqueue(T item)
        {
            data.Add(item);
            int ci = data.Count - 1;
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (data[ci].CompareTo(data[pi]) >= 0)
                    break;
                T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
                ci = pi;
            }
        }

        public T Dequeue()
        {
            int li = data.Count - 1;
            T frontItem = data[0];
            data[0] = data[li];
            data.RemoveAt(li);

            --li; // last index after removal
            int pi = 0; // parent index
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci > li) break;
                int rc = ci + 1;
                if (rc <= li && data[rc].CompareTo(data[ci]) < 0)
                    ci = rc;
                if (data[pi].CompareTo(data[ci]) <= 0)
                    break;
                T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp;
                pi = ci;
            }
            return frontItem;
        }

        public int Count
        {
            get { return data.Count; }
        }
    }
}

/*using System.Collections.Generic;
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

        Node startNode = new Node(startHex, null, 0, HexCoord.Distance(startHex, targetHex));
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
                    float hCost = HexCoord.Distance(neighborHex, targetHex);
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
    float HexCoord.Distance(HexCoord a, HexCoord b)
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
}*/