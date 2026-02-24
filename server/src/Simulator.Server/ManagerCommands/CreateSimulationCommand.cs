using Simulator.Core;

namespace Simulator.Server.ManagerCommands;

public class CreateSimulationCommand(Guid id, SimulationConfig config, int priority) : IManagerCommand
{
    public Guid Id = id;
    public readonly SimulationConfig Config = config;
    public readonly int Priority = priority;

    public void Apply(SimulationManager manager)
    {
        manager.CreateSimulationInternal(this);
    }
}