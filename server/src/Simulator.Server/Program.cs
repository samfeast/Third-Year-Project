using Simulator.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<SimulationManager>();
builder.Services.AddHostedService<SimulationHostedService>();

var app = builder.Build();

var manager = app.Services.GetRequiredService<SimulationManager>();

manager.SnapshotProduced += (id, snapshot) =>
{
    Console.WriteLine(
        $"[{DateTime.Now:HH:mm:ss.fff}] Sim {id} Tick {snapshot.Step}");
};

app.MapControllers();

app.MapGet("/", () => "Hello, simulation server is running!");

app.Run();
