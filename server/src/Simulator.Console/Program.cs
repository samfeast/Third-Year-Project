using System.Diagnostics;
using Simulator.Core;
using Simulator.Core.Geometry.Primitives;
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
        var registry = DeserialiserRegistryFactory.Default<InputGeometry>();

        // Load the input geometry using whichever deserialiser works
        var inputGeometry = registry.Load(inPath);

        var simulator = new SimulationEngine();
        var watch = Stopwatch.StartNew();
        var setupSuccess = simulator.SetupSimulation(inputGeometry);
        watch.Stop();
        Debug.Assert(setupSuccess, "Simulation Setup Failed");

        // Select a serialiser and write to file
    }
}

