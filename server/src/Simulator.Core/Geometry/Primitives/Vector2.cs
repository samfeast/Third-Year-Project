namespace Simulator.Core.Geometry.Primitives;

public readonly struct Vector2(int x, int y) : IEquatable<Vector2>
{
    private const double Epsilon = 1e-9;
    
    public int X { get; } = x;
    public int Y { get; } = y;
    
    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    
    public static bool operator ==(Vector2 a, Vector2 b) => a.Equals(b);
    public static bool operator !=(Vector2 a, Vector2 b) => !a.Equals(b);

    public bool Equals(Vector2 other) => Math.Abs(X - other.X) < Epsilon && Math.Abs(Y - other.Y) < Epsilon;
    public override bool Equals(object? obj) => obj is Vector2 other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";
}