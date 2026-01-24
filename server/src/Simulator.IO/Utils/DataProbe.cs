using System.Text.Json;

namespace Simulator.IO.Utils;

// Class that inspects a data stream and tries to determine serialisation type, version, and format
public class DataProbe
{
    public enum DataFormat
    {
        JSON
    }
    
    public string? Type { get; }
    public int? Version { get; }
    public DataFormat Format { get; }
    
    private DataProbe(string? type, int? version, DataFormat format)
    {
        Type = type;
        Version = version;
        Format = format;
    }
    
    public static DataProbe FromStream(Stream s)
    {
        // First 8 bytes of stream - relies on the format header being no longer than this
        Span<byte> header = stackalloc byte[8];
        // Fill header with exactly the first 8 bytes
        s.ReadExactly(header);
        // Rewind the pointer to the start of the stream
        s.Position = 0;
        
        // If the header starts with the UTF8 character '{', we assume it is JSON
        if (header.StartsWith("{"u8))
        {
            return ParseJsonProbe(s);
        }
        
        throw new UnsupportedFormatException();
    }
    
    // Parse stream as JSON to create probe
    private static DataProbe ParseJsonProbe(Stream s)
    {
        // Parse stream as JSON, but don't construct C# objects for fields
        using var doc = JsonDocument.Parse(
            s,
            new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
            });

        // Verify the outermost element is a JSON object
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new UnsupportedFormatException("JSON root must be an object.");

        // Parse the type and version fields if they are present
        string? type = null;
        int? version = null;
        
        if (root.TryGetProperty("type", out var typeProp) &&
            typeProp.ValueKind == JsonValueKind.String)
        {
            type = typeProp.GetString();
        }

        if (root.TryGetProperty("version", out var versionProp) &&
            versionProp.ValueKind == JsonValueKind.Number)
        {
            version = versionProp.GetInt32();
        }

        return new DataProbe(
            type,
            version,
            DataFormat.JSON
        );
    }
}