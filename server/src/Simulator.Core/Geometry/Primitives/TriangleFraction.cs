namespace Simulator.Core.Geometry.Primitives;

// Representation of a triangle defined by three vertices, each represented by rational fractions
public class TriangleFraction
{
    public Vector2Fraction A;
    public Vector2Fraction B;
    public Vector2Fraction C;
    
    // Always store triangle with CCW winding for consistency
    public TriangleFraction(Vector2Fraction a, Vector2Fraction b, Vector2Fraction c)
    {
        if (Sign(a, b, c).IsNegative())
            (b, c) = (c, b);

        A = a;
        B = b;
        C = c;
    }
    
    // Returns true if p is inside the triangle or on one of the edges, false otherwise
    // This strategic is safe from floating point inaccuracy as rational fractions are never computed
    public bool ContainsPoint(Vector2Fraction p)
    {
        var d1 = Sign(p, A, B);
        var d2 = Sign(p, B, C);
        var d3 = Sign(p, C, A);

        var hasNeg = d1.IsNegative() || d2.IsNegative() || d3.IsNegative();
        var hasPos = d1.IsPositive() || d2.IsPositive() || d3.IsPositive();

        return !(hasNeg && hasPos);
    }

    private static LongFraction Sign(Vector2Fraction v0, Vector2Fraction v1, Vector2Fraction v2)
    {
        return (v0.X - v2.X) * (v1.Y - v2.Y) - (v1.X - v2.X) * (v0.Y - v2.Y);
    }
    
    public override string ToString() => $"<{A}, {B}, {C}>";
}