using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry.Utils;

// Utility methods for parsing and storing geometric data using a custom format
public static class GeometryFileIo
{
    // Parse geometric data from a file and return the corresponding list of polygons
    // A polygon is defined as a list of vertices on consecutive rows of a csv file
    // An empty row signifies the end of one polygon and the start of the next
    // The first polygon in the csv and returned list represents the outer polygon, the rest should represent holes
    // The outer polygon is expected to have a CCW winding, the rest a CW winding - but this is not enforced here
    public static List<Polygon> ReadGeometryFromFile(string path)
    {
        List<Polygon> polygons = [];
        List<Vector2Int> parsedVertices = [];
        
        foreach (var line in File.ReadLines(path))
        {
            // When encountering an empty line dump the parsed vertices to form a polygon
            if (string.IsNullOrWhiteSpace(line) && parsedVertices.Count > 0)
            {
                polygons.Add(new Polygon([..parsedVertices]));
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

        // Form the final polygon after the last row has been read
        if (parsedVertices.Count > 0)
            polygons.Add(new Polygon([..parsedVertices]));
        
        return polygons;
    }
    
    // Write a list of triangles to the specified csv file
    // Each row represents one triangle: x0,y0,x1,y1,x2,y2
    public static void WriteTrianglesToFile(string path, List<Triangle> triangles)
    {
        using var writer = new StreamWriter(path);

        foreach (var t in triangles)
        {
            writer.WriteLine($"{t.A.X},{t.A.Y},{t.B.X},{t.B.Y},{t.C.X},{t.C.Y}");
        }
    }
}