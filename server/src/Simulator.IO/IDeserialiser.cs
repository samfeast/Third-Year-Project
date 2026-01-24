using Simulator.IO.Utils;

namespace Simulator.IO;

public interface IDeserialiser<T>
{
    public bool CanRead(DataProbe probe);
    public T Deserialise(Stream stream);
}