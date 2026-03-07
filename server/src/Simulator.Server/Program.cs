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
    public required string clientId { get; set; }
    public required string command { get; set; }
    public JsonElement payload { get; set; }
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
                            HandleCreate(manager, clientId, message.payload);
                            break;
                        case "get-snapshots":
                            HandleGetSnapshots(manager, clientId, message.payload);
                            break;
                        default:
                            Console.WriteLine($"Unknown command received: {message.command}");
                            break;
                    }
                }
            }
            Console.WriteLine($"Client disconnected");
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
            TimeStep = 0.1f,
            NumAgents = numAgents,
            Seed = 100,
        };
                        
        manager.EnqueueCommand(new CreateSimulationCommand(clientId, config, 0));
    }

    private static void HandleGetSnapshots(SimulationManager manager, Guid clientId, JsonElement payload)
    {
        var data = payload.Deserialize<GetSnapshotsPayload>();

        if (data == null)
            throw new Exception("Invalid get-snapshots payload");

        // Calculate buffer size needed for 5 seconds of playback capped at 200 steps
        var targetBufferSize = Math.Min((int)(50 * data.playbackSpeed), 200);
        var numSteps = targetBufferSize - (data.lastBufferedStep - data.lastDisplayedStep);

        var simulator = manager.GetSimulator(clientId);
        var snapshots = simulator.GetSnapshots(data.lastBufferedStep, numSteps);
        
        // Serialise snapshots and send over WS
    }
}

