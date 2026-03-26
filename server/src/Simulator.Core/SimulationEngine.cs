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
    private double _orcaTimeHorizon = 3f;
    private int _maxAgentId = -1;
    
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
        var endPoints = points.Skip(config.NumAgents).ToArray();

        // Create the agents at the start positions on step 0
        CreateAgents(startPoints, endPoints);
    }

    private void CreateAgents(Vector2Int[] startPoints, Vector2Int[] endPoints)
    {
        for (int i = 0; i < startPoints.Length; i++)
        {
            var speed = GenerateRandomSpeed();
            var cell = AgentGrid.ComputeCell(startPoints[i]);
            var agent = new Agent(TimeStep, Mesh, i, speed, startPoints[i], endPoints[i], cell);

            LiveAgents.Add(agent);
            AgentGrid.AddToCell(agent, cell);
        }

        _maxAgentId = startPoints.Length - 1;
    }

    public void AddAgent(int? maxSpeed, Vector2Int? startPoint, Vector2Int? target)
    {
        startPoint ??= GeneratePoint();
        target ??= GeneratePoint();

        maxSpeed ??= GenerateRandomSpeed();
        maxSpeed = Math.Min(maxSpeed.Value, _globalMaxSpeed);

        if (Mesh.GetCurrentNode(startPoint.Value).Count == 0)
            throw new ArgumentException("Start point is outside mesh");
        if (Mesh.GetCurrentNode(target.Value).Count == 0)
            throw new ArgumentException("Target is outside mesh");


        var cell = AgentGrid.ComputeCell(startPoint.Value);
        var agent = new Agent(TimeStep, Mesh, _maxAgentId + 1, maxSpeed.Value, startPoint.Value, target.Value, cell);

        LiveAgents.Add(agent);
        AgentGrid.AddToCell(agent, cell);

        _maxAgentId++;
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
            var velocity = agent.GetVelocity(agentConstraints, _orcaTimeHorizon);
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

        var wallWindowRadius = agentA.MaxSpeed * _orcaTimeHorizon;
        var wallWindowRadiusSq = wallWindowRadius * wallWindowRadius;
        foreach (var cell in Mesh.Grid.GetRange(agentA.Position, wallWindowRadius))
        {
            foreach (var nodeIndex in cell)
            {
                var node = Mesh.Nodes[nodeIndex];
                foreach (var (a, b, i) in node.Triangle.GetEdges())
                {
                    // If the node has a neighbour over the edge it isn't a wall, so continue
                    // Since node.Neighbours is also populated with Triangle.GetEdges(), index i will always refer to
                    // the right edge
                    if (node.Neighbours[i] != -1) continue;

                    // Compare squared distances to avoid square root operation
                    var distanceSq = GetSquareDistancePointToSegment(agentA.Position, a, b);
                    if (distanceSq > wallWindowRadiusSq) continue;

                    // MovementConstraints stores walls as direction agnostic EdgeKey's, so it won't be added to the
                    // hashset if it already exists
                    aConstraints.AddConflictingWall(a, b);
                }
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

    // Get the minimum distance between point and line segment ab
    private static double GetSquareDistancePointToSegment(Vector2Int point, Vector2Int a, Vector2Int b)
    {
        var ab = b - a;
        var ap = point - a;

        var t = (double)ap.Dot(ab) / ab.GetSquaredLength();
        // Clamp to [0,1]
        t = Math.Max(0, Math.Min(1, t));

        // Closest point on segment
        var closestPoint = new Vector2(
            a.X + t * ab.X,
            a.Y + t * ab.Y
        );

        var differenceVector = point - closestPoint;
        return differenceVector.GetSquaredLength();
    }
}