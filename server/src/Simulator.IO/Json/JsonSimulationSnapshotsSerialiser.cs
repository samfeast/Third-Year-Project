using System.Text.Json;
using Simulator.Core;
using Simulator.IO.Utils;

namespace Simulator.IO.Json;

internal class JsonSimulationSnapshotsSerialiser : ISerialiser<List<SimulationSnapshot>>
{
    public bool CanWrite(DataFormat format, DataType type, int version)
    {
        return format == DataFormat.JSON && type == DataType.SimulationSnapshots && version is >= 1 and <= 2;
    }
    
    public byte[] Serialise(List<SimulationSnapshot> snapshots, int version)
    {
        return version switch
        {
            1 => SerialiseV1(snapshots),
            2 => SerialiseV2(snapshots),
            _ => throw new NotImplementedException(
                $"JSON simulation snapshots serialiser does not implement version {version}")
        };
    }
    
    private byte[] SerialiseV1(List<SimulationSnapshot> snapshots)
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
        
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }

    private byte[] SerialiseV2(List<SimulationSnapshot> snapshots)
    {
        var data = new
        {
            type = "snapshots",
            version = 2,
            numSnapshots = snapshots.Count,
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
        
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}