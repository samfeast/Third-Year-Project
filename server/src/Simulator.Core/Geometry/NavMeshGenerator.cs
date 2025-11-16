using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public class NavMeshGenerator
{
    public List<Triangle> GenerateNavMesh(List<Polygon> polygons)
    {
        ITriangulator triangulator = new EarClippingTriangulator();
        List<Triangle> triangles = triangulator.Triangulate(polygons);
        
        return triangles;
    }
}