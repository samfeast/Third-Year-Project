using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Console;

public class Program
{
    static void Main(string[] args)
    {
        // If two args are given (input file, output file), use file IO, otherwise don't
        bool useFileIo = args.Length == 2;

        List<Polygon> polygons = new();
        if (useFileIo)
        {
            string inPath = args[0];
            if (!File.Exists(inPath))
            {
                System.Console.WriteLine($"File not found: {inPath}");
                return;
            }
            Polygon polygon = new();
            polygon.vertices = LoadVerticesFromFile(inPath);
            polygon.direction = Direction.CCW;
            polygons.Add(polygon);
        }
        else
        {
            Polygon polygon = new();
            polygon.vertices = new();
            polygon.direction = Direction.CCW;
            polygons.Add(polygon);
        }

        var generator = new NavMeshGenerator();
        var triangles = generator.GenerateNavMesh(polygons);

        if (useFileIo)
        {
            string outPath = args[1];
            WriteTrianglesToFile(outPath, triangles);
        }
        else
        {
            System.Console.WriteLine($"Geometry divided into {triangles.Count} triangles");
            foreach (var triangle in triangles)
            {
                System.Console.WriteLine(triangle);
            }
        }
    }
    
    private static List<Vector2Int> LoadVerticesFromFile(string path)
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

    private static void WriteTrianglesToFile(string path, List<Triangle> triangles)
    {
        using var writer = new StreamWriter(path);

        foreach (var t in triangles)
        {
            writer.WriteLine($"{t.A.X},{t.A.Y},{t.B.X},{t.B.Y},{t.C.X},{t.C.Y}");
        }
    }
}

