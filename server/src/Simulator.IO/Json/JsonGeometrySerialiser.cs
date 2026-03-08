using System.Text.Json;
using Simulator.Core.Geometry.Utils;
using Simulator.IO.Utils;

namespace Simulator.IO.Json;

// Serialisation logic for all JSON geometry writers
// When adding a new version, update Version bounds in CanWrite(), add version to switch in Serialise(), and implement
// SerialiseVn()
internal class JsonGeometrySerialiser : ISerialiser<InputGeometry>
{
    public bool CanWrite(DataFormat format, DataType type, int version)
    {
        return format == DataFormat.JSON && type == DataType.Geometry && version is >= 1 and <= 1;
    }
    
    public byte[] Serialise(InputGeometry inputGeometry, int version)
    {
        return version switch
        {
            1 => SerialiseV1(inputGeometry),
            _ => throw new NotImplementedException($"JSON geometry serialiser does not implement version {version}")
        };
    }
    
    /* Format:
     * {
     *    "type": "geometry",
     *    "version": 1,
     *    "positive": [[x1,y1],[x2,y2],...],
     *    "negatives": [
     *       [[x3,y3],[x4,y4],...],
     *       [[x5,y5],[x6,y6],...],
     *       ...
     *    ]
     * }
     */
    private byte[] SerialiseV1(InputGeometry inputGeometry)
    {
        var positive = inputGeometry.Positive.ToListInt();
        List<List<int[]>> negatives = [];
        foreach (var negative in inputGeometry.Negatives)
            negatives.Add(negative.ToListInt());

        var data = new { type = "geometry", version = 1, positive, negatives };

        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}