using System.Text.Json;

namespace Simulator.IO;

public static class DeserialiserExtensions
{
    public static T Deserialise<T>(this Stream stream)
    {
        return DeserialiserRegistryFactory
            .Default<T>()
            .Load(stream);
    }

    public static T Deserialise<T>(this string path)
    {
        return DeserialiserRegistryFactory
            .Default<T>()
            .Load(path);
    }

    public static T Deserialise<T>(this JsonElement element)
    {
        return DeserialiserRegistryFactory
            .Default<T>()
            .Load(element);
    }
}