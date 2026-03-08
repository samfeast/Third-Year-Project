using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Simulator.Core;
using Simulator.Core.Geometry.Primitives;
using Simulator.Core.Geometry.Utils;
using Simulator.IO;
using Simulator.IO.Json;
using Simulator.Server.ManagerCommands;
using Simulator.Server.Payloads;

namespace Simulator.Server;

public class ClientMessage
{
    public required string ClientId { get; set; }
    public required string Command { get; set; }
    public JsonElement Payload { get; set; }
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
                    
                    var clientId = Guid.Parse(message.ClientId);
                    
                    Console.WriteLine($"Received message from client {clientId}: {messageJson}");

                    switch (message.Command)
                    {
                        case "create":
                            HandleCreate(manager, clientId, message.Payload);
                            break;
                        case "get-snapshots":
                            var bytes = HandleGetSnapshots(manager, clientId, message.Payload);
                            await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, context.RequestAborted);
                            break;
                        default:
                            Console.WriteLine($"Unknown command received: {message.Command}");
                            break;
                    }
                }
            }
            Console.WriteLine("Client disconnected");
        });
    }

    private static void HandleCreate(SimulationManager manager, Guid clientId, JsonElement payload)
    {
        var data = payload.Deserialize<CreatePayload>();
        
        if (data == null)
            throw new Exception("Invalid create payload");
                        
        var deserialiser = new JsonGeometryDeserialiser();
        var geometry = deserialiser.Deserialise(data.layout);
        var numAgents = (int)(500 * data.agentDensity);
                        
        var config = new SimulationConfig {
            Geometry = geometry,
            TimeStep = TIME_STEP,
            NumAgents = numAgents,
            Seed = 100,
        };
                        
        manager.EnqueueCommand(new CreateSimulationCommand(clientId, config, 0));
    }

    private static byte[] HandleGetSnapshots(SimulationManager manager, Guid clientId, JsonElement payload)
    {
        var data = payload.Deserialize<GetSnapshotsPayload>();

        if (data == null)
            throw new Exception("Invalid get-snapshots payload");

        // Calculate buffer size capped at 200 steps
        var targetBufferSize = Math.Min((int)(data.playbackSpeed * TARGET_BUFFER_DURATION / TIME_STEP), 200);
        // Work out number of steps needed to fill buffer
        var numSteps = targetBufferSize - (data.lastBufferedStep - data.lastDisplayedStep);

        var simulator = manager.GetSimulator(clientId);
        
        var serialiser = new JsonSimulationSnapshotsSerialiser();
        var snapshots = simulator.GetSnapshots(data.lastBufferedStep, numSteps);
        var snapshotsJson = serialiser.Serialise(snapshots, 1);
        
        return Encoding.UTF8.GetBytes(snapshotsJson);
    }
}

