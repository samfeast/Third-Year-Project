using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry.Utils;

public static class GeometryFileIo
{
    public static List<Polygon> ReadGeometryFromFile(string path)
    {
        List<Polygon> polygons = new();
        var parsedVertices = new List<Vector2Int>();
        
        foreach (var line in File.ReadLines(path))
        {
            // An empty row signifies the end of one polygon and the start of the next
            if (string.IsNullOrWhiteSpace(line) && parsedVertices.Count > 0)
            {
                polygons.Add(new Polygon(parsedVertices));
                parsedVertices.Clear();
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            string[] parts = line.Split(',');

            if (parts.Length != 2)
                throw new FormatException($"Invalid CSV line: {line}");

            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);
            
            parsedVertices.Add(new Vector2Int(x, y));
        }

        if (parsedVertices.Count > 0)
            polygons.Add(new Polygon(parsedVertices));
        
        return polygons;
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