using Clipper2Lib;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry.Utils;

// Utility methods to and from types used by the Clipper2 library
public static class ClipperConversions
{
    // Convert a list of Vector2Int's to a Clipper2 Path64
    public static Path64 ListToPath64(List<Vector2Int> vertices)
    {
        List<Point64> listPoint64 = [];
        foreach (var v in vertices)
        {
            listPoint64.Add(new Point64(v.X, v.Y));
        }
        return new Path64(listPoint64);
    }

    // Convert a Paths64 object to a list of triangles
    // Coordinates of points on a Path64 are long, but Vector2Int takes int - risk of overflow
    public static List<Triangle> Paths64ToTriangles(Paths64 paths)
    {
        List<Triangle> triangles = [];
        foreach (var path in paths)
        {
            var a = new Vector2Int((int)path[0].X, (int)path[0].Y);
            var b = new Vector2Int((int)path[1].X, (int)path[1].Y);
            var c = new Vector2Int((int)path[2].X, (int)path[2].Y);
            
            triangles.Add(new Triangle(a, b, c));
        }

        return triangles;
    }
}