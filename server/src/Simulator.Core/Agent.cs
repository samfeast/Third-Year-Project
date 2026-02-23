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

public class Agent(int id, int startStep, double _maxSpeed, Vector2 startPos)
{
    private List<Vector2Fraction> _waypoints = [];
    private int _nextWaypointIndex = 0;
    
    public Vector2 Position = startPos;
    public Vector2 NextWaypoint => _waypoints[_nextWaypointIndex].Evaluate();
    public int Id = id;
    public int StartStep = startStep;

    // Generate waypoints from current position to destination across provided navMesh
    public void ComputePath(NavMesh navMesh, Vector2 destination)
    {
        _waypoints = navMesh.Navigate(Position, destination);
        _nextWaypointIndex = 0;
    }

    // Return true if journey complete
    public AgentSnapshot Update(double timeStep)
    {
        var distToTravel = _maxSpeed * timeStep;
        while (distToTravel > -1e-6)
        {
            // Do not move if already at final waypoint
            if (_nextWaypointIndex >= _waypoints.Count)
                return new AgentSnapshot(Id, Position, _maxSpeed, true);

            
            var distToNextWaypoint = DistToNextWaypoint();
            if (distToNextWaypoint > distToTravel)
            {
                MoveTowardsNextWaypoint(distToTravel);
                break;
            }
            
            MoveTowardsNextWaypoint(distToNextWaypoint);
            distToTravel -= distToNextWaypoint;
            _nextWaypointIndex++;
        }

        return new AgentSnapshot(Id, Position, _maxSpeed, false);
    }

    private double DistToNextWaypoint()
    {
        var difference = NextWaypoint - Position;
        return Math.Sqrt(difference.X * difference.X + difference.Y * difference.Y);
    }

    private void MoveTowardsNextWaypoint(double distance)
    {
        var directionVector = NextWaypoint - Position;
        if (directionVector is { X: 0, Y: 0 } || distance == 0) return;
        Position += directionVector.GetNormalized() * distance;
    }
}