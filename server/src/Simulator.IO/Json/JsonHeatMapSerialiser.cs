using System.Text.Json;
using Simulator.Core.Geometry.Utils;
using Simulator.Core.Utils;
using Simulator.IO.Utils;

namespace Simulator.IO.Json;

// Serialisation logic for all JSON heatmap writers
// When adding a new version, update Version bounds in CanWrite(), add version to switch in Serialise(), and implement
// SerialiseVn()
internal class JsonHeatMapSerialiser : ISerialiser<List<ResultsCollector.HeatMapCell>>
{
    public bool CanWrite(DataFormat format, DataType type, int version)
    {
        return format == DataFormat.JSON && type == DataType.HeatMap && version is >= 1 and <= 1;
    }
    
    public byte[] Serialise(List<ResultsCollector.HeatMapCell> heatMapCells, int version)
    {
        return version switch
        {
            1 => SerialiseV1(heatMapCells),
            _ => throw new NotImplementedException($"JSON geometry serialiser does not implement version {version}")
        };
    }
    
    /* Format:
     * {
     *    "type": "heatmap",
     *    "version": 1,
     *    "positive": [[x1,y1],[x2,y2],...],
     *    "negatives": [
     *       [[x3,y3],[x4,y4],...],
     *       [[x5,y5],[x6,y6],...],
     *       ...
     *    ]
     * }
     */
    private byte[] SerialiseV1(List<ResultsCollector.HeatMapCell> heatMapCells)
    {
        var cells = new List<object>();

        foreach (var cell in heatMapCells)
        {
            var triangle = new[]
            {
                new[] { cell.Triangle.A.X, cell.Triangle.A.Y },
                new[] { cell.Triangle.B.X, cell.Triangle.B.Y },
                new[] { cell.Triangle.C.X, cell.Triangle.C.Y }
            };

            cells.Add(new
            {
                triangle,
                value = cell.Value
            });
        }

        var data = new
        {
            type = "heatmap",
            version = 1,
            cells
        };

        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}