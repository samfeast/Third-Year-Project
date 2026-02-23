using Clipper2Lib;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry.Utils;

public class Clipper2Conversions
{
    // Convert a list of Vector2Int's to a Clipper2 Path64
    public static Path64 ListToPath64(List<Vector2Int> vertices)
    {
        var listPoint64 = new Point64[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            listPoint64[i] = new Point64(vertices[i].X, vertices[i].Y);
        }
        
        return new Path64(listPoint64);
    }

    public static List<Vector2Int> Path64ToList(Path64 path)
    {
        var vertices = new List<Vector2Int>(path.Count);

        foreach (var v in path)
        {
            vertices.Add(new Vector2Int((int) v.X, (int) v.Y));
        }

        return vertices;
    }
}