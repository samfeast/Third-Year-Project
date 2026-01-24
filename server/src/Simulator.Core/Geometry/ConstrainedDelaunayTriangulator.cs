using System.Diagnostics;
using Clipper2Lib;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Geometry;

// Implementation of ITriangulator which uses the Clipper2 library
public class ConstrainedDelaunayTriangulator : ITriangulator
{
    public List<Triangle> Triangulate(InputGeometry inputGeometry)
    {
        // Build the pseudosimple polygon to incorporate holes
        var vertices = PolygonBuilder.BuildVertexList(inputGeometry);

        // Convert the vertex linkedlist to a Path64 from Clipper2
        var path = ClipperConversions.LinkedListToPath64(vertices);

        // Run Clipper2 triangulation algorithm (CDT)
        var result = Clipper.Triangulate([path], out var output);
        Debug.Assert(result == TriangulateResult.success, "Triangulation failed");

        // Convert the resulting Paths64 object to a list of triangles
        return ClipperConversions.Paths64ToTriangles(output);
    }
}