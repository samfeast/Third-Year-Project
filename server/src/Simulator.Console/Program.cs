using Simulator.Core;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Utils;
using Simulator.IO;

namespace Simulator.Console;

public class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("Welcome to the evacuation simulator CLI interface! Select an option by typing the desired number.");
        var complete = false;
        while (!complete)
        {
            System.Console.WriteLine("\t1. Write triangulation to disk");
            System.Console.WriteLine("\t2. Run single agent simulation");
            System.Console.WriteLine("\t3. Exit");
            var option = System.Console.ReadLine();
            
            string? inPath;
            string? outPath;
            InputGeometry inputGeometry;
            
            switch (option)
            {
                case "1":
                    System.Console.Write("Path to geometry input: ");
                    inPath = System.Console.ReadLine();
                    if (string.IsNullOrEmpty(inPath))
                    {
                        System.Console.WriteLine("Must provide a valid input path");
                        break;
                    }

                    System.Console.Write("Path to triangulation output: ");
                    outPath = System.Console.ReadLine();
                    if (string.IsNullOrEmpty(outPath))
                    {
                        System.Console.WriteLine("Must provide a valid output path");
                        break;
                    }
                    
                    System.Console.Write("Geometry exclusion radius (default 225mm): ");
                    var exclusionRadStr = System.Console.ReadLine();
                    int exclusionRad;
                    if (string.IsNullOrEmpty(exclusionRadStr))
                        exclusionRad = 225;
                    else
                        exclusionRad = int.Parse(exclusionRadStr);
                    
                    inputGeometry = inPath.Deserialise<InputGeometry>();
                    var mesh = NavMeshGenerator.GenerateNavMesh(inputGeometry, 50, exclusionRad);
                    mesh.Save(outPath, 1);
                    System.Console.WriteLine("Triangulation saved to disk");
                    break;
                case "2":
                    System.Console.Write("Path to geometry input: ");
                    inPath = System.Console.ReadLine();
                    if (string.IsNullOrEmpty(inPath))
                    {
                        System.Console.WriteLine("Must provide a valid input path");
                        break;
                    }
                    
                    System.Console.Write("Spawn seed (default 100): ");
                    var spawnSeedStr = System.Console.ReadLine();
                    int spawnSeed;
                    if (string.IsNullOrEmpty(spawnSeedStr))
                        spawnSeed = 100;
                    else
                        spawnSeed = int.Parse(spawnSeedStr);
                    
                    inputGeometry = inPath.Deserialise<InputGeometry>();
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

                        if (snapshot.AllComplete)
                        {
                            System.Console.WriteLine("Simulation finished");
                            System.Console.WriteLine(snapshot);
                            break;
                        }
                    }
                    break;
                case "3":
                    complete = true;
                    break;
            }
        }
    }
}