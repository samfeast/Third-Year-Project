namespace Simulator.Core.Geometry.Primitives;

// Struct to represent a fraction without precision issues
// This struct does not differentiate between +ve and -ve infinities
// All infinities are equal, infinity is larger than all fractions (i.e (-1)/0 > 1/2)
public struct LongFraction(long num, long den)
{
    private readonly long _numerator = num;
    private readonly long _denominator = den;
    
    public bool IsInfinity => _denominator == 0;

    public static bool operator <(LongFraction a, LongFraction b)
    {
        if (a.IsInfinity && b.IsInfinity) return false;
        if (a.IsInfinity) return false;
        if (b.IsInfinity) return true;
        
        return a._numerator * b._denominator < b._numerator * a._denominator;
    }

    public static bool operator >(LongFraction a, LongFraction b)
    {
        if (a.IsInfinity && b.IsInfinity) return false;
        if (a.IsInfinity) return true;
        if (b.IsInfinity) return false;
        
        return a._numerator * b._denominator > b._numerator * a._denominator;
    }
    
    public static bool operator ==(LongFraction a, LongFraction b)
    {
        if (a.IsInfinity && b.IsInfinity) return true;
        if (a.IsInfinity || b.IsInfinity) return false;
        
        return a._numerator * b._denominator == b._numerator * a._denominator;
    }
    
    public static bool operator !=(LongFraction a, LongFraction b) => !(a == b);
    
    public override bool Equals(object? obj)
    {
        return obj is LongFraction other && this == other;
    }

    public double Compute()
    {
        return _numerator / (double)_denominator;
    }
    
    public override int GetHashCode()
    {
        if (IsInfinity) return int.MaxValue; // All infinities have the same hash code
        
        return HashCode.Combine(_numerator, _denominator);
    }
    
    public override string ToString()
    {
        return IsInfinity ? "+-Infinity" : $"{_numerator}/{_denominator}";
    }
}