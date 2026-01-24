using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Primitives;

namespace Simulator.Core;

public class SimulationEngine
{
    public NavMesh? Mesh { get; private set; }

    public bool SetupSimulation(Polygon positive, List<Polygon> negatives)
    {
        Mesh = NavMeshGenerator.GenerateNavMesh(positive, negatives);
        return true;
    }
}