using Simulator.IO.Utils;

namespace Simulator.IO;

public interface ISerialiser<T>
{
    public bool CanWrite(DataFormat format, DataType type, int version);
    public string Serialise(T data, int version, Stream? stream = null);
}