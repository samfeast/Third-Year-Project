using System.Diagnostics;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core;

public struct AgentSnapshot(int id, Vector2 position, double speed, bool reachedDestination)
{
    public readonly int Id = id;
    public readonly Vector2 Position = position;
    public readonly double Speed = speed;
    public readonly bool ReachedDestination = reachedDestination;
}

public class Agent(NavMesh navMesh, int id, double maxSpeed, Vector2Fraction startPos, Vector2Fraction target)
{

    private List<NavMesh.Portal> _portals = navMesh.GetPortals(startPos.Evaluate(), target.Evaluate());
    
    public Vector2Fraction Position = startPos;
    public int Id = id;
    public double MaxSpeed = maxSpeed;
    public Vector2Fraction Destination = target;
    
    public AgentSnapshot Update(double timeStep)
    {
        var nextTurningPoint = navMesh.GetNextTurningPoint(Position, _portals);
        
        var directionVector = (nextTurningPoint - Position).Evaluate().GetNormalized();
        var preferredVelocity = directionVector * MaxSpeed;

        // Return early if agent can reach destination this step
        if (nextTurningPoint == Destination && MaxSpeed * timeStep >= (Destination - Position).Evaluate().GetMagnitude())
        {
            return new AgentSnapshot(Id, Destination.Evaluate(), MaxSpeed, true);
        }
        
        // If we can reach the next turning point this step, go directly to that point, otherwise go in that direction
        Vector2Fraction nextPosition;
        if (preferredVelocity.GetMagnitude() * timeStep >= (nextTurningPoint - Position).Evaluate().GetMagnitude())
        {
            nextPosition = nextTurningPoint;
        }
        else
        {
            var positionDelta = (preferredVelocity * timeStep).Round(1).ToVector2Fraction();
            nextPosition = Position + positionDelta;
        }
        
        var crossedPortals = 0;
        // Keep crossing portals until encountering one we haven't crossed
        foreach (var portal in _portals)
        {
            if (portal.Left == portal.Right) continue;
            // Condition met if nextPoint is to the right of the left->right portal vector
            if (NavMesh.Sign(portal.Left, portal.Right, nextPosition) >= LongFraction.Zero)
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
        
        return new AgentSnapshot(Id, Position.Evaluate(), MaxSpeed, false);
    }
}