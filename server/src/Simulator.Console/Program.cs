using System.Diagnostics;
using Simulator.Core;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Utils;
using Simulator.IO;

namespace Simulator.Console;

public class Program
{
    static void Main(string[] args)
    {
        // Expect two arguments: input file path, output file path 
        string inPath = args[0];
        string outPath = args[1];

        // Get a registry with all the deserialisers which can parse InputGeometry
        var readRegistry = DeserialiserRegistryFactory.Default<InputGeometry>();

        // Load the input geometry using whichever deserialiser works
        var inputGeometry = readRegistry.Load(inPath);

        var simulator = new SimulationEngine();
        var watch = Stopwatch.StartNew();
        var setupSuccess = simulator.SetupSimulation(inputGeometry);
        watch.Stop();
        Debug.Assert(setupSuccess, "Simulation Setup Failed");
        if (simulator.Mesh == null)
        {
            return;
        }
        System.Console.WriteLine($"Geometry divided into {simulator.Mesh.Nodes.Count} triangles in {watch.ElapsedMilliseconds}ms");
        
        // Save the navmesh using whichever serialiser works
        var writeRegistry = SerialiserRegistryFactory.Default<NavMesh>();
        var writeSuccess = writeRegistry.Save(outPath, simulator.Mesh, 1);
        Debug.Assert(writeSuccess, "Failed to write input geometry");
    }
}