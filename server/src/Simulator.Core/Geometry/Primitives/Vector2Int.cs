namespace Simulator.Core.Geometry.Primitives;

public readonly struct Vector2Int(int x, int y) : IEquatable<Vector2Int>
{
    public int X { get; } = x;
    public int Y { get; } = y;
    
    public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new(a.X - b.X, a.Y - b.Y);
    
    public static bool operator ==(Vector2Int a, Vector2Int b) => a.Equals(b);
    public static bool operator !=(Vector2Int a, Vector2Int b) => !a.Equals(b);
    
    public bool Equals(Vector2Int other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is Vector2Int other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";
}