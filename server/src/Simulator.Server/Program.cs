using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Simulator.Core;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;
using Simulator.IO;
using Simulator.IO.Json;
using Simulator.Server.ManagerCommands;

namespace Simulator.Server;

public class ClientMessage
{
    public required string command { get; set; }
    public int preset { get; set; }
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder.Services);

        var app = builder.Build();

        Configure(app);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSingleton<SimulationManager>();
        services.AddHostedService<SimulationHostedService>();
    }

    private static void Configure(WebApplication app)
    {
        app.UseWebSockets();

        var manager = app.Services.GetRequiredService<SimulationManager>();
        manager.SnapshotProduced += (id, snapshot) =>
        {
            if (snapshot.Step % 100 == 0 || snapshot.AllComplete)
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Sim {id} Tick {snapshot.Step}");
        };

        app.MapControllers();

        // WebSocket endpoint
        app.Map("/ws", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            
            var clientId = Guid.NewGuid();
            Console.WriteLine($"Client {clientId} connected via WebSocket");

            // Subscribe to SnapshotProduced
            manager.SnapshotProduced += SendSnapshots;

            var buffer = new byte[1024];
            while (!context.RequestAborted.IsCancellationRequested && webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(buffer, context.RequestAborted);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", context.RequestAborted);
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received from client: {messageJson}");
                    var message = JsonSerializer.Deserialize<ClientMessage>(messageJson);


                    if (message == null) continue;

                    // Crude message processing for hardcoded presets
                    var readRegistry = DeserialiserRegistryFactory.Default<InputGeometry>();
                    if (message is { command: "create", preset: 1 })
                    {
                        var inputGeometry = readRegistry.Load("../../../scripts/data/vertices1.json");
                        var config = new SimulationConfig {
                            Geometry = inputGeometry,
                            TimeStep = 0.1f,
                            NumAgents = 10,
                            Seed = 100,
                        };
                        manager.EnqueueCommand(new CreateSimulationCommand(clientId, config, 0));
                    }
                    if (message is { command: "create", preset: 2 })
                    {
                        var inputGeometry = readRegistry.Load("../../../scripts/data/vertices2.json");
                        var config = new SimulationConfig {
                            Geometry = inputGeometry,
                            TimeStep = 0.1f,
                            NumAgents = 30,
                            Seed = 101
                        };
                        manager.EnqueueCommand(new CreateSimulationCommand(clientId, config, 0));
                    }
                    if (message is { command: "create", preset: 3 })
                    {
                        var inputGeometry = readRegistry.Load("../../../scripts/data/vertices3.json");
                        var config = new SimulationConfig {
                            Geometry = inputGeometry,
                            TimeStep = 0.1f,
                            NumAgents = 80,
                            Seed = 102,
                            Target = new Vector2(250, 250)
                        };
                        manager.EnqueueCommand(new CreateSimulationCommand(clientId, config, 0));
                    }
                    if (message is { command: "create", preset: 4 })
                    {
                        var inputGeometry = readRegistry.Load("../../../scripts/data/vertices4.json");
                        var config = new SimulationConfig {
                            Geometry = inputGeometry,
                            TimeStep = 0.1f,
                            NumAgents = 100,
                            Seed = 103,
                            Target = new Vector2(3000, 2000)
                        };
                        manager.EnqueueCommand(new CreateSimulationCommand(clientId, config, 0));
                    }
                }
            }

            // Unsubscribe from SnapshotProduced
            manager.SnapshotProduced -= SendSnapshots;
            Console.WriteLine($"Client {clientId} disconnected");
            return;

            void SendSnapshots(Guid id, SimulationSnapshot snapshot)
            {
                if (webSocket.State != WebSocketState.Open) return;

                var serialiser = new JsonSimulationSnapshotSerialiser();
                var data = serialiser.Serialise(snapshot, 1);
                var bytes = Encoding.UTF8.GetBytes(data);
                // Could drop exceptions here
                _ = webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, context.RequestAborted);
            }
        });
    }
}

