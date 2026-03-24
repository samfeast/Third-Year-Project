using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Utils;

public class MovementConstraints
{
    public readonly List<(Vector2Int Position, Vector2 Velocity)> ConflictingAgents = [];
    public readonly HashSet<EdgeKey> ConflictingWalls = []; // EdgeKey is direction agnostic

    public void AddConflictingAgent(Vector2Int position, Vector2 velocity)
    {
        ConflictingAgents.Add((position, velocity));
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
            sb.AppendLine($"\tPosition: {agent.Position}\tVelocity: {agent.Velocity}");
        }
        
        sb.AppendLine($"Constrained  by {ConflictingWalls.Count} walls:");
        foreach (var wall in ConflictingWalls)
        {
            sb.AppendLine($"\t{wall.ToString()}");
        }
        
        return sb.ToString();
    }
}