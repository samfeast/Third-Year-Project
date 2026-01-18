namespace Simulator.Core.Geometry.Primitives;

// Type to represent point in 2D space, with values stored as rational fractions
public readonly struct Vector2Fraction(LongFraction x, LongFraction y) : IEquatable<Vector2Fraction>
{
    public LongFraction X { get; } = x;
    public LongFraction Y { get; } = y;
    
    public static Vector2Fraction operator +(Vector2Fraction a, Vector2Fraction b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2Fraction operator -(Vector2Fraction a, Vector2Fraction b) => new(a.X - b.X, a.Y - b.Y);
    
    public static bool operator ==(Vector2Fraction a, Vector2Fraction b) => a.Equals(b);
    public static bool operator !=(Vector2Fraction a, Vector2Fraction b) => !a.Equals(b);
    
    public bool Equals(Vector2Fraction other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is Vector2Fraction other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";
}