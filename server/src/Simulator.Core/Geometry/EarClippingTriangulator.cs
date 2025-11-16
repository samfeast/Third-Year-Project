using System.Diagnostics;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public class EarClippingTriangulator : ITriangulator
{

    private LinkedList<Vector2Int> _vertices = [];
    private HashSet<LinkedListNode<Vector2Int>> _convexVertices = [];
    private HashSet<LinkedListNode<Vector2Int>> _reflexVertices = [];
    private HashSet<LinkedListNode<Vector2Int>> _earVertices = [];
    public List<Triangle> Triangulate(List<Polygon> polygons) 
    {
        // Construct doubly linked list with vertices in CCW order
        // TODO: Handle polygons with holes by ordering vertices according to bridge
        _vertices = BuildVertexList(polygons);

        // Handle edge cases where a triangle cannot be formed, or only one triangle can be formed
        if (_vertices.Count < 3) 
            return [];
        if (_vertices.Count == 3)
            return [ GetLastTriangle() ];
        
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
    
    // Take list of input polygons and return correctly ordered vertex list
    private static LinkedList<Vector2Int> BuildVertexList(List<Polygon> polygons)
    {
        var vertices = new LinkedList<Vector2Int>();
        foreach (var polygon in polygons)
        {
            if (polygon.direction == Direction.CCW) 
            {
                foreach (var vertex in polygon.vertices) {
                    vertices.AddLast(vertex);
                } 
            }
            else 
            {
                foreach (var vertex in polygon.vertices) {
                    vertices.AddFirst(vertex);
                } 
            }
        }

        return vertices;
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
    private bool ClassifyVertex(LinkedListNode<Vector2Int> vertex)
    {
        var (prevNode, nextNode) = GetNeighbours(vertex);
        if (Cross(prevNode.Value, vertex.Value, nextNode.Value) > 0)
        {
            _convexVertices.Add(vertex);
            _reflexVertices.Remove(vertex);
            return true;
        }

        _reflexVertices.Add(vertex);
        _convexVertices.Remove(vertex);
        return false;

    }
    
    // Add ear vertices to the set
    // NOTE: Only call this when _convexVertices is up to date
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

        return (prevNode!, nextNode!);
    }
    
    // Get the triangle formed by the first, second, and last elements in the vertices list
    // Only to be used when there are exactly three vertices in the list
    private Triangle GetLastTriangle()
    {
        Debug.Assert(_vertices.Count == 3, "Tried to get last triangle when _vertices.Count != 3");
        return new Triangle(_vertices.First!.Value, _vertices.First.Next!.Value, _vertices.Last!.Value);
    }
    
    // 2D cross product to determine convexity
    private static int Cross(Vector2Int v0, Vector2Int v1, Vector2Int v2)
    {
        return (v1.X - v0.X)*(v2.Y - v1.Y) - (v1.Y - v0.Y)*(v2.X - v1.X);
    }
}