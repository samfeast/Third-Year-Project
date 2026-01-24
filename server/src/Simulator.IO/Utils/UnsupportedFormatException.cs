namespace Simulator.IO.Utils;

public class UnsupportedFormatException : Exception
{
    public UnsupportedFormatException() { }
    public UnsupportedFormatException(string message) : base(message) { }
    public UnsupportedFormatException(string message, Exception inner) : base(message, inner) { }
}