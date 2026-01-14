using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry.Utils;

public static class GeometryFileIo
{
    public static List<Vector2Int> ReadVerticesFromFile(string path)
    {
        var result = new List<Vector2Int>();

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(',');

            if (parts.Length != 2)
                throw new FormatException($"Invalid CSV line: {line}");

            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);

            result.Add(new Vector2Int(x, y));
        }

        return result;
    }
    
    public static void WriteTrianglesToFile(string path, List<Triangle> triangles)
    {
        using var writer = new StreamWriter(path);

        foreach (var t in triangles)
        {
            writer.WriteLine($"{t.A.X},{t.A.Y},{t.B.X},{t.B.Y},{t.C.X},{t.C.Y}");
        }
    }
}