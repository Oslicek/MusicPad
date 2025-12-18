using System.Text.Json.Serialization;

namespace MusicPad.Core.Models;

/// <summary>
/// Configuration for bundled instruments.
/// </summary>
public class InstrumentsConfig
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("instruments")]
    public List<InstrumentEntry> Instruments { get; set; } = new();
}

/// <summary>
/// Entry for a single instrument in the config.
/// </summary>
public class InstrumentEntry
{
    [JsonPropertyName("folder")]
    public string Folder { get; set; } = "";
    
    [JsonPropertyName("sfzFile")]
    public string SfzFile { get; set; } = "";
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = "";
}

