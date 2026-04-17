using Simulator.Core.Geometry.Primitives;

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

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Constrained by {ConflictingAgents.Count} agents:");
        foreach (var agent in ConflictingAgents)
        {
            sb.AppendLine($"\tPosition: {agent.Position}\tVelocity: {agent.Velocity}\tRadius: {agent.Radius}");
        }
        
        return sb.ToString();
    }
}