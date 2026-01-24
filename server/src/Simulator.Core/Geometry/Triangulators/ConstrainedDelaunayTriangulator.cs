using System.Diagnostics;
using Clipper2Lib;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Geometry.Triangulators;

// Implementation of ITriangulator which uses the Clipper2 library
public class ConstrainedDelaunayTriangulator : ITriangulator
{
    public List<Triangle> Triangulate(InputGeometry inputGeometry)
    {
        // Convert the vertex list to a Path64 from Clipper2
        var positivePath = ClipperConversions.ListToPath64(inputGeometry.Positive.Vertices);
        Paths64 paths = [positivePath];
        foreach (var polygon in inputGeometry.Negatives)
        {
            var negativePath = ClipperConversions.ListToPath64(polygon.Vertices);
            paths.Add(negativePath);
        }

        // Run Clipper2 triangulation algorithm (CDT)
        var result = Clipper.Triangulate(paths, out var output);
        Debug.Assert(result == TriangulateResult.success, "Triangulation failed");

        // Convert the resulting Paths64 object to a list of triangles
        return ClipperConversions.Paths64ToTriangles(output);
    }
}