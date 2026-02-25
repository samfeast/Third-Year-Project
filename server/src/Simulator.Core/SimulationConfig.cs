using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core;

public struct SimulationConfig(InputGeometry geometry, double timeStep, int numAgents, int? seed = null, Vector2? target = null)
{
    public InputGeometry Geometry = geometry;
    public double TimeStep = timeStep;
    public int NumAgents = numAgents;
    public int? Seed = seed;
    public Vector2? Target = target;
}