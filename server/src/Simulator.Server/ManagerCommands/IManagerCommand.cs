namespace Simulator.Server.ManagerCommands;

public interface IManagerCommand
{
    public void Apply(SimulationManager manager);
}