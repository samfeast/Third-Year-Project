using System.Diagnostics;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Utils;

namespace Simulator.Core;

public class SimulationEngine
{
    private readonly Random _rng;
    public NavMesh Mesh { get; private set; }

    public int Step = 1;
    public double TimeStep;

    private int _globalMaxSpeed = 1750; // Would require a random sample of 0.9999, so only clamp in 1/10000 cases
    private double _orcaTimeHorizon = 2f;
    private int _maxAgentId = 0;
    
    public ResultsCollector Results = new();
    
    public List<Agent> LiveAgents;
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
        
        if (config.Geometry.Exits.Count == 0)
        {
            var endPoints = points.Skip(config.NumAgents).ToArray();
            CreateAgents(startPoints, endPoints);
        } else if (config.Geometry.Exits.Count == 1)
        {
            CreateAgents(startPoints, config.Geometry.Exits[0]);
        }
        else
        {
            for (int i = 0; i < config.NumAgents; i++)
            {
                var bestExit = GetBestExit(startPoints[i], config.Geometry.Exits);
                CreateAgent(startPoints[i], bestExit);
            }
        }
    }

    private void CreateAgents(Vector2Int[] startPoints, Vector2Int[] endPoints)
    {
        for (int i = 0; i < startPoints.Length; i++)
            CreateAgent(startPoints[i], endPoints[i]);
    }

    private void CreateAgents(Vector2Int[] startPoints, Vector2Int endPoint)
    {
        foreach (var startPoint in startPoints)
            CreateAgent(startPoint, endPoint);
    }

    private void CreateAgent(Vector2Int startPoint, Vector2Int endPoint)
    {
        var speed = GenerateRandomSpeed();
        var cell = AgentGrid.ComputeCell(startPoint);
        var agent = new Agent(TimeStep, Mesh, _maxAgentId + 1, speed, startPoint, endPoint, cell);

        LiveAgents.Add(agent);
        AgentGrid.AddToCell(agent, cell);

        _maxAgentId++;
    }

    private Vector2Int GetBestExit(Vector2Int startPoint, List<Vector2Int> exits)
    {
        var bestExit = exits[0];
        var distanceToBestExit = double.PositiveInfinity;

        foreach (var exit in exits)
        {
            var portals = Mesh.GetPortals(startPoint, exit);
            var path = Mesh.GetFullFunnelPath(startPoint, portals);
                    
            double distance = 0;

            for (int j = 1; j < path.Count; j++)
            {
                var a = path[j - 1];
                var b = path[j];

                distance += (a - b).GetLength();
            }

            if (!(distance < distanceToBestExit)) continue;
            
            distanceToBestExit = distance;
            bestExit = exit;
        }
        
        return bestExit;
    }

    private int GenerateRandomSpeed()
    {
        // Shape and scale parameters taken from Poulos et al.
        var sampledSpeed = StatisticalDistributions.SampleWeibull(_rng, 10.14, 1.41) * 1000;
        // Clamp to _globalMaxSpeed
        return Math.Min((int)Math.Round(sampledSpeed), _globalMaxSpeed);
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
        var constraints = new Dictionary<int, MovementConstraints>();
        foreach (var agent in LiveAgents)
            GenerateConstraints(constraints, agent);

        // Later parallelize this loop with Parallel.For()
        var agentSnapshots = new AgentSnapshot[LiveAgents.Count];
        for (int i = 0; i < LiveAgents.Count; i++)
        {
            var agent = LiveAgents[i];
            var agentConstraints = constraints[agent.Id];
            
            var debugLogging = agent.Id == -1;

            var (preferredVelocity, actualVelocity) = agent.GetVelocity(agentConstraints, _orcaTimeHorizon, debugLogging);
            agentSnapshots[i] = agent.UpdatePosition(actualVelocity, preferredVelocity);
            
            var currentNodes = Mesh.GetCurrentNode(agent.Position);
            foreach (var nodeIndex in currentNodes)
                Mesh.HeatMap[nodeIndex] += 1;
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
                // If the agent has reached its destination remove it from AgentGrid and collect results
                AgentGrid.RemoveFromCell(agent, agent.CurrentCell);
                Results.Evacuationtimes.Add(Step);
            }
        }

        // Force simulation to terminate after 1 hour
        snapshot.AllComplete = allComplete || Step > 36000;
        snapshot.Step = Step;

        Step++;
        LiveAgents.Clear();
        LiveAgents.AddRange(newLiveAgents);
        
        if (allComplete)
            GenerateHeatmap();

        return snapshot;
    }

    public void GenerateConstraints(Dictionary<int, MovementConstraints> constraints, Agent agentA)
    {
        var aConstraints = GetOrCreateConstraints(constraints, agentA.Id);

        var agentWindowRadius = (agentA.MaxSpeed + _globalMaxSpeed) * _orcaTimeHorizon;
        foreach (var cell in AgentGrid.GetRange(agentA.Position, agentWindowRadius))
        {
            foreach (var agentB in cell)
            {
                // Avoid computing both A->B and B->A
                if (agentB.Id <= agentA.Id) continue;

                var horizonDistance = (agentA.MaxSpeed + agentB.MaxSpeed) * _orcaTimeHorizon;

                var horizonDistanceSq = horizonDistance * horizonDistance;
                var distanceSq = (agentA.Position - agentB.Position).GetSquaredLength();

                // Compare squared distances to avoid square root
                if (distanceSq > horizonDistanceSq) continue;

                var bConstraints = GetOrCreateConstraints(constraints, agentB.Id);

                aConstraints.AddConflictingAgent(agentB.Position, agentB.Velocity, agentB.Radius);
                bConstraints.AddConflictingAgent(agentA.Position, agentA.Velocity, agentA.Radius);
            }
        }
    }

    private static MovementConstraints GetOrCreateConstraints(Dictionary<int, MovementConstraints> constraints,
        int agentId)
    {
        if (constraints.TryGetValue(agentId, out var agentConstraints)) return agentConstraints;

        agentConstraints = new MovementConstraints();
        constraints[agentId] = agentConstraints;
        return agentConstraints;
    }

    private void GenerateHeatmap()
    {
        double maxDensity = -1;
        for (int i = 0; i < Mesh.Nodes.Count; i++)
        {
            var density = (double)Mesh.HeatMap[i] / Mesh.Nodes[i].DoubleArea;
            if (density > maxDensity)
                maxDensity = density;
        }

        for (int i = 0; i < Mesh.Nodes.Count; i++)
        {
            var density = (double)Mesh.HeatMap[i] / Mesh.Nodes[i].DoubleArea / maxDensity;
            Results.HeatMap.Add(new ResultsCollector.HeatMapCell(Mesh.Nodes[i].Triangle, density));
        }
    }
}