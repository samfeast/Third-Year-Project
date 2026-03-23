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
        
        // Position delta if we weren't on a fixed grid resolution
        var positionDelta = preferredVelocity * timeStep;
        
        var positionDeltaInt = positionDelta.ToVector2Int();
        // Candidate positions deltas which we could actually move by
        List<Vector2Int> candidatePositionDeltas = [
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
        
        Debug.Assert(validCandidatePositionDeltas.Count > 0, "Expected at least one valid candidate");
        
        // Select the candidate which crosses the most portals
        var maximalCandidate = new Vector2Int();
        var maximalCrossedPortals = -1;
        var foundCandidate = false;
        foreach (var candidate in validCandidatePositionDeltas)
        {
            var candidateCrossedPortals = NavMesh.GetNumCrossedPortals(Position + candidate, _portals);

            if (candidateCrossedPortals > maximalCrossedPortals)
            {
                foundCandidate = true;
                maximalCandidate = candidate;
                maximalCrossedPortals = candidateCrossedPortals;
            }
        }
        
        Debug.Assert(foundCandidate, "Expect to have found a viable candidate");
        Debug.Assert(maximalCrossedPortals < _portals.Count, "Didn't expect to try removing all portals");
        
        // Update Position and remove portals crossed on this step
        Position += maximalCandidate;
        _portals.RemoveRange(0, maximalCrossedPortals);
        
        return new AgentSnapshot(Id, Position, MaxSpeed, false);
    }
}