namespace Simulator.Server;

public class SimulationHostedService(SimulationManager manager) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        manager.Start();

        stoppingToken.Register(manager.Stop);

        return Task.CompletedTask;
    }
}