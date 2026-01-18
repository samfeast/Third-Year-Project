using Clipper2Lib;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry.Utils;

public class ClipperConversions
{
    public static Path64 LinkedListToPath64(LinkedList<Vector2Int> vertices)
    {
        List<Point64> listPoint64 = [];
        var node = vertices.First;
        while (node != null)
        {
            listPoint64.Add(new Point64(node.Value.X, node.Value.Y));
            node = node.Next;
        }
        return new Path64(listPoint64);

    }

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