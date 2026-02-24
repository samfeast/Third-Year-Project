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
        string meshOutPath = args[1];
        string snapshotsOutPath = args[2];

        // Get a registry with all the deserialisers which can parse InputGeometry
        var readRegistry = DeserialiserRegistryFactory.Default<InputGeometry>();

        // Load the input geometry using whichever deserialiser works
        var inputGeometry = readRegistry.Load(inPath);
        
        System.Console.WriteLine("Geometry loaded");

        int numAgents = 50;
        double timeStep = 0.05f;

        var watch = Stopwatch.StartNew();
        var config = new SimulationConfig(inputGeometry, timeStep, numAgents, 100);
        var simulator = new SimulationEngine(config);
        watch.Stop();
        
        
        System.Console.WriteLine($"Geometry divided into {simulator.Mesh.Nodes.Count} triangles in {watch.ElapsedMilliseconds}ms");
        
        // Save the navmesh using whichever serialiser works
        var meshWriteRegistry = SerialiserRegistryFactory.Default<NavMesh>();
        var meshWriteSuccess = meshWriteRegistry.Save(meshOutPath, simulator.Mesh, 1);
        Debug.Assert(meshWriteSuccess, "Failed to write input geometry");
        
        watch = Stopwatch.StartNew();

        List<SimulationSnapshot> snapshots = [];        

        while (true)
        {
            var snapshot = simulator.StepSimulation();
            snapshots.Add(snapshot);

            if (snapshot.Step % 60 == 0)
            {
                System.Console.WriteLine($"{Math.Round(snapshot.Step * timeStep)}s of simulation time ({snapshot.Step} steps) simulated in {watch.ElapsedMilliseconds}ms with {numAgents} agents");
                System.Console.WriteLine(snapshot);
            }

            if (snapshot.AllComplete)
            {
                System.Console.WriteLine("Simulation finished");
                System.Console.WriteLine(snapshot);
                break;
            }
        }
        
        var snapshotsWriteRegistry = SerialiserRegistryFactory.Default<List<SimulationSnapshot>>();
        var snapshotsWriteSuccess = snapshotsWriteRegistry.Save(snapshotsOutPath, snapshots, 1);
        Debug.Assert(snapshotsWriteSuccess, "Failed to write simulation snapshots");
    }
}