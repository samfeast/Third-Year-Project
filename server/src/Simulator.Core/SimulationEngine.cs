using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core;

public class SimulationEngine
{
    public NavMesh? Mesh { get; private set; }

    public bool SetupSimulation(InputGeometry inputGeometry)
    {
        Mesh = NavMeshGenerator.GenerateNavMesh(inputGeometry);
        return true;
    }
}