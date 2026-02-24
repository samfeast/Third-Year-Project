using Simulator.Core;

namespace Simulator.Server.SimulationCommands;

public interface ISimulationCommand
{
    public void Apply(SimulationEngine engine);
}