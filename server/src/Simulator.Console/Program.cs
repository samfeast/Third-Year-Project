using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

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
            polygon.vertices = GeometryFileIo.ReadVerticesFromFile(inPath);
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
            System.Console.WriteLine($"Geometry divided into {triangles.Count} triangles and saved to disk");
            GeometryFileIo.WriteTrianglesToFile(outPath, triangles);
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
}

