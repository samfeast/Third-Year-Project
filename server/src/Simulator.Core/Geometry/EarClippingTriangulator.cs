using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public class EarClippingTriangulator : ITriangulator 
{
    public List<Triangle> Triangulate(List<Polygon> input) 
    {
        var polygon = input[0];
        
        // Construct doubly linked list with vertices in CCW order
        var vertices = new LinkedList<Vector2Int>();
        
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
        
        return [];
    }
}