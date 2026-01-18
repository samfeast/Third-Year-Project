namespace Simulator.Core.Geometry.Primitives;

// Type to represent point on an integer grid
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
    
    // Convert to a Vector2Fraction: (X/1,Y/1)
    public Vector2Fraction ToVector2Fraction()
    {
        var x = new LongFraction(X, 1);
        var y = new LongFraction(Y, 1);
        return new Vector2Fraction(x, y);
    }
}