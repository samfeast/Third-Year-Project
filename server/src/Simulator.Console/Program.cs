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
            new (3, 48),
            new (52, 8),
            new (99, 50),
            new (138, 25),
            new (175, 77),
            new (131, 72),
            new (111, 113),
            new (72, 43),
            new (26, 55),
            new (29, 100)
        ];

        polygon.vertices = vertices;
        polygon.direction = Direction.CCW;
        
        List<Polygon> polygons = [polygon];

        var generator = new NavMeshGenerator();
        generator.GenerateNavMesh(polygons);
    }
}

