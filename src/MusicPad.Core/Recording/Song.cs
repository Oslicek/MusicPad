using System.Text.Json.Serialization;

namespace MusicPad.Core.Recording;

/// <summary>
/// Represents a recorded song with metadata and events.
/// </summary>
public class Song
{
    /// <summary>
    /// Unique identifier for the song.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    
    /// <summary>
    /// Display name of the song.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// When the recording was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Duration in milliseconds.
    /// </summary>
    [JsonPropertyName("durationMs")]
    public long DurationMs { get; set; }
    
    /// <summary>
    /// List of instruments used during recording.
    /// </summary>
    [JsonPropertyName("instruments")]
    public List<string> Instruments { get; set; } = new();
    
    /// <summary>
    /// List of effects that were enabled during recording.
    /// </summary>
    [JsonPropertyName("effects")]
    public List<string> Effects { get; set; } = new();
    
    /// <summary>
    /// Initial instrument ID at the start of recording.
    /// </summary>
    [JsonPropertyName("initialInstrumentId")]
    public string? InitialInstrumentId { get; set; }
    
    /// <summary>
    /// Initial effect settings at the start of recording (JSON).
    /// </summary>
    [JsonPropertyName("initialSettings")]
    public string? InitialSettings { get; set; }
    
    /// <summary>
    /// Generates a default name based on recording properties.
    /// Format: "YYYY-MM-DD_HHmm_Instrument_Duration"
    /// </summary>
    public static string GenerateName(DateTime dateTime, string instrumentName, long durationMs)
    {
        var duration = TimeSpan.FromMilliseconds(durationMs);
        var durationStr = duration.TotalMinutes >= 1 
            ? $"{(int)duration.TotalMinutes}m{duration.Seconds:D2}s"
            : $"{duration.Seconds}s";
        
        var sanitizedInstrument = SanitizeFileName(instrumentName);
        return $"{dateTime:yyyy-MM-dd_HHmm}_{sanitizedInstrument}_{durationStr}";
    }
    
    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        return sanitized.Replace(' ', '_').Substring(0, Math.Min(sanitized.Length, 20));
    }
}


