using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public class NavMeshGenerator
{
    private readonly bool _useCdt = true;
    
    public List<Triangle> GenerateNavMesh(Polygon positive, List<Polygon> negatives)
    {
        // Toggle between CDT (from Clipper2) or ear clipping (own implementation)
        ITriangulator triangulator;
        if (_useCdt)
            triangulator = new ConstrainedDelaunayTriangulator();
        else
            triangulator = new EarClippingTriangulator();
        
        List<Triangle> triangles = triangulator.Triangulate(positive, negatives);
        
        return triangles;
    }
}