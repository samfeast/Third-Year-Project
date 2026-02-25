using System.Diagnostics;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Utils;

namespace Simulator.Core;

public class SimulationEngine
{
    private readonly Random _rng;
    public NavMesh Mesh { get; private set; }
    public List<Agent> LiveAgents;
    public int Step = 1;
    public double TimeStep;
    
    public SimulationEngine(SimulationConfig config)
    {
        TimeStep = config.TimeStep;
        // Initialise RNG with random seed if one isn't provided
        _rng = config.Seed == null ? new Random() : new Random(config.Seed.Value);
        
        Mesh = NavMeshGenerator.GenerateNavMesh(config.Geometry, 50);
        LiveAgents = new List<Agent>(config.NumAgents);

        // Generate random start and end points for all numAgents
        var points = GenerateRandomPoints(2 * config.NumAgents);
        var startPoints = points.Take(config.NumAgents).ToArray();
        var endPoints = points.Skip(config.NumAgents).ToArray();
        
        // Create the agents at the start positions on step 0
        CreateAgents(0, startPoints);

        // Calculate paths to end positions
        for (int i = 0; i < config.NumAgents; i++)
        {
            var target = config.Target ?? endPoints[i];
            LiveAgents[i].ComputePath(Mesh, target);
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