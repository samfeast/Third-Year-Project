using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core.Utils;

public class SimulationConfig(
    InputGeometry geometry,
    double timeStep,
    int numAgents,
    int agentRadius,
    double speedShape,
    double speedScale,
    int exitRadius,
    List<Vector2Int> startPoints,
    int? seed = null)
{
    public InputGeometry Geometry = geometry;
    public double TimeStep = timeStep;
    public int NumAgents = numAgents;
    public int AgentRadius = agentRadius;
    public double SpeedShape = speedShape;
    public double SpeedScale = speedScale;
    public int ExitRadius = exitRadius;
    public List<Vector2Int> StartPoints = startPoints;
    public int? Seed = seed;

    public static SimulationConfig Empty() =>
        new(
            geometry: new InputGeometry(),
            timeStep: 1,
            numAgents: 0,
            agentRadius: 1,
            speedShape: 1,
            speedScale: 1,
            exitRadius: 1,
            startPoints: [],
            seed: null
        );
}