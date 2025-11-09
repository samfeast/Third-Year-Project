using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core;

public class Class1
{
    static void Main(string[] args) 
    {
        Polygon polygon = new();
        List<Vector2Int> vertices =
        [
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0)
        ];

        polygon.vertices = vertices;
        polygon.direction = Direction.CCW;
        
        List<Polygon> polygons = [polygon];

        var generator = new NavMeshGenerator();
        generator.GenerateNavMesh(polygons);
    }
}
