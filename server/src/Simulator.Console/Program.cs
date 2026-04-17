using System.Diagnostics;
using Simulator.Core;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;
using Simulator.Core.Utils;
using Simulator.IO;

namespace Simulator.Console;

public class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine(
            "Welcome to the evacuation simulator CLI interface! Select an option by typing the desired number.");
        var complete = false;
        while (!complete)
        {
            System.Console.WriteLine("\t1. Write triangulation to disk");
            System.Console.WriteLine("\t2. Run single agent simulation");
            System.Console.WriteLine("\t3. Exit");
            var option = System.Console.ReadLine();

            switch (option)
            {
                case "1":
                    HandleOption1();
                    break;
                case "2":
                    HandleOption2();
                    break;
                case "3":
                    complete = true;
                    break;
            }
        }
    }

    private static void HandleOption1()
    {
        System.Console.Write("Path to geometry input: ");
        var inPath = System.Console.ReadLine();
        if (string.IsNullOrEmpty(inPath))
        {
            System.Console.WriteLine("Must provide a valid input path");
            return;
        }

        System.Console.Write("Path to triangulation output: ");
        var outPath = System.Console.ReadLine();
        if (string.IsNullOrEmpty(outPath))
        {
            System.Console.WriteLine("Must provide a valid output path");
            return;
        }

        System.Console.Write("Geometry exclusion radius (default 225mm): ");
        var exclusionRadStr = System.Console.ReadLine();
        int exclusionRad;
        if (string.IsNullOrEmpty(exclusionRadStr))
            exclusionRad = 225;
        else
            exclusionRad = int.Parse(exclusionRadStr);

        var inputGeometry = inPath.Deserialise<InputGeometry>();
        var mesh = NavMeshGenerator.GenerateNavMesh(inputGeometry, 5000, exclusionRad);
        mesh.Save(outPath, 1);
        System.Console.WriteLine("Triangulation saved to disk");
    }

    private static void HandleOption2()
    {
        System.Console.Write("Path to geometry input: ");
        var inPath = System.Console.ReadLine();
        if (string.IsNullOrEmpty(inPath))
        {
            System.Console.WriteLine("Must provide a valid input path");
            return;
        }

        System.Console.Write("Spawn seed (default 100): ");
        var spawnSeedStr = System.Console.ReadLine();
        int spawnSeed;
        if (string.IsNullOrEmpty(spawnSeedStr))
            spawnSeed = 100;
        else
            spawnSeed = int.Parse(spawnSeedStr);

        var inputGeometry = inPath.Deserialise<InputGeometry>();
        const float timeStep = 0.1f;
        var config = new SimulationConfig(inputGeometry, timeStep, 1, spawnSeed);
        var simulator = new SimulationEngine(config);

        while (true)
        {
            var snapshot = simulator.StepSimulation();

            if (snapshot.Step % 60 == 0)
            {
                System.Console.WriteLine(snapshot);
            }

            if (!snapshot.AllComplete) continue;

            System.Console.WriteLine("Simulation finished");
            System.Console.WriteLine(snapshot);
            break;
        }
    }
}