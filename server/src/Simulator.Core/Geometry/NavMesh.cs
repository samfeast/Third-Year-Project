using System.Diagnostics;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Shapes;

namespace Simulator.Core.Geometry;

public class NavMesh(int cellSize)
{
    public struct Node(Triangle triangle)
    {
        public readonly Vector2Int[] Vertices = [triangle.A, triangle.B, triangle.C];
        public readonly int[] Neighbours = [-1, -1, -1];
        public readonly Vector2Fraction Centroid = triangle.GetCentroid();
        public readonly Triangle Triangle = triangle;
        public readonly long DoubleArea = triangle.GetDoubleArea();
    }

    public struct Portal(Vector2Int left, Vector2Int right)
    {
        public readonly Vector2Int Left = left;
        public readonly Vector2Int Right = right;
    }

    public readonly List<Node> Nodes = [];
    public readonly List<long> HeatMap = [];
    public readonly UniformGrid<int> Grid = new(cellSize);
    public readonly List<long> CumulativeDoubleAreas = [];

    public List<Portal> GetPortals(Vector2Int source, Vector2Int destination)
    {
        // Degrades when starting and ending on boundaries
        var startNode = GetCurrentNode(source.X, source.Y)[0];
        var endNode = GetCurrentNode(destination.X, destination.Y)[0];

        // Pathfinding using Dijkstra's
        var channel = GetChannel(startNode, endNode);
        
        var portals = GetPortalsFromChannel(channel);
        // Add degenerate portals for destination
        portals.Add(new Portal(destination, destination));

        return portals;
    }

    public List<Vector2Int> GetFullFunnelPath(Vector2Int position, List<Portal> portals)
    {
        List<Vector2Int> path = [position];
        // Portals should never end up empty, could be while (true) but this acts as a guard
        while (portals.Count > 0)
        {
            // Get the next turning point from the last fixed point currently in the path
            var nextPoint = GetNextTurningPoint(path[^1], portals);
            path.Add(nextPoint);
            
            // If the next point is equal to the destination return the completed path
            if (nextPoint == portals[^1].Left)
                return path;
            
            // Remove portals which were crossed to reach the next point
            var crossedPortals = GetNumCrossedPortals(nextPoint, portals);

            // If we would be removing all remaining portals, add the destination and return
            if (crossedPortals >= portals.Count)
            {
                path.Add(portals[^1].Left);
                return path;
            }
            
            portals.RemoveRange(0, crossedPortals);
        }
        
        throw new UnreachableException();
    }

    public static int GetNumCrossedPortals(Vector2Int position, List<Portal> remainingPortals)
    {
        var crossedPortals = 0;
        foreach (var portal in remainingPortals)
        {
            if (portal.Left == portal.Right) continue;
            // Condition met if position is to the right of the left->right portal vector
            if (Sign(portal.Left, portal.Right, position) >= 0)
                crossedPortals++;
            else
                break;
        }
        return crossedPortals;
    }

    public static Vector2Int GetNextTurningPoint(Vector2Int position, List<Portal> remainingPortals)
    {
        // Current funnel state
        var portalApex = position;
        var portalLeft = position;
        var portalRight = position;

        for (int i = 0; i < remainingPortals.Count; i++)
        {
            var newLeft = remainingPortals[i].Left;
            var newRight = remainingPortals[i].Right;

            // Right boundary
            if (Sign(portalApex, portalRight, newRight) >= 0)
            {
                if (portalApex == portalRight ||
                    Sign(portalApex, portalLeft, newRight) < 0)
                {
                    portalRight = newRight;
                }
                else
                {
                    return portalLeft;
                }
            }

            // Left boundary
            if (Sign(portalApex, portalLeft, newLeft) <= 0)
            {
                if (portalApex == portalLeft ||
                    Sign(portalApex, portalRight, newLeft) > 0)
                {
                    portalLeft = newLeft;
                }
                else
                {
                    return portalRight;
                }
            }
        }
        
        return remainingPortals[^1].Left;
    }

    public List<int> GetCurrentNode(Vector2Int position)
    {
        return GetCurrentNode(position.X, position.Y);
    }

    public List<int> GetCurrentNode(int x, int y)
    {
        var possibleNodeIndices = Grid.Get(x, y);

        var rtn = new List<int>(possibleNodeIndices.Count);
        foreach (var i in possibleNodeIndices)
        {
            if (Nodes[i].Triangle.ContainsPoint(new Vector2Int(x, y)))
                rtn.Add(i);
        }

        return rtn;
    }

    private List<int> GetChannel(int sourceIndex, int destinationIndex)
    {
        var dist = new double[Nodes.Count];
        var prev = new int[Nodes.Count];
        var visited = new bool[Nodes.Count];

        for (int i = 0; i < Nodes.Count; i++)
        {
            dist[i] = double.PositiveInfinity;
            prev[i] = -1;
        }

        dist[sourceIndex] = 0;

        // Min-priority queue: (distance, nodeIndex)
        var queue = new PriorityQueue<int, double>();
        queue.Enqueue(sourceIndex, 0);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (visited[current])
                continue;

            visited[current] = true;

            // Early exit if destination found
            if (current == destinationIndex)
                break;

            foreach (var neighbour in Nodes[current].Neighbours)
            {
                if (neighbour == -1 || visited[neighbour])
                    continue;

                double alt = dist[current] + GetCentroidDistance(current, neighbour);

                if (alt < dist[neighbour])
                {
                    dist[neighbour] = alt;
                    prev[neighbour] = current;
                    queue.Enqueue(neighbour, alt);
                }
            }
        }

        var path = new List<int>();

        // Case where no path exists
        if (prev[destinationIndex] == -1 && destinationIndex != sourceIndex)
            return path;

        for (int at = destinationIndex; at != -1; at = prev[at])
            path.Add(at);

        path.Reverse();
        return path;
    }

    private List<Portal> GetPortalsFromChannel(List<int> channel)
    {
        var portals = new List<Portal>(channel.Count);

        for (int i = 0; i < channel.Count - 1; i++)
        {
            Portal? portal = GetPortal(channel[i + 1], channel[i]);
            Debug.Assert(portal != null, "Portal not found between adjacent nodes");
            portals.Add(portal.Value);
        }

        return portals;
    }

    private Portal? GetPortal(int firstIndex, int secondIndex)
    {
        var first = Nodes[firstIndex];
        for (int i = 0; i < 3; i++)
        {
            if (first.Neighbours[i] == secondIndex)
            {
                var left = first.Vertices[i];
                var right = first.Vertices[(i + 1) % 3];
                return new Portal(left, right);
            }
        }

        return null;
    }

    private static int Sign(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        return (b.X - a.X) * (c.Y - a.Y)
               - (b.Y - a.Y) * (c.X - a.X);
    }


    private double GetCentroidDistance(int firstIndex, int secondIndex)
    {
        if (!AreNeighbours(firstIndex, secondIndex))
            return -1;

        var firstNodeCentroid = Nodes[firstIndex].Centroid;
        var secondNodeCentroid = Nodes[secondIndex].Centroid;

        var dx = firstNodeCentroid.X - secondNodeCentroid.X;
        var dy = firstNodeCentroid.Y - secondNodeCentroid.Y;

        return Math.Sqrt((dx * dx + dy * dy).Evaluate());
    }

    private bool AreNeighbours(int firstIndex, int secondIndex)
    {
        var firstNodeNeighbours = Nodes[firstIndex].Neighbours;
        var secondNodeNeighbours = Nodes[secondIndex].Neighbours;

        var firstHasSecond = false;
        var secondHasFirst = false;

        for (int i = 0; i < 3; i++)
        {
            if (firstNodeNeighbours[i] == secondIndex)
                firstHasSecond = true;
            if (secondNodeNeighbours[i] == firstIndex)
                secondHasFirst = true;
        }

        return firstHasSecond && secondHasFirst;
    }

    public override string ToString()
    {
        var s = $"NavMesh: {Nodes.Count} nodes";
        for (int i = 0; i < Nodes.Count; i++)
        {
            var node = Nodes[i];
            var centroid = node.Centroid.Evaluate().Round(1);
            s += $"\nNode {i}: Centroid = {centroid}\tA = {node.Vertices[0]}\t";
            string neighbours = string.Join(", ", node.Neighbours);
            s += $"Neighbours: [{neighbours}]";
        }

        s += $"\nGrid: {Grid}";

        return s;
    }
}