using System.Text.Json;
using Simulator.Core.Geometry;
using Simulator.IO.Utils;

namespace Simulator.IO.Json;

// Serialisation logic for all JSON navmesh writers
// When adding a new version, update Version bounds in CanWrite(), add version to switch in Serialise(), and implement
// SerialiseVn()
public class JsonMeshSerialiser : ISerialiser<NavMesh>
{
    public bool CanWrite(DataFormat format, DataType type, int version)
    {
        return format == DataFormat.JSON && type == DataType.Mesh && version is >= 1 and <= 1;
    }
    
    public string Serialise(NavMesh navMesh, int version, Stream? s = null)
    {
        return version switch
        {
            1 => SerialiseV1(navMesh, s),
            _ => throw new NotImplementedException($"JSON mesh serialiser does not implement version {version}")
        };
    }
    
    /* Format:
     * {
     *    "type": "mesh",
     *    "version": 1,
     *    "triangles": [
     *       [[x1,y1],[x2,y2],[x3,y3]],
     *       [[x4,y4],[x5,y5],[x6,y6]],
     *       ...
     *    ]
     * }
     */
    private string SerialiseV1(NavMesh navMesh, Stream? s = null)
    {
        List<int[][]> triangles = [];
        foreach (var node in navMesh.Nodes)
        {
            var triangle = new int[3][];
            for (int i = 0; i < 3; i++)
            {
                triangle[i] = [node.Vertices[i].X, node.Vertices[i].Y];
            }
            triangles.Add(triangle);
        }

        var data = new { type = "mesh", version = 1, triangles };
        if (s == null)
        {
            return JsonSerializer.Serialize(data);
        }
        
        JsonSerializer.Serialize(s, data);
        return string.Empty;
    }
}