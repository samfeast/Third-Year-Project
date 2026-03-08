using System.Text.Json;
using Simulator.Core;
using Simulator.IO.Utils;

namespace Simulator.IO.Json;

internal class JsonSimulationSnapshotSerialiser : ISerialiser<SimulationSnapshot>
{
    public bool CanWrite(DataFormat format, DataType type, int version)
    {
        return format == DataFormat.JSON && type == DataType.SimulationSnapshot && version is >= 1 and <= 1;
    }
    
    public byte[] Serialise(SimulationSnapshot snapshot, int version)
    {
        return version switch
        {
            1 => SerialiseV1(snapshot),
            _ => throw new NotImplementedException(
                $"JSON simulation snapshot serialiser does not implement version {version}")
        };
    }
    
    private byte[] SerialiseV1(SimulationSnapshot snapshot)
    {
        var data = new
        {
            version = 1,
            snapshot = new
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
            }
        };
        
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}