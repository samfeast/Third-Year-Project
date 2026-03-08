using System.Text.Json;
using Simulator.Core.Geometry;
using Simulator.IO.Utils;

namespace Simulator.IO.Json;

// Serialisation logic for all JSON navmesh writers
// When adding a new version, update Version bounds in CanWrite(), add version to switch in Serialise(), and implement
// SerialiseVn()
internal class JsonMeshSerialiser : ISerialiser<NavMesh>
{
    public bool CanWrite(DataFormat format, DataType type, int version)
    {
        return format == DataFormat.JSON && type == DataType.Mesh && version is >= 1 and <= 1;
    }
    
    public byte[] Serialise(NavMesh navMesh, int version)
    {
        return version switch
        {
            1 => SerialiseV1(navMesh),
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
    private byte[] SerialiseV1(NavMesh navMesh)
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

        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}