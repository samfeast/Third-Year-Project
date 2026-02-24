using Simulator.Core;
using Simulator.Core.Geometry;
using Simulator.Core.Geometry.Utils;
using Simulator.IO.Json;
using Simulator.IO.Utils;

namespace Simulator.IO;

public class SerialiserRegistry<T>(IEnumerable<ISerialiser<T>> serialisers)
{
    private readonly List<ISerialiser<T>> _serialisers = serialisers.ToList();
    
    public bool Save(string path, T data, int version)
    {
        DataFormat format = Path.GetExtension(path)?.ToLowerInvariant() switch
        {
            ".json" => DataFormat.JSON,
            _ => throw new UnsupportedFormatException($"Unknown file extension {path}")
        };

        DataType type = typeof(T) switch
        {
            var t when t == typeof(InputGeometry) => DataType.Geometry,
            var t when t == typeof(NavMesh) => DataType.Mesh,
            var t when t == typeof(List<SimulationSnapshot>) => DataType.SimulationSnapshots,
            var t when t == typeof(SimulationSnapshot) => DataType.SimulationSnapshot,
            _ => throw new NotSupportedException($"Unknown data type {typeof(T).Name}")
        };

        // Get the first serialiser that is able to read the file
        var serialiser = _serialisers.FirstOrDefault(s => s.CanWrite(format, type, version));
        if (serialiser == null)
            throw new UnsupportedFormatException($"No serialiser for type={type}, version={version}, format={format}");

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        serialiser.Serialise(data, version, stream);
        return true;
    }
}

public static class SerialiserRegistryFactory
{
    // Get a SerialiserRegistry containing all the serialisers of type T
    public static SerialiserRegistry<T> Default<T>()
    {
        if (typeof(T) == typeof(InputGeometry))
        {
            return new SerialiserRegistry<T>([
                (ISerialiser<T>) new JsonGeometrySerialiser(),
            ]);
        }
        if (typeof(T) == typeof(NavMesh))
        {
            return new SerialiserRegistry<T>([
                (ISerialiser<T>) new JsonMeshSerialiser()
            ]);
        }
        if (typeof(T) == typeof(List<SimulationSnapshot>))
        {
            return new SerialiserRegistry<T>([
                (ISerialiser<T>) new JsonSimulationSnapshotsSerialiser()
            ]);
        }
        if (typeof(T) == typeof(SimulationSnapshot))
        {
            return new SerialiserRegistry<T>([
                (ISerialiser<T>) new JsonSimulationSnapshotSerialiser()
            ]);
        }

        throw new NotSupportedException($"No default serialiser registry for type {typeof(T).Name}");
    }
}