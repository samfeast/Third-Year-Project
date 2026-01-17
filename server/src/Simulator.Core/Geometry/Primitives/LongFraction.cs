namespace Simulator.Core.Geometry.Primitives;

// Struct to represent a fraction without precision issues
// This struct does not differentiate between +ve and -ve infinities
// All infinities are equal, infinity is larger than all fractions (i.e. even (-2)/0 > 1/2)
// Not protected against overflow - avoid numerators and denominators exceeding ~2^20 for safety
public readonly struct LongFraction : IEquatable<LongFraction>
{
    private readonly long _numerator;
    private readonly long _denominator;

    public LongFraction(long num, long den)
    {
        if (den < 0)
        {
            num = -num;
            den = -den;
        }

        long gcd = Gcd(Math.Abs(num), den);
        _numerator = num / gcd;
        _denominator = den / gcd;
    }
    
    public bool IsInfinity => _denominator == 0;
    
    public bool IsZero => !IsInfinity && _numerator == 0;
    public bool IsOne => !IsInfinity && _numerator == _denominator;
    
    // Infinities not considered either positive or negative
    // 0 considered positive
    public bool IsPositive()
    {
        if (IsInfinity) return false;
        if (IsZero) return true;
        return _numerator > 0;
    }

    public bool IsNegative()
    {
        if (IsInfinity) return false;
        if (IsZero) return false;
        return _numerator < 0;
    }

    public static LongFraction operator +(LongFraction a, LongFraction b)
    {
        if (a.IsInfinity || b.IsInfinity) return new LongFraction(1, 0);

        var num = a._numerator * b._denominator + b._numerator * a._denominator;
        var den = a._denominator * b._denominator;
        
        return new LongFraction(num, den);
    }
    
    public static LongFraction operator -(LongFraction a, LongFraction b)
    {
        if (a.IsInfinity || b.IsInfinity) return new LongFraction(1, 0);

        var num = a._numerator * b._denominator - b._numerator * a._denominator;
        var den = a._denominator * b._denominator;
        
        return new LongFraction(num, den);
    }
    
    public static LongFraction operator *(LongFraction a, LongFraction b)
    {
        if (a.IsInfinity || b.IsInfinity) return new LongFraction(1, 0);
        
        var numerator = a._numerator * b._numerator;
        var denominator = a._denominator * b._denominator;
        return new LongFraction(numerator, denominator);
    }

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
    
    public bool Equals(LongFraction other) => this == other;
    public override bool Equals(object? obj)
    {
        return obj is LongFraction other && this == other;
    }

    // Evaluate all infinities to positive infinity
    public double Compute()
    {
        if (IsInfinity) return double.PositiveInfinity;
        
        return _numerator / (double)_denominator;
    }
    
    public override int GetHashCode()
    {
        // All infinities have the same hash code
        if (IsInfinity) return int.MaxValue;
        
        return HashCode.Combine(_numerator, _denominator);
    }
    
    public override string ToString()
    {
        return IsInfinity ? "+-Infinity" : $"{_numerator}/{_denominator}";
    }
    
    // Euclid's GCD algorithm
    private static long Gcd(long a, long b)
    {
        while (b != 0)
        {
            long temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}