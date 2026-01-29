namespace Simulator.Core.Geometry.Primitives;

// Representation of a triangle defined by three points on an integer grid
public class Triangle
{
    public Vector2Int A;
    public Vector2Int B;
    public Vector2Int C;

    // Always store triangle with CCW winding for consistency
    public Triangle(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        if (Sign(a, b, c) < 0)
            (b, c) = (c, b);

        A = a;
        B = b;
        C = c;
    }
    
    // Returns true if p is inside the triangle or on one of the edges, false otherwise
    public bool ContainsPoint(Vector2Int p)
    {
        var d1 = Sign(p, A, B);
        var d2 = Sign(p, B, C);
        var d3 = Sign(p, C, A);

        var hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
        var hasPos = d1 > 0 || d2 > 0 || d3 > 0;

        return !(hasNeg && hasPos);
    }

    public bool IsValid() => Sign(A, B, C) != 0;

    private static int Sign(Vector2Int v0, Vector2Int v1, Vector2Int v2)
    {
        return (v0.X - v2.X) * (v1.Y - v2.Y) - (v1.X - v2.X) * (v0.Y - v2.Y);
    }

    public Vector2Fraction GetCentroid()
    {
        var x = new LongFraction(A.X + B.X + C.X, 3);
        var y = new LongFraction(A.Y + B.Y + C.Y, 3);
        return new Vector2Fraction(x, y);
    }

    public BoundingBox GetBoundingBox()
    {
        return new BoundingBox([A, B, C]);
    }
    
    public override string ToString() => $"<{A}, {B}, {C}>";
}