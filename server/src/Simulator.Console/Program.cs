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
        System.Console.WriteLine("Welcome to the evacuation simulator CLI interface! Select an option by typing the desired number.");
        var complete = false;
        while (!complete)
        {
            System.Console.WriteLine("\t1. Write triangulation to disk");
            System.Console.WriteLine("\t2. Run single agent simulation");
            System.Console.WriteLine("\t3. Compute ORCA half planes");
            System.Console.WriteLine("\t4. Exit");
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
                    HandleOption3();
                    break;
                case "4":
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

    private static void HandleOption3()
    {
        System.Console.Write("Path to geometry input: ");
        var inPath = System.Console.ReadLine();
        if (string.IsNullOrEmpty(inPath))
        {
            System.Console.WriteLine("Must provide a valid input path");
            return;
        }

        var inputGeometry = inPath.Deserialise<InputGeometry>();
        const float timeStep = 0.1f;

        var config = new SimulationConfig(inputGeometry, timeStep, 0);
        var simulator = new SimulationEngine(config);

        simulator.AddAgent(1000, new Vector2Int(2000, 1000), new Vector2Int(4000, 1000));
        simulator.AddAgent(1000, new Vector2Int(4000, 1000), new Vector2Int(2000, 1000));
        simulator.AddAgent(1000, new Vector2Int(2000, 3000), new Vector2Int(2000, 1000));
        simulator.AddAgent(1000, new Vector2Int(2500, 1000), new Vector2Int(2000, 1000));

        var constraints = new Dictionary<int, MovementConstraints>();
        foreach (var agent in simulator.LiveAgents)
            simulator.GenerateConstraints(constraints, agent);

        foreach (var agent in simulator.LiveAgents)
        {
            System.Console.WriteLine($"\nAgent {agent.Id} at {agent.Position}\n");
            var agentConstraints = constraints[agent.Id];
            System.Console.WriteLine(agentConstraints);
            var preferredVelocity = agent.GetPreferredVelocity();
            System.Console.WriteLine($"Preferred velocity: {preferredVelocity}\n");

            var halfPlanes = new List<OrcaHelpers.HalfPlane>(agentConstraints.ConflictingAgents.Count);
            foreach (var other in agentConstraints.ConflictingAgents)
            {
                System.Console.WriteLine($"Get half plane with agent at {other.Position}");
                var vOpt = new Vector2(0,0);
                var vOptOther = new Vector2(0,0);

                Vector2 halfPlanePoint;
                var vo = OrcaHelpers.GetVelocityObstacle(agent.Position, agent.Radius, other.Position, other.Radius, 3f);
                if (vo.IsOverlapping)
                {
                    System.Console.WriteLine($"\tOverlapping");
                    var separation = agent.Position - other.Position;
                    if (separation.GetSquaredLength() == 0)
                        throw new UnreachableException("Agents should never be entirely overlapping");
                
                    var normal = separation.GetNormalized();
                    var penetrationSpeed = (agent.Radius + other.Radius - separation.GetLength()) / timeStep;

                    halfPlanePoint = vOpt + normal * penetrationSpeed;
                
                    halfPlanes.Add(new OrcaHelpers.HalfPlane
                    {
                        Point = halfPlanePoint,
                        Normal = normal,
                    });
                    System.Console.WriteLine($"Adding half plane with point {halfPlanePoint} and normal {normal}");
                    
                    var normalRounded = normal.Round(3);
                    System.Console.WriteLine($"\tDesmos str: {normalRounded.X}x + {normalRounded.Y}y < {Math.Round(halfPlanePoint.Dot(normal),3)}");
                    continue;
                }

                System.Console.WriteLine($"\tTruncation center: {vo.TruncationCentre}");
                System.Console.WriteLine($"\tTruncation radius: {vo.TruncationRadius}");
                System.Console.WriteLine($"\tLeft intersection: {vo.LeftLegIntersection}");
                System.Console.WriteLine($"\tRight intersection: {vo.RightLegIntersection}");
                
                var relOptVelocity = vOpt - vOptOther;
            
                var (closestPoint, outwardNormal) = OrcaHelpers.GetNearestPointOnBoundary(vo, relOptVelocity);
                System.Console.WriteLine($"\t{closestPoint.Round(3)} is closest to {relOptVelocity.Round(3)}");
                var u = closestPoint - relOptVelocity;

                halfPlanePoint = vOpt + u * 0.5f;
                
                halfPlanes.Add(new OrcaHelpers.HalfPlane
                {
                    Point = halfPlanePoint,
                    Normal = outwardNormal
                });
                
                System.Console.WriteLine($"Adding half plane with point {halfPlanePoint} and normal {outwardNormal}");
                var outwardNormalRounded = outwardNormal.Round(3);
                System.Console.WriteLine($"\tDesmos str: {outwardNormalRounded.X}x + {outwardNormalRounded.Y}y < {Math.Round(halfPlanePoint.Dot(outwardNormal),3)}");
            }

            var vActual = OrcaHelpers.LinearProgram2(halfPlanes, preferredVelocity);
            System.Console.WriteLine($"V_Actual: {vActual}");
            System.Console.WriteLine($"End of agent {agent.Id}\n");
        }
    }
}