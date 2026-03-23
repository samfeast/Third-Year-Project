using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Utils;

public struct MovementConstraints
{
    public struct ConflictingAgent
    {
        public Vector2Int Position;
        public Vector2 Velocity;
    }

    public struct ConflictingWall
    {
        public Vector2Int A;
        public Vector2Int B;
    }
    
    public List<ConflictingAgent> ConflictingAgents;
    public List<ConflictingWall> ConflictingWalls;
}