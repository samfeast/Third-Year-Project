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
        
        s += $"\nGrid: {Grid}";

        return s;
    }
}