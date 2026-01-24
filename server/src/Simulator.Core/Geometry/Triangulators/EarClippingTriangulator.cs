using System.Diagnostics;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Geometry.Triangulators;

// Implementation of ITriangulator which triangulates geometry using ear clipping algorithm
public class EarClippingTriangulator : ITriangulator
{

    private LinkedList<Vector2Int> _vertices = [];
    private HashSet<LinkedListNode<Vector2Int>> _convexVertices = [];
    private HashSet<LinkedListNode<Vector2Int>> _reflexVertices = [];
    private HashSet<LinkedListNode<Vector2Int>> _earVertices = [];
    
    public List<Triangle> Triangulate(InputGeometry inputGeometry)
    {
        _vertices = PolygonBuilder.BuildVertexList(inputGeometry);

        // Handle edge cases where a triangle cannot be formed, or only one triangle can be formed
        if (_vertices.Count < 3) 
            return [];
        if (_vertices.Count == 3)
            return [ GetLastTriangle() ];
        
        // Classify all vertices as convex or reflex and find ears
        ClassifyVertices();
        ComputeEars();
        
        List<Triangle> triangles = [];
        // Clipping loop
        while (_vertices.Count > 3)
        {
            var ear = _earVertices.First();
            var triangle = ClipEar(ear);
            triangles.Add(triangle);
        }
        
        triangles.Add(GetLastTriangle());
        
        return triangles;
    }
    
    // Group all vertices into the convex and reflex sets
    private void ClassifyVertices()
    {
        var node = _vertices.First;
        while (node != null)
        {
            ClassifyVertex(node);
            node = node.Next;
        }
    }

    // Ensure the specified vertex is in exactly one of the convex/reflex sets
    // Returns true if the vertex is convex, false otherwise
    private bool ClassifyVertex(LinkedListNode<Vector2Int> node)
    {
        var (prevNode, nextNode) = GetNeighbours(node);
        if (IsConvex(prevNode.Value, node.Value, nextNode.Value ))
        {
            _convexVertices.Add(node);
            _reflexVertices.Remove(node);
            return true;
        }

        _reflexVertices.Add(node);
        _convexVertices.Remove(node);
        return false;

    }

    // Use the cross product to determine if vertex curr is convex (given its neighbours)
    private static bool IsConvex(Vector2Int prev, Vector2Int curr, Vector2Int next)
    {
        return Cross(curr - prev, next - curr) > 0;
    }
    
    // Add ear vertices to the set
    // Only call this when _convexVertices is up to date
    private void ComputeEars()
    {
        foreach (var node in _convexVertices)
        {
            if (IsEar(node))
                _earVertices.Add(node);
        }
    }
    
    // Check if a given vertex is an ear
    private bool IsEar(LinkedListNode<Vector2Int> tipNode)
    {
        var (prevNode, nextNode) = GetNeighbours(tipNode);

        var triangle = new Triangle(prevNode.Value, tipNode.Value, nextNode.Value);

        // Now check if any reflex vertices are in the triangle created by the three vertices
        foreach (var node in _reflexVertices)
        {
            if (node == prevNode || node == tipNode || node == nextNode)
                continue;
            if (triangle.ContainsPoint(node.Value))
                return false;
        }
        
        return true;
    }

    // Single iteration of clipping loop
    private Triangle ClipEar(LinkedListNode<Vector2Int> earNode)
    {
        var (prevNode, nextNode) = GetNeighbours(earNode);
        var triangle = new Triangle(prevNode.Value, earNode.Value, nextNode.Value);

        _vertices.Remove(earNode);
        _convexVertices.Remove(earNode);
        _reflexVertices.Remove(earNode);
        _earVertices.Remove(earNode);

        // Classify the previous node as convex/reflex
        // If its convex, check if it's an ear and update the ear set
        if (ClassifyVertex(prevNode) && IsEar(prevNode))
            _earVertices.Add(prevNode);
        else
            _earVertices.Remove(prevNode);
        
        if (ClassifyVertex(nextNode) && IsEar(nextNode))
            _earVertices.Add(nextNode);
        else
            _earVertices.Remove(nextNode);

        return triangle;
    }

    // Get the previous and next nodes in a circular manner
    private (LinkedListNode<Vector2Int>, LinkedListNode<Vector2Int>) GetNeighbours(LinkedListNode<Vector2Int> node)
    {
        Debug.Assert(_vertices.Count > 0, "Tried to get neighbours for empty vertex list");
        var prevNode = node.Previous ?? _vertices.Last;
        var nextNode = node.Next ?? _vertices.First;

        // We know prev and next are not null as this is checked by the assertion
        return (prevNode!, nextNode!);
    }
    
    // Get the triangle formed by the first, second, and last elements in the vertices list
    // Only to be used when there are exactly three vertices in the list
    private Triangle GetLastTriangle()
    {
        Debug.Assert(_vertices.Count == 3, "Tried to get last triangle when _vertices.Count != 3");
        return new Triangle(_vertices.First!.Value, _vertices.First.Next!.Value, _vertices.Last!.Value);
    }
    
    // 2D cross product used to determine convexity
    private static int Cross(Vector2Int a, Vector2Int b)
    {
        return a.X * b.Y - a.Y * b.X;
    }
}