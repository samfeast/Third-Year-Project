using Simulator.Core.Geometry.Shapes;
using Simulator.Core.Geometry.Utils;
using TriangleNet.Geometry;
using Clipper2Lib;
using Polygon = Simulator.Core.Geometry.Shapes.Polygon;

namespace Simulator.Core.Geometry.Triangulators;

// Implementation of ITriangulator which uses the Triangle.NET library
public class ConstrainedDelaunayTriangulator(int exclusionRad) : ITriangulator
{
    public List<Triangle> Triangulate(InputGeometry inputGeometry)
    {
        // Inflate polygons by exclusion radius to prevent agents hugging walls
        // Does not do error checking to see if this produces corridors which are too narrow
        var inflatedPositive = InflatePolygon(inputGeometry.Positive);
        var inflatedNegatives = new List<Polygon>(inputGeometry.Negatives.Count);
        foreach (var negative in inputGeometry.Negatives)
            inflatedNegatives.Add(InflatePolygon(negative));

        var polygon = TriangleNetConversions.InputGeometryToExtPolygon(inflatedPositive, inflatedNegatives);
        var mesh = polygon.Triangulate();
        
        var triangles = new List<Triangle>();
        foreach (var tri in mesh.Triangles)
        {
            triangles.Add(TriangleNetConversions.ExtTriangleToTriangle(tri));
        }

        return triangles;
    }

    private Polygon InflatePolygon(Polygon polygon)
    {
        Paths64 path = [Clipper2Conversions.ListToPath64(polygon.Vertices)];
        var inflatedPath = Clipper.InflatePaths(path, -exclusionRad, JoinType.Miter, EndType.Polygon);
        return new Polygon(Clipper2Conversions.Path64ToList(inflatedPath[0]));
    }
}