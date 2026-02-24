namespace Simulator.Server;

public class SimulationHostedService(SimulationManager manager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await manager.RunLoop(stoppingToken);
    }
}