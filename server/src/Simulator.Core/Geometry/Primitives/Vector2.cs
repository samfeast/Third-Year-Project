using System.Diagnostics;

namespace Simulator.Core.Geometry.Primitives;

// Type to represent point in 2D space
public readonly struct Vector2(double x, double y) : IEquatable<Vector2>
{
    private const double Epsilon = 1e-9;
    
    public double X { get; } = x;
    public double Y { get; } = y;
    
    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator +(Vector2Int a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2Int a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 a, double b) => new(a.X * b, a.Y * b);
    
    public static bool operator ==(Vector2 a, Vector2 b) => a.Equals(b);
    public static bool operator !=(Vector2 a, Vector2 b) => !a.Equals(b);

    // Epsilon to avoid floating point errors
    public bool Equals(Vector2 other) => Math.Abs(X - other.X) < Epsilon && Math.Abs(Y - other.Y) < Epsilon;
    public override bool Equals(object? obj) => obj is Vector2 other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(X, Y);

    public Vector2Fraction ToVector2Fraction()
    {
        var x = LongFraction.ToLongFraction(X);
        var y = LongFraction.ToLongFraction(Y);
        return new Vector2Fraction(x, y);
    }

    public Vector2 Round(int places)
    {
        if (places < 0)
            places = 0;
        
        return new Vector2(Math.Round(X, places), Math.Round(Y, places));
    }

    public Vector2 GetNormalized()
    {
        var magnitude = Math.Sqrt(X * X + Y * Y);
        return new Vector2(X / magnitude, Y / magnitude);
    }

    public override string ToString() => $"({X}, {Y})";
}