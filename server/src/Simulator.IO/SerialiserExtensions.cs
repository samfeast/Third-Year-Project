using Simulator.IO.Utils;

namespace Simulator.IO;

public static class SerialiserExtensions
{
    public static byte[] Serialise<T>(this T data, DataFormat format, int version)
    {
        return SerialiserRegistryFactory
            .Default<T>()
            .Serialise(format, data, version);
    }

    public static void Save<T>(this T data, string path, int version)
    {
        SerialiserRegistryFactory
            .Default<T>()
            .Save(path, data, version);
    }
}