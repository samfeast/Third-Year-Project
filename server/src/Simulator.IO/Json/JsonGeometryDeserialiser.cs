using System.Text.Json;
using Simulator.Core.Geometry.Primitives;
using Simulator.IO.Utils;

namespace Simulator.IO.Json;

// Deserialisation logic for all JSON geometry parsers
// When adding a new version, update Version bounds in CanRead(), add version to switch in Deserialise(), and implement
// DeserialiseVn()
public class JsonGeometryDeserialiser : IDeserialiser<InputGeometry>
{
    public bool CanRead(DataProbe probe)
    {
        return probe is { Format: DataFormat.JSON, Type: DataType.Geometry, Version: >= 1 and <= 1 };
    }

    public InputGeometry Deserialise(Stream s)
    {
        // Extract the version and call the relevant deserialiser
        using var doc = JsonDocument.Parse(s);
        var root = doc.RootElement;

        if (!root.TryGetProperty("version", out var versionProp) ||
            versionProp.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidDataException("Missing or invalid version field.");
        }
        
        var version = versionProp.GetInt32();
        
        return version switch
        {
            1 => DeserialiseV1(root),
            _ => throw new NotImplementedException($"JSON geometry deserialiser does not implement version {version}")
        };
    }
    
    private InputGeometry DeserialiseV1(JsonElement root)
    {
        return new InputGeometry();
    }
}