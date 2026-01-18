using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;



public class NavMesh
{
    public class Node(Triangle triangle)
    {
        public readonly Vector2Int[] Vertices = [triangle.A, triangle.B, triangle.C];
        public readonly int[] Neighbours = [-1, -1, -1];
        public Vector2Fraction Centroid = triangle.GetCentroid();
    }
    
    private readonly List<Node> _nodes = [];
    //private UniformGrid grid;
    public int Count => _nodes.Count;

    // Add a node and return its index in the nodes list
    public int AddNode(Node node)
    {
        _nodes.Add(node);
        return _nodes.Count - 1;
    }

    public void AddNeighbour(SharedEdge edge)
    {
        _nodes[edge.TriangleIndex1].Neighbours[edge.EdgeIndex1] = edge.TriangleIndex2;
        _nodes[edge.TriangleIndex2].Neighbours[edge.EdgeIndex2] = edge.TriangleIndex1;
    }

    public override string ToString()
    {
        var s = $"NavMesh: {_nodes.Count} nodes";
        for (int i = 0; i < _nodes.Count; i++)
        {
            var node = _nodes[i];
            var centroid = node.Centroid.Evaluate().Round(1);
            s += $"\nNode {i}: Centroid = {centroid}\tA = {node.Vertices[0]}\t";
            string neighbors = string.Join(", ", node.Neighbours);
            s += $"Neighbours: [{neighbors}]";
        }

        return s;
    }
}