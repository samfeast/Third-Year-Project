using System.Diagnostics;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;
using Simulator.Core.Utils;

namespace Simulator.Core;

public class SimulationEngine
{
    // Initialise RNG with random seed if one isn't provided
    private readonly Random _rng;
    public NavMesh Mesh { get; private set; }
    public List<Agent> LiveAgents;
    public int Step = 1;
    public double TimeStep;
    
    public SimulationEngine(InputGeometry inputGeometry, double timeStep, int numAgents, int? seed = null)
    {
        TimeStep = timeStep;
        _rng = seed == null ? new Random() : new Random(seed.Value);
        
        Mesh = NavMeshGenerator.GenerateNavMesh(inputGeometry, 50);
        LiveAgents = new List<Agent>(numAgents);

        // Generate random start and end points for all numAgents
        var points = GenerateRandomPoints(2 * numAgents);
        var startPoints = points.Take(numAgents).ToArray();
        var endPoints = points.Skip(numAgents).ToArray();
        
        // Create the agents at the start positions on step 0
        CreateAgents(0, startPoints);

        // Calculate paths to end positions
        for (int i = 0; i < numAgents; i++)
        {
            //LiveAgents[i].ComputePath(Mesh, endPoints[i]);
            LiveAgents[i].ComputePath(Mesh, new Vector2(250, 250));
        }
    }

    private void CreateAgents(int startStep, Vector2[] startPoints)
    {
        for (int i = 0; i < startPoints.Length; i++)
        {
            // Shape and scale parameters taken from Poulos et al.
            var speed = StatisticalDistributions.SampleWeibull(_rng, 10.14f, 1.41f) * 100f;
            LiveAgents.Add(new Agent(i, startStep, speed, startPoints[i]));
        }
    }

    // Generate n random points on the navmesh (uniformly distributed)
    private Vector2[] GenerateRandomPoints(int n)
    {
        var points = new Vector2[n];
        
        for (int i = 0; i < n; i++)
        {
            points[i] = GeneratePoint();
        }

        return points;
    }

    private Vector2 GeneratePoint()
    {
        Debug.Assert(Mesh != null, "Cannot generate point without a mesh");
        
        int rand = _rng.Next(Mesh.CumulativeDoubleAreas.Last());

        for (int i = 0; i < Mesh.CumulativeDoubleAreas.Count; i++)
        {
            if (rand < Mesh.CumulativeDoubleAreas[i])
            {
                return Mesh.Nodes[i].Triangle.GenerateRandomPoint(_rng);
            }
        }

        throw new UnreachableException();
    }

    public SimulationSnapshot StepSimulation()
    {
        var snapshot = new SimulationSnapshot(LiveAgents.Count, Step);
        
        var allComplete = true;
        var completeThisStep = new List<Agent>();
        foreach (var agent in LiveAgents)
        {
            var agentSnapshot = agent.Update(TimeStep);
            
            snapshot.AddAgent(agentSnapshot.Id, agentSnapshot.Position, agentSnapshot.Speed);

            if (agentSnapshot.ReachedDestination)
                completeThisStep.Add(agent);
            else
                allComplete = false;
        }

        foreach (var completeAgent in completeThisStep)
            LiveAgents.Remove(completeAgent);
        
        snapshot.AllComplete = allComplete;
        snapshot.Step = Step;

        Step++;
        
        return snapshot;
    }
}