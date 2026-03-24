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

    public UniformGrid<Agent> AgentGrid = new(1500);
    
    public SimulationEngine(SimulationConfig config)
    {
        TimeStep = config.TimeStep;
        // Initialise RNG with random seed if one isn't provided
        _rng = config.Seed == null ? new Random() : new Random(config.Seed.Value);
        
        Mesh = NavMeshGenerator.GenerateNavMesh(config.Geometry);
        LiveAgents = new List<Agent>(config.NumAgents);

        // Generate random start and end points for all numAgents
        var points = GenerateRandomPoints(2 * config.NumAgents);
        var startPoints = points.Take(config.NumAgents).ToArray();
        var endPoints = points.Skip(config.NumAgents).ToArray();
        
        // Create the agents at the start positions on step 0
        CreateAgents(startPoints, endPoints);
    }

    private void CreateAgents(Vector2Int[] startPoints, Vector2Int[] endPoints)
    {
        for (int i = 0; i < startPoints.Length; i++)
        {
            // Shape and scale parameters taken from Poulos et al.
            var speed = (int)Math.Round(StatisticalDistributions.SampleWeibull(_rng, 10.14f, 1.41f) * 1000);
            
            var cell = AgentGrid.ComputeCell(startPoints[i]);
            var agent = new Agent(TimeStep, Mesh, i, speed, startPoints[i], endPoints[i], cell);
            
            LiveAgents.Add(agent);
            AgentGrid.AddToCell(agent, cell);
        }
    }

    // Generate n random points on the navmesh (uniformly distributed)
    private Vector2Int[] GenerateRandomPoints(int n)
    {
        var points = new Vector2Int[n];
        
        for (int i = 0; i < n; i++)
        {
            points[i] = GeneratePoint();
        }

        return points;
    }

    private Vector2Int GeneratePoint()
    {
        Debug.Assert(Mesh != null, "Cannot generate point without a mesh");
        
        var rand = _rng.NextInt64(Mesh.CumulativeDoubleAreas.Last());

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
        var constraints = new MovementConstraints[LiveAgents.Count];
        
        // Populate ConflictingAgents and ConflictingWalls for all agents

        // Later parallelize this loop with Parallel.For()
        var agentSnapshots = new AgentSnapshot[LiveAgents.Count];
        for (int i = 0; i < LiveAgents.Count; i++)
        {
            var agent = LiveAgents[i];
            var velocity = agent.GetVelocity(constraints[i]);
            agentSnapshots[i] = agent.UpdatePosition(velocity);
        }
        
        var newLiveAgents = new List<Agent>(LiveAgents.Count);
        var snapshot = new SimulationSnapshot(LiveAgents.Count, Step);
        var allComplete = true;
        for (int i = 0; i < agentSnapshots.Length; i++)
        {
            var agent = LiveAgents[i];
            var agentSnapshot = agentSnapshots[i];
            snapshot.AddAgent(agentSnapshot.Id, agentSnapshot.Position, agentSnapshot.Speed);

            if (!agentSnapshot.ReachedDestination)
            {
                newLiveAgents.Add(agent);
                allComplete = false;
                
                var newCell = AgentGrid.ComputeCell(agentSnapshot.Position);
                // If the agent has remained in the same grid cell, continue
                if (agent.CurrentCell == newCell) continue;
                // Otherwise update the position in AgentGrid
                AgentGrid.RemoveFromCell(agent, agent.CurrentCell);
                AgentGrid.AddToCell(agent, newCell);
                agent.CurrentCell = newCell;
            }
            else
            {
                // If the agent has reached its destination remove it from AgentGrid
                AgentGrid.RemoveFromCell(agent, agent.CurrentCell);
            }
        }
        
        snapshot.AllComplete = allComplete;
        snapshot.Step = Step;

        Step++;
        LiveAgents.Clear();
        LiveAgents.AddRange(newLiveAgents);
        
        return snapshot;
    }
}