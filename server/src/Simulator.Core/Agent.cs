using System.Diagnostics;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Utils;

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

    public int Radius = 225;

    public Vector2 GetVelocity(MovementConstraints constraints, double timeHorizon)
    {
        var preferredVelocity = GetPreferredVelocity();
        // If there are no conflicting agents we can move at preferred velocity (this already avoids walls)
        if (constraints.ConflictingAgents.Count == 0) return preferredVelocity;
        
        // ORCA logic
        var halfPlanes = new List<OrcaHelpers.HalfPlane>(constraints.ConflictingAgents.Count);
        foreach (var other in constraints.ConflictingAgents)
        {
            // For now set vOpt to 0
            var vOpt = new Vector2(0, 0);
            var vOptOther = new Vector2(0, 0);
            
            var vo = OrcaHelpers.GetVelocityObstacle(Position, Radius, other.Position, other.Radius, timeHorizon);

            // If they're already overlappaing construct a half plane directly between their positions
            if (vo.IsOverlapping)
            {
                var separation = Position - other.Position;
                if (separation.GetSquaredLength() == 0)
                    throw new UnreachableException("Agents should never be entirely overlapping");
                
                var normal = separation.GetNormalized();
                var penetrationSpeed = (Radius + other.Radius - separation.GetLength()) / timeStep;
                
                halfPlanes.Add(new OrcaHelpers.HalfPlane
                {
                    Point = vOpt + normal * penetrationSpeed,
                    Normal = normal,
                });
                continue;
            }
            
            var relOptVelocity = vOpt - vOptOther;
            
            var (closestPoint, outwardNormal) = OrcaHelpers.GetNearestPointOnBoundary(vo, relOptVelocity);
            var u = closestPoint - relOptVelocity;
            
            halfPlanes.Add(new OrcaHelpers.HalfPlane
            {
                Point = vOpt + u * 0.5f,
                Normal = outwardNormal
            });
        }
        
        // TODO: Add half planes for static objects
        // TODO: Implement linear program to solve half planes
        
        return preferredVelocity;
    }

    public Vector2 GetPreferredVelocity()
    {
        var nextTurningPoint = NavMesh.GetNextTurningPoint(Position, _portals);

        if (Position == Destination)
            return new Vector2(0, 0);

        var directionVector = (nextTurningPoint - Position).GetNormalized();
        // If the next turning point is the destination, speed is determined by the smaller of the max speed and the
        // speed required to reach the destination on this step
        var preferredSpeed = nextTurningPoint == Destination
            ? Math.Min(MaxSpeed, (Destination - Position).GetLength())
            : MaxSpeed;

        return directionVector * preferredSpeed * timeStep;
    }

    public AgentSnapshot UpdatePosition(Vector2 velocity)
    {
        // Snap to destination if agent is within 10 units (1cm)
        // Use squared distance to avoid square root
        if ((Position - Destination).GetSquaredLength() < 100)
            return new AgentSnapshot(Id, Destination, velocity.GetLength(), true);

        // Position delta if we weren't on a fixed grid resolution
        var positionDelta = velocity * timeStep;

        var positionDeltaInt = positionDelta.ToVector2Int();
        // Candidate positions deltas which we could actually move by
        List<Vector2Int> candidatePositionDeltas =
        [
            positionDeltaInt + new Vector2Int(1, 1),
            positionDeltaInt + new Vector2Int(1, 0),
            positionDeltaInt + new Vector2Int(0, 1),
            positionDeltaInt
        ];

        // Discard candidates which are outside the navigable area
        List<Vector2Int> validCandidatePositionDeltas = [];
        foreach (var candidate in candidatePositionDeltas)
        {
            if (navMesh.GetCurrentNode(candidate).Count == 0)
                validCandidatePositionDeltas.Add(candidate);
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

        return new AgentSnapshot(Id, Position, velocity.GetLength(), false);
    }
}