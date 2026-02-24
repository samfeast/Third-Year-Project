using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Shapes;
using Simulator.Core.Geometry.Utils;

namespace Simulator.Core;

public struct SimulationConfig(InputGeometry geometry, double timeStep, int numAgents, int? seed = null)
{
    public InputGeometry Geometry = geometry;
    public double TimeStep = timeStep;
    public int NumAgents = numAgents;
    public int? Seed = seed;
    
    private static readonly InputGeometry Preset1Geometry = new()
    {
        Positive = new Polygon(
        [new Vector2Int(0, 0), 
            new Vector2Int(500, 0), 
            new Vector2Int(500, 500), 
            new Vector2Int(0, 500)]),
        Negatives = []
    };

    public static readonly SimulationConfig Preset1 = new()
    {
        Geometry = Preset1Geometry,
        TimeStep = 0.1f,
        NumAgents = 5,
        Seed = 100
    };
}