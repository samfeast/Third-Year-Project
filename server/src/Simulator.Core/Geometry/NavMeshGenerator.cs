using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public class NavMeshGenerator
{
    public List<Triangle> GenerateNavMesh(Polygon positive, List<Polygon> negatives)
    {
        ITriangulator triangulator = new EarClippingTriangulator();
        List<Triangle> triangles = triangulator.Triangulate(positive, negatives);
        
        return triangles;
    }
}