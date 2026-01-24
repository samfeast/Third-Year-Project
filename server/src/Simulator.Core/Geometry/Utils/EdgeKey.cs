using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry.Utils;

// Direction-agnostic representation of an edge
public struct EdgeKey(Vector2Int a, Vector2Int b) : IEquatable<EdgeKey>
{
    private readonly Vector2Int _v0 = a;
    private readonly Vector2Int _v1 = b;

    public static bool operator ==(EdgeKey a, EdgeKey b)
    {
        return a._v0 == b._v0 && a._v1 == b._v1 || a._v0 == b._v1 && a._v1 == b._v0;
    }
    public static bool operator !=(EdgeKey a, EdgeKey b) => !(a == b);
    
    public bool Equals(EdgeKey other) => this == other;
    public override bool Equals(object? obj) => obj is EdgeKey other && Equals(other);
    
    public override int GetHashCode()
    {
        // Ensure hash is direction-agnostic
        var min = _v0.X < _v1.X || (_v0.X == _v1.X && _v0.Y <= _v1.Y) ? _v0 : _v1;
        var max = min.Equals(_v0) ? _v1 : _v0;
        return HashCode.Combine(min, max);
    }

}