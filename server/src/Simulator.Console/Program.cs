using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Console;

public class Program
{
    static void Main(string[] args) 
    {
        Polygon polygon = new();
        // Unit square
        List<Vector2Int> vertices =
        [
            new (0, 0),
            new (0, 1),
            new (1, 1),
            new (1, 0)
        ];

        polygon.vertices = vertices;
        polygon.direction = Direction.CCW;
        
        List<Polygon> polygons = [polygon];

        var generator = new NavMeshGenerator();
        generator.GenerateNavMesh(polygons);
    }
}

