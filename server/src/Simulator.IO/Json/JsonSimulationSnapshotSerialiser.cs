using System.Text.Json;
using Simulator.Core;
using Simulator.IO.Utils;

namespace Simulator.IO.Json;

public class JsonSimulationSnapshotSerialiser : ISerialiser<SimulationSnapshot>
{
    public bool CanWrite(DataFormat format, DataType type, int version)
    {
        return format == DataFormat.JSON && type == DataType.SimulationSnapshot && version is >= 1 and <= 1;
    }
    
    public string Serialise(SimulationSnapshot snapshot, int version, Stream? s = null)
    {
        return version switch
        {
            1 => SerialiseV1(snapshot, s),
            _ => throw new NotImplementedException($"JSON simulation snapshot serialiser does not implement version {version}")
        };
    }
    
    private string SerialiseV1(SimulationSnapshot snapshot, Stream? s = null)
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
        
        
        if (s == null)
        {
            return JsonSerializer.Serialize(data);
        }
        
        JsonSerializer.Serialize(s, data);
        return string.Empty;
    }
}