using Clipper2Lib;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Geometry;

public class ConstrainedDelaunayTriangulator : ITriangulator
{
    public List<Triangle> Triangulate(Polygon positive, List<Polygon> negatives)
    {
        var vertices = PolygonBuilder.BuildVertexList(positive, negatives);

        var path = ClipperConversions.LinkedListToPath64(vertices);

        Clipper.Triangulate([path], out var output);

        return ClipperConversions.Paths64ToTriangles(output);
    }
}