using System.Text.Json;
using Simulator.Core;
using Simulator.IO.Utils;

namespace Simulator.IO.Json;

public class JsonSimulationSnapshotsSerialiser : ISerialiser<List<SimulationSnapshot>>
{
    public bool CanWrite(DataFormat format, DataType type, int version)
    {
        return format == DataFormat.JSON && type == DataType.SimulationSnapshots && version is >= 1 and <= 1;
    }
    
    public string Serialise(List<SimulationSnapshot> snapshots, int version, Stream? s = null)
    {
        return version switch
        {
            1 => SerialiseV1(snapshots, s),
            _ => throw new NotImplementedException($"JSON simulation snapshots serialiser does not implement version {version}")
        };
    }
    
    
    // Format:
    // {
    //     "version": 1,
    //     "snapshots": [
    //         {
    //             "step": n1,
    //             "final": false,
    //             "agents": [
    //                 {
    //                     "id": a1,
    //                     "position": [x1,y1],
    //                     "speed": v1
    //                 },
    //                 {
    //                     "id": a2,
    //                     "position": [x2,y2],
    //                     "speed": v2
    //                 }...
    //             ]
    //         },
    //         {
    //             "step": n2,
    //             "final": false,
    //             "agents": [
    //                 {
    //                     "id": a3,
    //                     "position": [x3,y3]
    //                     "speed": v3
    //                 },
    //                 {
    //                     "id": a4,
    //                     "position": [x4,y4]
    //                     "speed": v4
    //                 }...
    //             ]
    //         }...
    //     ]
    // }


    private string SerialiseV1(List<SimulationSnapshot> snapshots, Stream? s = null)
    {
        var data = new
        {
            version = 1,
            snapshots = snapshots.Select(snapshot => new
            {
                step = snapshot.Step,
                final = snapshot.AllComplete,
                agents = snapshot.Ids
                    .Select((id, i) => new
                    {
                        id,
                        position = new[]
                        {
                            snapshot.Positions[i].X,
                            snapshot.Positions[i].Y
                        },
                        speed = snapshot.Speeds[i]
                    })
            })
        };
        
        if (s == null)
        {
            return JsonSerializer.Serialize(data);
        }
        
        JsonSerializer.Serialize(s, data);
        return string.Empty;
    }
}