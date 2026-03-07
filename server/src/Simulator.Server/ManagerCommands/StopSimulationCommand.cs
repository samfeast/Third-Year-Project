namespace Simulator.Server.ManagerCommands;

public class StopSimulationCommand(Guid id) : IManagerCommand
{
    public Guid Id = id;

    public void Apply(SimulationManager manager)
    {
        manager.StopSimulationInternal(this);
    }
}