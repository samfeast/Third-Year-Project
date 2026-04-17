using System.Diagnostics;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Utils;
using RVO2Lib = RVO2;

namespace Simulator.Core;

public struct AgentSnapshot(int id, Vector2Int position, double speed, bool reachedDestination)
{
    public readonly int Id = id;
    public readonly Vector2Int Position = position;
    public readonly double Speed = speed;
    public readonly bool ReachedDestination = reachedDestination;
}

public class Agent(
    double timeStep,
    NavMesh navMesh,
    int id,
    int maxSpeed,
    Vector2Int startPos,
    Vector2Int target,
    (int, int) startCell)
{
    private List<NavMesh.Portal> _portals = navMesh.GetPortals(startPos, target);

    public int Id = id;
    public (int, int) CurrentCell = startCell;

    public int MaxSpeed = maxSpeed;
    public Vector2Int Destination = target;

    public Vector2Int Position = startPos;
    public Vector2 Velocity = new(0, 0);

    private Vector2Int _lastGoodPosition = startPos;

    private int _fails;

    public int Radius = 225;
    private bool UseOrca = true;

    private const double EPSILON = 0.00001;

    public (Vector2 preferredVelocity, Vector2 actualVelocity) GetVelocity(MovementConstraints constraints,
        double timeHorizon, bool debugLogging = false)
    {
        var (preferredVelocity, isFinal) = GetPreferredVelocity();
        // If there are no conflicting agents we can move at preferred velocity (this already avoids walls)
        if (constraints.ConflictingAgents.Count == 0 || !UseOrca || isFinal)
            return (preferredVelocity, preferredVelocity);

        if (debugLogging)
            Console.WriteLine($"[{Id}] Current Position: {Position}");

        // ORCA logic
        var halfPlanes = new List<RVO2Lib.Line>(constraints.ConflictingAgents.Count);

        // Add half planes for conflicting agents
        foreach (var other in constraints.ConflictingAgents)
            halfPlanes.Add(GetAgentHalfPlane(other, timeHorizon, debugLogging));

        RVO2Lib.Vector2 rvo2NewVelocity;
        RVO2Lib.Vector2 rvo2PreferredVelocity =
            new RVO2Lib.Vector2((float)preferredVelocity.X, (float)preferredVelocity.Y);

        int lineFail =
            RVO2Lib.Solvers.LinearProgram2(halfPlanes, MaxSpeed, rvo2PreferredVelocity, false, out rvo2NewVelocity);

        // Fallback if LinearProgram2 cannot be satisfied
        if (lineFail < halfPlanes.Count)
        {
            RVO2Lib.Solvers.LinearProgram3(halfPlanes, 0, lineFail, MaxSpeed, ref rvo2NewVelocity);
        }

        var actualVelocity = new Vector2(rvo2NewVelocity.X, rvo2NewVelocity.Y);
        if (debugLogging)
        {
            Console.WriteLine($"[{Id}] Preferred V: {preferredVelocity}");
            Console.WriteLine($"[{Id}] ORCA V: {actualVelocity}");
        }

        return (preferredVelocity, actualVelocity);
    }

    public (Vector2 velocity, bool isFinal) GetPreferredVelocity()
    {
        if (Position == Destination)
            return (new Vector2(0, 0), true);

        var nextTurningPoint = NavMesh.GetNextTurningPoint(Position, _portals);

        var directionVector = (nextTurningPoint - Position).GetNormalized();
        // If the next turning point is the destination, speed is determined by the smaller of the max speed and the
        // speed required to reach the destination on this step
        var preferredSpeed = nextTurningPoint == Destination
            ? Math.Min(MaxSpeed, (Destination - Position).GetLength() / timeStep)
            : MaxSpeed;

        var isFinal = preferredSpeed < MaxSpeed - EPSILON;

        return (directionVector * preferredSpeed, isFinal);
    }

    public AgentSnapshot UpdatePosition(Vector2 velocity, Vector2 fallbackVelocity)
    {
        // Snap to destination if agent is within 10 units (1cm)
        // Use squared distance to avoid square root
        if ((Position - Destination).GetSquaredLength() < 100)
            return new AgentSnapshot(Id, Destination, velocity.GetLength(), true);

        // Position delta if we weren't on a fixed grid resolution
        var positionDelta = velocity * timeStep;

        var positionDeltaInt = positionDelta.ToVector2Int();
        // Candidate positions deltas which we could actually move by
        
        var validCandidatePositionDeltas = GetValidCandidatePositionDeltas(positionDeltaInt);

        // If the proposed new positions are invalid, it means ORCA is trying to push the agent outside the navigable area
        // In this case, let the agent move at its preferred velocity (and ignore ORCA constraints)
        if (validCandidatePositionDeltas.Count == 0)
        {
            if (velocity != fallbackVelocity)
                return UpdatePosition(fallbackVelocity, fallbackVelocity);
            
            // In rare cases the preferred velocity may also be invalid.
            // This only happens when the agent has crossed a portal near a corner, and is subsequently pushed back over it again. This can cause the velocity vector to point through an obstacle.
            // When this happens, teleport the agent to _lastGoodPosition, which is the last position the agent was at where it successfully crossed a boundary.
            // While imperfect, visually the result appears realistic.
            if (velocity == fallbackVelocity && _fails < 20)
            {
                _fails++;
                Position = _lastGoodPosition;
                Velocity = new Vector2(0, 0);
                return new AgentSnapshot(Id, Position, 0, false);
            }
            
            // Last resort fallback - if the agent has been stuck for more than 2 seconds (20 steps), move it to the next portal
            var toPortalLeft = _portals[0].Left - Position;
            var toPortalRight = _portals[0].Right - Position;
            validCandidatePositionDeltas.Add(toPortalLeft.GetSquaredLength() < toPortalRight.GetSquaredLength()
                ? toPortalLeft
                : toPortalRight);
        }

        // Select the candidate which crosses the most portals
        var maximalCandidate = default(Vector2Int);
        var maximalCrossedPortals = -1;
        foreach (var candidate in validCandidatePositionDeltas)
        {
            var crossed = NavMesh.GetNumCrossedPortals(Position + candidate, _portals);
            if (crossed <= maximalCrossedPortals) continue;

            maximalCandidate = candidate;
            maximalCrossedPortals = crossed;
        }

        // Update Position and Velocity, and remove portals crossed on this step
        Position += maximalCandidate;
        Velocity = velocity;

        _portals.RemoveRange(0, maximalCrossedPortals);

        // If we crossed a portal, update the last good position
        if (maximalCrossedPortals > 0)
            _lastGoodPosition = Position;

        _fails = 0;

        return new AgentSnapshot(Id, Position, velocity.GetLength(), false);
    }

    private List<Vector2Int> GetValidCandidatePositionDeltas(Vector2Int positionDelta)
    {
        List<Vector2Int> candidatePositionDeltas =
        [
            positionDelta + new Vector2Int(1, 1),
            positionDelta + new Vector2Int(1, 0),
            positionDelta + new Vector2Int(0, 1),
            positionDelta
        ];

        // Discard candidates which are outside the navigable area
        List<Vector2Int> validCandidatePositionDeltas = [];
        foreach (var candidate in candidatePositionDeltas)
        {
            if (navMesh.GetCurrentNode(Position + candidate).Count > 0)
                validCandidatePositionDeltas.Add(candidate);
        }
        
        return validCandidatePositionDeltas;
    }

    private RVO2Lib.Line GetAgentHalfPlane(MovementConstraints.ConflictingAgent other, double timeHorizon,
        bool debugLogging = false)
    {
        var vOpt = Velocity;
        var vOptOther = other.Velocity;

        var vo = OrcaHelpers.GetVelocityObstacle(Position, Radius, other.Position, other.Radius, timeHorizon);

        Vector2 point;
        Vector2 normal;

        RVO2Lib.Vector2 rvo2Point;
        RVO2Lib.Vector2 rvo2Direction;

        // If they're already overlapping construct a half plane directly between their positions
        if (vo.IsOverlapping)
        {
            var separation = Position - other.Position;
            if (separation.GetSquaredLength() == 0)
                throw new UnreachableException("Agents should never be entirely overlapping");

            var penetrationSpeed = (Radius + other.Radius - separation.GetLength()) / timeStep;

            normal = separation.GetNormalized();
            point = vOpt + normal * penetrationSpeed;

            rvo2Direction = new RVO2Lib.Vector2((float)normal.Y, (float)-normal.X);
            rvo2Point = new RVO2Lib.Vector2((float)point.X, (float)point.Y);

            if (debugLogging)
                Console.WriteLine($"[{Id}] Constrained by agent at {other.Position} -> {normal.Round(3).X}x + {normal.Round(3).Y}y < {Math.Round(point.Dot(normal),3)}");

            return new RVO2Lib.Line
            {
                Point = rvo2Point,
                Direction = rvo2Direction
            };
        }

        var relOptVelocity = vOpt - vOptOther;

        var (closestPoint, outwardNormal) = OrcaHelpers.GetNearestPointOnBoundary(vo, relOptVelocity);
        var u = closestPoint - relOptVelocity;

        normal = outwardNormal;
        point = vOpt + u * 0.5f;

        rvo2Direction = new RVO2Lib.Vector2((float)normal.Y, (float)-normal.X);
        rvo2Point = new RVO2Lib.Vector2((float)point.X, (float)point.Y);
        
        if (debugLogging)
            Console.WriteLine($"[{Id}] Constrained by agent at {other.Position} -> {normal.Round(3).X}x + {normal.Round(3).Y}y < {Math.Round(point.Dot(normal),3)}");

        return new RVO2Lib.Line
        {
            Point = rvo2Point,
            Direction = rvo2Direction
        };
    }
}