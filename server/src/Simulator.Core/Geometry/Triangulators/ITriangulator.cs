using Simulator.Core.Geometry.Shapes;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Geometry.Triangulators;

public interface ITriangulator
{
    List<Triangle> Triangulate(InputGeometry inputGeometry);
}