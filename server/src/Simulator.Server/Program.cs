using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Simulator.Core;
using Simulator.Core.Geometry.Utils;
using Simulator.IO;
using Simulator.IO.Utils;
using Simulator.Server.ManagerCommands;
using Simulator.Server.Payloads;

namespace Simulator.Server;

public class ClientMessage
{
    public required string clientId { get; set; }
    public required string command { get; set; }
    public JsonElement payload { get; set; }
}

public class Program
{
    private const double TARGET_BUFFER_DURATION = 5.0f;
    private const double TIME_STEP = 0.1f;
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
            
            Console.WriteLine("New websocket connection established");

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
                    var message = JsonSerializer.Deserialize<ClientMessage>(messageJson);
                    
                    if (message == null) continue;

                    var clientId = Guid.Parse(message.clientId);
                    
                    Console.WriteLine($"Received message from client {clientId}: {messageJson}");

                    switch (message.command)
                    {
                        case "create":
                            var createPayload = message.payload.Deserialize<CreatePayload>();
                            if (createPayload == null)
                                throw new Exception("Missing payload in create command");
                            
                            HandleCreate(manager, clientId, createPayload);
                            break;
                        case "get-snapshots":
                            var getSnapshotsPayload = message.payload.Deserialize<GetSnapshotsPayload>();
                            if (getSnapshotsPayload == null)
                                throw new Exception("Missing payload in get-snapshots command");
                            
                            var bytes = HandleGetSnapshots(manager, clientId, getSnapshotsPayload);
                            if (bytes.Length > 0)
                                await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, context.RequestAborted);
                            
                            break;
                        default:
                            Console.WriteLine($"Unknown command received: {message.command}");
                            break;
                    }
                }
            }
            Console.WriteLine("Client disconnected");
        });
    }

    private static void HandleCreate(SimulationManager manager, Guid clientId, CreatePayload data)
    {
        // Load json layout into InputGeometry object
        var geometry = data.layout.Deserialise<InputGeometry>();

        // Area is in cm^2 but agent density is in agents per m^2, so divide by 100^2
        var numAgents = (int)(geometry.Area * data.agentDensity / 10_000);
                        
        var config = new SimulationConfig {
            Geometry = geometry,
            TimeStep = TIME_STEP,
            NumAgents = numAgents,
            Seed = 100,
        };
                        
        manager.EnqueueCommand(new CreateSimulationCommand(clientId, config, 0));
    }

    private static byte[] HandleGetSnapshots(SimulationManager manager, Guid clientId, GetSnapshotsPayload data)
    {
        // Calculate buffer size capped at 200 steps
        var targetBufferSize = (int) Math.Round(data.playbackSpeed * TARGET_BUFFER_DURATION / TIME_STEP);
        // Work out number of steps needed to fill buffer
        var numSteps = Math.Min(targetBufferSize, 200) - (data.lastBufferedStep - data.lastDisplayedStep);

        // Return empty byte array if buffer is already full
        if (numSteps <= 0)
            return [];

        var simulator = manager.GetSimulator(clientId);
        var snapshots = simulator.GetSnapshots(Math.Max(data.lastBufferedStep, 0), numSteps);

        if (snapshots.Count == 0)
            return [];
        
        return snapshots.Serialise(DataFormat.JSON, 2);
    }
}

