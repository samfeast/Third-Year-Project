using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Geometry;

public class NavMesh
{
    public class Node(Triangle triangle)
    {
        public readonly Vector2Int[] Vertices = [triangle.A, triangle.B, triangle.C];
        public readonly int[] Neighbours = [-1, -1, -1];
        public Vector2Fraction Centroid = triangle.GetCentroid();
    }
    
    public readonly List<Node> Nodes = [];
    //private UniformGrid grid;
    public int Count => Nodes.Count;

    // Add a node and return its index in the nodes list
    public int AddNode(Node node)
    {
        Nodes.Add(node);
        return Nodes.Count - 1;
    }

    public void AddNeighbour(SharedEdge edge)
    {
        Nodes[edge.TriangleIndex1].Neighbours[edge.EdgeIndex1] = edge.TriangleIndex2;
        Nodes[edge.TriangleIndex2].Neighbours[edge.EdgeIndex2] = edge.TriangleIndex1;
    }

    public override string ToString()
    {
        var s = $"NavMesh: {Nodes.Count} nodes";
        for (int i = 0; i < Nodes.Count; i++)
        {
            var node = Nodes[i];
            var centroid = node.Centroid.Evaluate().Round(1);
            s += $"\nNode {i}: Centroid = {centroid}\tA = {node.Vertices[0]}\t";
            string neighbors = string.Join(", ", node.Neighbours);
            s += $"Neighbours: [{neighbors}]";
        }

        return s;
    }
}