using System.Diagnostics;
using Simulator.Core;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Console;

public class Program
{
    static void Main(string[] args)
    {
        // If two args are given (input file, output file), use file IO, otherwise don't
        // Expect two arguments: input file path, output file path 

        string inPath = args[0];
        if (!File.Exists(inPath))
        {
            System.Console.WriteLine($"File not found: {inPath}");
            return;
        }
        List<Polygon> polygons = GeometryFileIo.ReadGeometryFromFile(inPath);
        Polygon positive = polygons[0];
        polygons.RemoveAt(0);
        List<Polygon> negatives = polygons;


        var simulator = new SimulationEngine();
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var setupSuccess = simulator.SetupSimulation(positive, negatives);
        watch.Stop();
        Debug.Assert(setupSuccess, "Simulation Setup Failed");

        // Select a serialiser and write to file
    }
}

