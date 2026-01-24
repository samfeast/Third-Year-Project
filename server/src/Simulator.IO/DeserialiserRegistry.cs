using Simulator.Core.Geometry.Primitives;
using Simulator.IO.Json;
using Simulator.IO.Utils;

namespace Simulator.IO;

public class DeserialiserRegistry<T>(IEnumerable<IDeserialiser<T>> deserializers)
{
    private readonly List<IDeserialiser<T>> _deserialisers = deserializers.ToList();
    
    public T Load(string path)
    {
        using var stream = File.OpenRead(path);
        var probe = DataProbe.FromStream(stream);

        // Get the first deserialiser that is able to read the file
        var deserialiser = _deserialisers.FirstOrDefault(d => d.CanRead(probe));
        if (deserialiser == null)
            throw new UnsupportedFormatException(path);

        stream.Position = 0;
        return deserialiser.Deserialise(stream);
    }
}

public static class DeserialiserRegistryFactory
{
    // Get a deserialiser registry containing all the deserialisers of type T
    public static DeserialiserRegistry<T> Default<T>()
    {
        if (typeof(T) == typeof(InputGeometry))
        {
            return new DeserialiserRegistry<T>([
                (IDeserialiser<T>) new JsonGeometryDeserialiser()
            ]);
        }

        throw new NotSupportedException($"No default registry for type {typeof(T).Name}");
    }
}
