using System.Diagnostics;

namespace Simulator.Core.Geometry.Primitives;

public class BoundingBox
{
    public int MinX { get; private set; } = int.MaxValue;
    public int MaxX { get; private set; } = int.MinValue;
    public int MinY { get; private set; } = int.MaxValue;
    public int MaxY { get; private set; } = int.MinValue;

    public BoundingBox(IEnumerable<Vector2Int> vertices)
    {
        foreach (var vertex in vertices)
        {
            UpdateBounds(vertex);
        }
    }
    
    private void UpdateBounds(Vector2Int vertex)
    {
        MinX = Math.Min(MinX, vertex.X);
        MaxX = Math.Max(MaxX, vertex.X);
        MinY = Math.Min(MinY, vertex.Y);
        MaxY = Math.Max(MaxY, vertex.Y);
    }
}