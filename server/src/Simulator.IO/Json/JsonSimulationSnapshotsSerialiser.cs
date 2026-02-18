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
    
    public bool Serialise(Stream s, List<SimulationSnapshot> snapshots, int version)
    {
        return version switch
        {
            1 => SerialiseV1(s, snapshots),
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
    //                     "position": [x1,y1]
    //                 },
    //                 {
    //                     "id": a2,
    //                     "position": [x2,y2]
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
    //                 },
    //                 {
    //                     "id": a4,
    //                     "position": [x4,y4]
    //                 }...
    //             ]
    //         }...
    //     ]
    // }


    private bool SerialiseV1(Stream s, List<SimulationSnapshot> snapshots)
    {
        var payload = new
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
                        }
                    })
            })
        };
        
        JsonSerializer.Serialize(s, payload);
        
        return true;
    }
}