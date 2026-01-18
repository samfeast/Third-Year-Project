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
    
    public static bool operator ==(Vector2 a, Vector2 b) => a.Equals(b);
    public static bool operator !=(Vector2 a, Vector2 b) => !a.Equals(b);

    // Epsilon to avoid floating point errors
    public bool Equals(Vector2 other) => Math.Abs(X - other.X) < Epsilon && Math.Abs(Y - other.Y) < Epsilon;
    public override bool Equals(object? obj) => obj is Vector2 other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(X, Y);

    public Vector2 Round(int places)
    {
        if (places < 0)
            places = 0;
        
        return new Vector2(Math.Round(X, places), Math.Round(Y, places));
    }

    public override string ToString() => $"({X}, {Y})";
}