using System.Text.Json;

namespace Simulator.Server.Payloads;

public class CreatePayload
{
    public double agentDensity { get; set; }
    public required List<List<int>> agentStartPositions { get; set; }
    public int agentRadius { get; set; }
    public int seed { get; set; }
    public int exitRadius { get; set; }
    public double speedShape { get; set; }
    public double speedScale { get; set; }
    public JsonElement layout { get; set; }
}