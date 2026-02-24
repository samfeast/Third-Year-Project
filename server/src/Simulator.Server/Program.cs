using System.Net.WebSockets;
using System.Text;
using Simulator.Core;
using Simulator.IO.Json;
using Simulator.Server.ManagerCommands;

namespace Simulator.Server;

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
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    Console.WriteLine($"Received from client: {message}");
                    if (message == "create")
                    {
                        manager.EnqueueCommand(new CreateSimulationCommand(clientId, SimulationConfig.Preset1, 0));
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
                
                Console.WriteLine($"{snapshot.Positions.Length} positions");

                var serialiser = new JsonSimulationSnapshotSerialiser();
                var data = serialiser.Serialise(snapshot, 1);
                var bytes = Encoding.UTF8.GetBytes(data);
                // Could drop exceptions here
                _ = webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, context.RequestAborted);
            }
        });
    }
}

