using System.Diagnostics;
using Simulator.Core;
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

        // Load file into InputGeometry object
        var inputGeometry = inPath.Deserialise<InputGeometry>();
        
        System.Console.WriteLine("Geometry loaded");

        int numAgents = 50;
        double timeStep = 0.05f;

        var watch = Stopwatch.StartNew();
        var config = new SimulationConfig(inputGeometry, timeStep, numAgents, 103);
        var simulator = new SimulationEngine(config);
        watch.Stop();
        
        
        System.Console.WriteLine($"Geometry divided into {simulator.Mesh.Nodes.Count} triangles in {watch.ElapsedMilliseconds}ms");
        
        // Save the navmesh to meshOutPath
        simulator.Mesh.Save(meshOutPath, 1);
        
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

        snapshots.Save(snapshotsOutPath, 1);
    }
}