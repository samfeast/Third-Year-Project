namespace Simulator.Core.Geometry.Primitives;

public class Triangle
{
    public Vector2Int a;
    public Vector2Int b;
    public Vector2Int c;
    public Direction direction;

    public enum Direction
    {
        CW,
        ACW,
    }
}