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

        Polygon positive;
        List<Polygon> negatives;
        if (useFileIo)
        {
            string inPath = args[0];
            if (!File.Exists(inPath))
            {
                System.Console.WriteLine($"File not found: {inPath}");
                return;
            }
            List<Polygon> polygons = GeometryFileIo.ReadGeometryFromFile(inPath);
            positive = polygons[0];
            polygons.RemoveAt(0);
            negatives = polygons;
        }
        else
        {
            positive = new Polygon();
            negatives = new List<Polygon>();
        }
        
        var generator = new NavMeshGenerator();
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var triangles = generator.GenerateNavMesh(positive, negatives);
        watch.Stop();

        if (useFileIo)
        {
            string outPath = args[1];
            System.Console.WriteLine($"Geometry divided into {triangles.Count} triangles in {watch.ElapsedMilliseconds}ms and saved to disk");
            GeometryFileIo.WriteTrianglesToFile(outPath, triangles);
        }
        else
        {
            System.Console.WriteLine($"Geometry divided into {triangles.Count} triangles in {watch.ElapsedMilliseconds}ms");
            foreach (var triangle in triangles)
            {
                System.Console.WriteLine(triangle);
            }
        }
    }
}

