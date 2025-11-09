namespace Simulator.Core.Geometry.Primitives;

public class Triangle(Vector2Int a, Vector2Int b, Vector2Int c)
{
    public Vector2Int a = a;
    public Vector2Int b = b;
    public Vector2Int c = c;
    public Direction direction;

    // Always print triangle vertices in CCW order
    public override string ToString() => direction == Direction.CCW ? $"<{a}, {b}, {c}>" : $"<{c}, {b}, {a}>";
}