using Simulator.Core.Geometry.Primitives;

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
    
    public readonly List<Node> Nodes = [];
    public readonly UniformGrid Grid = new(gridSize,gridSize);

    public List<int> GetCurrentNode(double x, double y)
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

    public List<int> GetChannel(int sourceIndex, int destinationIndex)
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

    public double GetCentroidDistance(int firstIndex, int secondIndex)
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