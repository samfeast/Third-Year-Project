using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public class NavMeshGenerator
{

    private const bool USE_CDT = true;
    public List<Triangle> GenerateNavMesh(Polygon positive, List<Polygon> negatives)
    {
        ITriangulator triangulator;
        if (USE_CDT)
            triangulator = new ConstrainedDelaunayTriangulator();
        else
            triangulator = new EarClippingTriangulator();
        
        List<Triangle> triangles = triangulator.Triangulate(positive, negatives);
        
        return triangles;
    }
}