using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public class EarClippingTriangulator : ITriangulator
{

    private LinkedList<Vector2Int> _vertices = [];
    private List<LinkedListNode<Vector2Int>> _convexVertices = [];
    private List<LinkedListNode<Vector2Int>> _reflexVertices = [];
    private List<LinkedListNode<Vector2Int>> _earVertices = [];
    public List<Triangle> Triangulate(List<Polygon> polygons) 
    {
        // Construct doubly linked list with vertices in CCW order
        // TODO: Handle polygons with holes by ordering vertices according to bridge
        _vertices = BuildVertexList(polygons);

        // Handle edge cases where a triangle cannot be formed, or only one triangle can be formed
        if (_vertices.Count < 3) return [];

        if (_vertices.Count == 3)
        {
            var v0 = _vertices.First!.Value;
            var v1 = _vertices.First.Next!.Value;
            var v2 = _vertices.Last!.Value;
            return [ new Triangle(v0, v1, v2) { direction = Direction.CCW } ];
        }
        
        ClassifyVertices();

        foreach (var v in _convexVertices)
        {
            Console.WriteLine($"Convex: {v.Value}");
        }
        foreach (var v in _reflexVertices)
        {
            Console.WriteLine($"Reflex: {v.Value}");
        }
        
        ComputeEars();
        
        foreach (var v in _earVertices)
        {
            Console.WriteLine($"Ear: {v.Value}");
        }
        
        /*
        
        var node = _vertices.First;
        while (node != null)
        {
            if (IsEar(node))
            {
                Console.WriteLine($"{node.Value} is an ear");
            }
            else
            {
                Console.WriteLine($"{node.Value} is not an ear");
            }

            node = node.Next;
        }
        */
        return [];
    }

    private LinkedList<Vector2Int> BuildVertexList(List<Polygon> polygons)
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

    private void ClassifyVertices()
    {
        var node = _vertices.First;
        while (node != null)
        {
            var prevNode = node.Previous ?? _vertices.Last;
            var nextNode = node.Next ?? _vertices.First;
            
            if (Cross(prevNode!.Value, node!.Value, nextNode!.Value) > 0)
                _convexVertices.Add(node);
            else
                _reflexVertices.Add(node);

            node = node.Next;
        }
    }

    private void ComputeEars()
    {
        foreach (var node in _convexVertices)
        {
            if (IsEar(node))
                _earVertices.Add(node);
        }
    }
    
    private bool IsEar(LinkedListNode<Vector2Int> tipNode)
    {
        // Get the previous and next nodes in a circular manner
        var prevNode = tipNode.Previous ?? _vertices.Last;
        var nextNode = tipNode.Next ?? _vertices.First;
        

        var triangle = new Triangle(prevNode!.Value, tipNode.Value, nextNode!.Value);

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
    
    private int Cross(Vector2Int v0, Vector2Int v1, Vector2Int v2)
    {
        return (v1.X - v0.X)*(v2.Y - v1.Y) - (v1.Y - v0.Y)*(v2.X - v1.X);
    }
}