using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Utils;

public class MovementConstraints
{
    public struct ConflictingAgent
    {
        public Vector2Int Position;
        public Vector2 Velocity;
        public int Radius;
    }
    public readonly List<ConflictingAgent> ConflictingAgents = [];
    public readonly HashSet<EdgeKey> ConflictingWalls = []; // EdgeKey is direction agnostic

    public void AddConflictingAgent(Vector2Int position, Vector2 velocity, int radius)
    {
        var conflictingAgent = new ConflictingAgent
        {
            Position = position,
            Velocity = velocity,
            Radius = radius
        };
        ConflictingAgents.Add(conflictingAgent);
    }

    public void AddConflictingWall(Vector2Int a, Vector2Int b)
    {
        ConflictingWalls.Add(new EdgeKey(a, b));
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Constrained by {ConflictingAgents.Count} agents:");
        foreach (var agent in ConflictingAgents)
        {
            sb.AppendLine($"\tPosition: {agent.Position}\tVelocity: {agent.Velocity}\tRadius: {agent.Radius}");
        }
        
        sb.AppendLine($"Constrained  by {ConflictingWalls.Count} walls:");
        foreach (var wall in ConflictingWalls)
        {
            sb.AppendLine($"\t{wall.ToString()}");
        }
        
        return sb.ToString();
    }
}