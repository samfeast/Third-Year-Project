namespace Simulator.Core.Geometry.Primitives;

// Struct to represent a fraction with integer numerator and denominator
// This struct does not differentiate between +ve and -ve infinities
// All infinities are equal, infinity is larger than all fractions (i.e. even (-2)/0 > 1/2)
// There is no protection against overflow. Avoid numerators and denominators exceeding ~2^20 for safety
public readonly struct LongFraction : IEquatable<LongFraction>
{
    private readonly long _numerator;
    private readonly long _denominator;

    public LongFraction(long num, long den)
    {
        // Normalise fraction to ensure denominator is always positive
        if (den < 0)
        {
            num = -num;
            den = -den;
        }

        // Simplify fraction
        long gcd = Gcd(Math.Abs(num), den);
        _numerator = num / gcd;
        _denominator = den / gcd;
    }
    
    public static LongFraction Zero => new LongFraction(0, 1);
    
    public bool IsInfinity => _denominator == 0;
    
    public bool IsZero => !IsInfinity && _numerator == 0;
    public bool IsOne => !IsInfinity && _numerator == _denominator;
    
    // Infinity not considered either positive or negative
    // 0 is neither positive nor negative
    public bool IsPositive()
    {
        if (IsInfinity) return false;
        if (IsZero) return false;
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

    public static bool operator >=(LongFraction a, LongFraction b) => a > b || a == b;
    public static bool operator <=(LongFraction a, LongFraction b) => a < b || a == b;
    
    public static bool operator !=(LongFraction a, LongFraction b) => !(a == b);
    
    public bool Equals(LongFraction other) => this == other;
    public override bool Equals(object? obj)
    {
        return obj is LongFraction other && this == other;
    }

    public static LongFraction ToLongFraction(double value)
    {
        // Return infinity if the input is NaN or infinity
        if (double.IsNaN(value) || double.IsInfinity(value)) return new LongFraction(1, 0);
        
        long sign = Math.Sign(value);
        value = Math.Abs(value);

        long n0 = 0, d0 = 1;
        long n1 = 1, d1 = 0;

        var x = value;

        // Should always break long before 1000 iterations, but exit after 1000 as failsafe
        var iter = 0;
        while (iter < 1000)
        {
            var a = (long)Math.Floor(x);

            var n2 = a * n1 + n0;
            var d2 = a * d1 + d0;

            if (d2 > 1_000_000)
                break;

            var approximation = (double)n2 / d2;
            if (Math.Abs(approximation - value) < 1e-9)
                return new LongFraction(sign * n2, d2);

            n0 = n1; d0 = d1;
            n1 = n2; d1 = d2;

            x = 1.0 / (x - a);

            iter++;
        }

        return new LongFraction(sign * n1, d1);
    }

    // Evaluate all infinities to positive infinity
    // Generally avoid this as it defeats the point in a rational datatype
    public double Evaluate()
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