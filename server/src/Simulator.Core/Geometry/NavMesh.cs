using System.Diagnostics;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Shapes;

namespace Simulator.Core.Geometry;

public class NavMesh(int gridSize)
{
    public struct Node(Triangle triangle)
    {
        public readonly Vector2Int[] Vertices = [triangle.A, triangle.B, triangle.C];
        public readonly int[] Neighbours = [-1, -1, -1];
        public readonly Vector2Fraction Centroid = triangle.GetCentroid();
        public readonly Triangle Triangle = triangle;
    }

    public struct Portal(Vector2Fraction left, Vector2Fraction right)
    {
        public readonly Vector2Fraction Left = left;
        public readonly Vector2Fraction Right = right;
    }
    
    public readonly List<Node> Nodes = [];
    public readonly UniformGrid Grid = new(gridSize,gridSize);

    public List<Vector2Fraction> Navigate(Vector2 source, Vector2 destination)
    {
        // Degrades when starting and ending on boundaries
        var startNode = GetCurrentNode(source.X, source.Y)[0];
        var endNode = GetCurrentNode(destination.X, destination.Y)[0];

        var channel = GetChannel(startNode, endNode);
        
        var portals = GetPortals(channel);
        
        portals.Add(new Portal(destination.ToVector2Fraction(), destination.ToVector2Fraction()));
        
        var path = Funnel(source.ToVector2Fraction(), destination.ToVector2Fraction(), portals);

        return path;
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

    private List<Portal> GetPortals(List<int> channel)
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

    private List<Vector2Fraction> Funnel(Vector2Fraction source, Vector2Fraction destination, List<Portal> portals)
    {
        var result = new List<Vector2Fraction> { source };

        var index = 0;
        var apex = source;
        while (index != -1)
        {
            (apex, index) = GetNextFixedPoint(apex, portals, index);
            result.Add(apex);
            if (index == portals.Count - 1)
            {
                result.Add(destination);
                break;
            }
        }
        
        return result;
    }

    private static (Vector2Fraction, int) GetNextFixedPoint(Vector2Fraction apex, List<Portal> portals, int portalIndex)
    {
        var leftIndex = portalIndex;
        var rightIndex = portalIndex;
        
        var left = portals[leftIndex].Left;
        var right = portals[rightIndex].Right;
        
        for (int i = portalIndex; i < portals.Count - 1; i++)
        {
            // If left and right have converged to the same vertex then it must be the destination, so exit.
            if (left == right)
            {
                return (left, -1);
            }
            
            var candidateLeft = portals[i+1].Left;
            var candidateRight = portals[i+1].Right;
            
            // If candidateLeft is outside the funnel on the left side, don't update the left edge (it would widen the full)
            if (Sign(apex, left, candidateLeft) <= LongFraction.Zero)
            {
                // If candidateLeft is inside the funnel, update the left edge (to shrink the funnel)
                if (Sign(apex, right, candidateLeft) >= LongFraction.Zero)
                {
                    leftIndex++;
                    left = candidateLeft;
                }
                // If candidateLeft crosses the right edge, return the current right edge (it's a fixed point)
                else
                {
                    return (right, GetNextFunnelIndex(portals, right, rightIndex, false)+1);
                }
            }

            // If candidateRight is outside the funnel on the right side, don't update the right edge (it would widen the full)
            if (Sign(apex, right, candidateRight) >= LongFraction.Zero)
            {
                // If candidateRight is inside the funnel, update the right edge (to shrink the funnel)
                if (Sign(apex, left, candidateRight) <= LongFraction.Zero)
                {
                    rightIndex++;
                    right = candidateRight;
                }
                // If candidateRight crosses the left edge, return the current left edge (it's a fixed point)
                else
                {
                    return (left, GetNextFunnelIndex(portals, left, leftIndex, true)+1);
                }
            }
        }
        
        // Unreachable
        return (apex, portalIndex);
    }

    private static int GetNextFunnelIndex(List<Portal> portals, Vector2Fraction point, int startIndex, bool isLeft)
    {
        for (int i = startIndex; i < portals.Count - 1; i++)
        {
            var nextPortal = portals[i + 1];
            if (isLeft && !nextPortal.Left.Equals(point))
                return i;
            if (!isLeft && !nextPortal.Right.Equals(point))
                return i;
        }

        // Unreachable
        return -1;
    }
    
    private static LongFraction Sign(Vector2Fraction a, Vector2Fraction b, Vector2Fraction c)
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
        var dy =  firstNodeCentroid.Y - secondNodeCentroid.Y;

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