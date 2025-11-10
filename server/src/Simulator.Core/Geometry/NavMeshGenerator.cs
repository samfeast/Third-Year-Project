using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public class NavMeshGenerator
{
    public List<Polygon> GenerateNavMesh(List<Polygon> polygons)
    {
        ITriangulator triangulator = new EarClippingTriangulator();
        List<Triangle> triangles = triangulator.Triangulate(polygons);

        Console.WriteLine($"Geometry divided into {triangles.Count} triangles");
        foreach (var triangle in triangles)
        {
            Console.WriteLine(triangle);
        }
        
        return new List<Polygon>();
    }
}