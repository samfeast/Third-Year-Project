namespace Simulator.Core.Geometry.Primitives;

public class TriangleFraction(Vector2Fraction a, Vector2Fraction b, Vector2Fraction c)
{
    public Vector2Fraction A = a;
    public Vector2Fraction B = b;
    public Vector2Fraction C = c;
    
    public bool ContainsPoint(Vector2Fraction p)
    {
        var d1 = Sign(p, A, B);
        var d2 = Sign(p, B, C);
        var d3 = Sign(p, C, A);

        var hasNeg = d1.IsNegative() || d2.IsNegative() || d3.IsNegative();
        var hasPos = d1.IsPositive() || d2.IsPositive() || d3.IsPositive();

        return !(hasNeg && hasPos);
    }

    private LongFraction Sign(Vector2Fraction v0, Vector2Fraction v1, Vector2Fraction v2)
    {
        return (v0.X - v2.X) * (v1.Y - v2.Y) - (v1.X - v2.X) * (v0.Y - v2.Y);
    }
    
    public override string ToString() => $"<{A}, {B}, {C}>";
}