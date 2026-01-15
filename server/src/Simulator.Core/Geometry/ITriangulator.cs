using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry;

public interface ITriangulator
{
    List<Triangle> Triangulate(Polygon positive, List<Polygon> negatives);
}