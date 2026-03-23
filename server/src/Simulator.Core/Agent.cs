using System.Diagnostics;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core;

public struct AgentSnapshot(int id, Vector2Int position, double speed, bool reachedDestination)
{
    public readonly int Id = id;
    public readonly Vector2Int Position = position;
    public readonly double Speed = speed;
    public readonly bool ReachedDestination = reachedDestination;
}

public class Agent(NavMesh navMesh, int id, int maxSpeed, Vector2Int startPos, Vector2Int target)
{
    private List<NavMesh.Portal> _portals = navMesh.GetPortals(startPos, target);
    
    public Vector2Int Position = startPos;
    public int Id = id;
    public int MaxSpeed = maxSpeed;
    public Vector2Int Destination = target;
    
    public AgentSnapshot Update(double timeStep)
    {
        var nextTurningPoint = NavMesh.GetNextTurningPoint(Position, _portals);
        
        // Return early if agent can reach destination this step
        if (nextTurningPoint == Destination && MaxSpeed * timeStep >= (Destination - Position).GetLength())
        {
            return new AgentSnapshot(Id, Destination, MaxSpeed, true);
        }
        
        var directionVector = (nextTurningPoint - Position).GetNormalized();
        var preferredVelocity = directionVector * MaxSpeed;
        
        Vector2Int nextPosition;
        if (preferredVelocity.GetMagnitude() * timeStep >= (nextTurningPoint - Position).GetLength())
        {
            nextPosition = nextTurningPoint;
        }
        else
        {
            var positionDelta = (preferredVelocity * timeStep).ToVector2Int();
            nextPosition = Position + positionDelta;
        }
        
        var crossedPortals = 0;
        // Keep crossing portals until encountering one we haven't crossed
        foreach (var portal in _portals)
        {
            if (portal.Left == portal.Right) continue;
            // Condition met if nextPoint is to the right of the left->right portal vector
            if (Sign(portal.Left, portal.Right, nextPosition) >= 0)
                crossedPortals++;
            else
                break;
        }

        Position = nextPosition;

        // If we would be removing all remaining portals, add the destination and return
        // Rarely used - only when we get instability around the final portal
        if (crossedPortals >= _portals.Count)
        {
            throw new UnreachableException();
        }
        
        _portals.RemoveRange(0, crossedPortals);
        
        return new AgentSnapshot(Id, Position, MaxSpeed, false);

        /*
        // Calculate true position delta
        var positionDelta = preferredVelocity * timeStep;

        // Position delta rounded down to next 0.1 interval
        var fixedPositionDelta = new Vector2Int((int)positionDelta.X, (int)positionDelta.Y);
        // List of candidate positions delta which we could actually move by
        List<Vector2Int> candidatePositionDeltas = [
            new (fixedPositionDelta.X + 1, fixedPositionDelta.Y + 1), 
            new (fixedPositionDelta.X + 1, fixedPositionDelta.Y), 
            new (fixedPositionDelta.X, fixedPositionDelta.Y + 1), 
            new (fixedPositionDelta.X, fixedPositionDelta.Y)
        ];
        
        // Discard candidates which are outside the navigable area
        List<Vector2Int> validCandidatePositionDeltas = [];
        foreach (var candidate in candidatePositionDeltas)
        {
            if (navMesh.GetCurrentNode(candidate).Count == 0)
                validCandidatePositionDeltas.Add(candidate);
        }
        
        Debug.Assert(validCandidatePositionDeltas.Count > 0, "Expected at least one valid candidate");

        // Select the candidate which crosses the most portals
        var maximalCandidate= new Vector2Int();
        var maximalCrossedPortals = -1;
        foreach (var candidate in validCandidatePositionDeltas)
        {
            var candidateCrossedPortals = NavMesh.GetNumCrossedPortals(candidate, _portals);

            if (candidateCrossedPortals > maximalCrossedPortals)
            {
                maximalCandidate = candidate;
                maximalCrossedPortals = candidateCrossedPortals;
            }
        }

        Position += maximalCandidate;
        _portals.RemoveRange(0, maximalCrossedPortals);
        
        return new AgentSnapshot(Id, Position, MaxSpeed, false);
        */
    }
    
    private static int Sign(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        return (b.X - a.X) * (c.Y - a.Y)
               - (b.Y - a.Y) * (c.X - a.X);
    }
}