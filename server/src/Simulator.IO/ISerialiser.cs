using Simulator.IO.Utils;

namespace Simulator.IO;

public interface ISerialiser<T>
{
    public bool CanWrite(DataFormat format, DataType type, int version);
    public bool Serialise(Stream stream, T data, int version);
}