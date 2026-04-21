using System.Text.Json;
using Simulator.Core.Geometry.Utils;
using Simulator.Core.Utils;
using Simulator.IO.Utils;

namespace Simulator.IO.Json;

// Serialisation logic for all JSON heatmap writers
// When adding a new version, update Version bounds in CanWrite(), add version to switch in Serialise(), and implement
// SerialiseVn()
internal class JsonHeatmapSerialiser : ISerialiser<ResultsCollector.BlurredHeatmap>
{
    public bool CanWrite(DataFormat format, DataType type, int version)
    {
        return format == DataFormat.JSON && type == DataType.Heatmap && version is >= 1 and <= 1;
    }
    
    public byte[] Serialise(ResultsCollector.BlurredHeatmap heatmap, int version)
    {
        return version switch
        {
            1 => SerialiseV1(heatmap),
            _ => throw new NotImplementedException($"JSON geometry serialiser does not implement version {version}")
        };
    }
    
    /* Format:
     * {
     *    "type": "heatmap",
     *    "version": 1,
     *    origin: [x,y],
     *    cellSize: z,
     *    width: w,
     *    height: h,
     *    heatmap: [a,b,c,d,...]
     * }
     */
    private byte[] SerialiseV1(ResultsCollector.BlurredHeatmap heatmap)
    {
        var data = new
        {
            type = "heatmap",
            version = 1,
            origin = heatmap.Origin,
            cellSize = heatmap.CellSize,
            width = heatmap.Width,
            height = heatmap.Height,
            // Flattens float[][]
            heatmap = heatmap.Heatmap.SelectMany(row => row).ToArray()
        };

        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}