using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;
using TriangleNet.Geometry;

namespace Simulator.Core.Geometry.Triangulators;

// Implementation of ITriangulator which uses the Triangle.NET library
public class ConstrainedDelaunayTriangulator : ITriangulator
{
    public List<Triangle> Triangulate(InputGeometry inputGeometry)
    {
        var polygon = TriangleNetConversions.InputGeometryToExtPolygon(inputGeometry);
        var mesh = polygon.Triangulate();
        
        var triangles = new List<Triangle>();
        foreach (var tri in mesh.Triangles)
        {
            triangles.Add(TriangleNetConversions.ExtTriangleToTriangle(tri));
        }

        return triangles;
    }
}