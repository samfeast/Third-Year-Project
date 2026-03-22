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
        public readonly int DoubleArea = triangle.GetDoubleArea();
    }

    public struct Portal(Vector2Fraction left, Vector2Fraction right)
    {
        public readonly Vector2Fraction Left = left;
        public readonly Vector2Fraction Right = right;
    }

    public readonly List<Node> Nodes = [];
    public readonly UniformGrid Grid = new(cellSize);
    public readonly List<int> CumulativeDoubleAreas = [];

    public List<Portal> GetPortals(Vector2 source, Vector2 destination)
    {
        // Degrades when starting and ending on boundaries
        var startNode = GetCurrentNode(source.X, source.Y)[0];
        var endNode = GetCurrentNode(destination.X, destination.Y)[0];

        // Pathfinding using Dijkstra's
        var channel = GetChannel(startNode, endNode);
        
        var portals = GetPortalsFromChannel(channel);
        // Add degenerate portals for destination
        portals.Add(new Portal(destination.ToVector2Fraction(), destination.ToVector2Fraction()));

        return portals;
    }

    public List<Vector2Fraction> GetFullFunnelPath(Vector2Fraction position, List<Portal> portals)
    {
        List<Vector2Fraction> path = [position];
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
            var crossedPortals = 0;
            foreach (var portal in portals)
            {
                // Condition met if nextPoint is to the right of the left->right portal vector
                if (Sign(portal.Left, portal.Right, nextPoint) >= LongFraction.Zero)
                    crossedPortals++;
                else
                    break;
            }

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

    public Vector2Fraction GetNextTurningPoint(Vector2Fraction position, List<Portal> remainingPortals)
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
            if (Sign(portalApex, portalRight, newRight) >= LongFraction.Zero)
            {
                if (portalApex == portalRight ||
                    Sign(portalApex, portalLeft, newRight) < LongFraction.Zero)
                {
                    portalRight = newRight;
                }
                else
                {
                    return portalLeft;
                }
            }

            // Left boundary
            if (Sign(portalApex, portalLeft, newLeft) <= LongFraction.Zero)
            {
                if (portalApex == portalLeft ||
                    Sign(portalApex, portalRight, newLeft) > LongFraction.Zero)
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

    private List<int> GetCurrentNode(double x, double y)
    {
        var possibleNodeIndices = Grid.Get(x, y);

        var rtn = new List<int>(possibleNodeIndices.Count);
        foreach (var i in possibleNodeIndices)
        {
            if (Nodes[i].Triangle.ContainsPoint(new Vector2(x, y)))
                rtn.Add(i);
        }

        Debug.Assert(rtn.Count > 0, $"Failed to find node: {x} {y}");
        if (rtn.Count > 1)
            Console.WriteLine($"WARNING: Position inside {rtn.Count} nodes");
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
                var left = first.Vertices[i].ToVector2Fraction();
                var right = first.Vertices[(i + 1) % 3].ToVector2Fraction();
                return new Portal(left, right);
            }
        }

        return null;
    }

    public static LongFraction Sign(Vector2Fraction a, Vector2Fraction b, Vector2Fraction c)
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