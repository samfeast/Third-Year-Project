using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core.Geometry.Triangulators;

public interface ITriangulator
{
    List<Triangle> Triangulate(InputGeometry inputGeometry);
}