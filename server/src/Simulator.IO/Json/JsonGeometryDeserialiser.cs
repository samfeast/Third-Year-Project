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
    
    /* Format:
     * {
     *    "positive": [[x1,y1],[x2,y2],...],
     *    "negatives": [
     *       [[x3,y3],[x4,y4],...],
     *       [[x5,y5],[x6,y6],...],
     *       ...
     *    ]
     * }
     */
    private InputGeometry DeserialiseV1(JsonElement root)
    {
        List<Vector2Int> positiveVertices = [];
        foreach (var v in root.GetProperty("positive").EnumerateArray())
        {
            positiveVertices.Add(new Vector2Int(v[0].GetInt32(), v[1].GetInt32()));
        }
        var positive = new Polygon(positiveVertices);
        
        List<Polygon> negatives = [];
        foreach (var negative in root.GetProperty("negatives").EnumerateArray())
        {
            List<Vector2Int> negativeVertices = [];
            foreach (var v in negative.EnumerateArray())
            {
                negativeVertices.Add(new Vector2Int(v[0].GetInt32(), v[1].GetInt32()));
            }
            negatives.Add(new Polygon(negativeVertices));
        }
        
        return new InputGeometry(positive, negatives);
    }
}