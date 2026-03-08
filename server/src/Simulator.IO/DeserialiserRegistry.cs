using System.Text.Json;
using Simulator.Core.Geometry.Utils;
using Simulator.IO.Json;
using Simulator.IO.Utils;

namespace Simulator.IO;

public class DeserialiserRegistry<T>(IEnumerable<IDeserialiser<T>> deserialisers)
{
    private readonly List<IDeserialiser<T>> _deserialisers = deserialisers.ToList();
    
    internal T Load(Stream stream)
    {
        var deserialiser = GetDeserialiser(stream);
        return deserialiser.Deserialise(stream);
    }
    
    internal T Load(string path)
    {
        using var stream = File.OpenRead(path);
        return Load(stream);
    }

    internal T Load(JsonElement element)
    {
        using var stream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(element));
        return Load(stream);
    }

    private IDeserialiser<T> GetDeserialiser(Stream stream)
    {
        stream.Position = 0;
        
        var probe = DataProbe.FromStream(stream);
        
        // Get the first deserialiser that is able to read the file
        var deserialiser = _deserialisers.FirstOrDefault(d => d.CanRead(probe));
        if (deserialiser == null)
            throw new UnsupportedFormatException("Could not read data with any deserialiser");

        stream.Position = 0;
        return deserialiser;
    }
}

public static class DeserialiserRegistryFactory
{
    // Get a DeserialiserRegistry containing all the deserialisers of type T
    public static DeserialiserRegistry<T> Default<T>()
    {
        if (typeof(T) == typeof(InputGeometry))
        {
            return new DeserialiserRegistry<T>([
                (IDeserialiser<T>) new JsonGeometryDeserialiser()
            ]);
        }

        throw new NotSupportedException($"No default deserialiser registry for type {typeof(T).Name}");
    }
}
