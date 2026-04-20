using System.Text.Json;

namespace Simulator.Server.Payloads;

public class CreatePayload
{
    public double agentDensity  { get; set; }
    public int seed { get; set; }
    public JsonElement layout { get; set; }
}