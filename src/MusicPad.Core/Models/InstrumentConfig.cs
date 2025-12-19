using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MusicPad.Core.Models;

/// <summary>
/// Voicing type for an instrument.
/// </summary>
public enum VoicingType
{
    /// <summary>Multiple notes can play simultaneously.</summary>
    Polyphonic,
    
    /// <summary>Only one note can play at a time.</summary>
    Monophonic
}

/// <summary>
/// Pitch type for an instrument.
/// </summary>
public enum PitchType
{
    /// <summary>Instrument plays pitched notes (e.g., piano, violin).</summary>
    Pitched,
    
    /// <summary>Instrument plays unpitched sounds (e.g., drums).</summary>
    Unpitched
}

/// <summary>
/// Configuration for an instrument including display name, SFZ path, and playback settings.
/// </summary>
public class InstrumentConfig
{
    private static readonly Regex InvalidFileNameChars = new(@"[<>:""/\\|?*]", RegexOptions.Compiled);
    
    /// <summary>
    /// Schema version for future migrations.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// Display name shown in the UI.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Relative path to the SFZ file (e.g., "Petrof_sf2/000_Grandioso.sfz").
    /// </summary>
    [JsonPropertyName("sfzPath")]
    public string SfzPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Voicing type: polyphonic or monophonic.
    /// </summary>
    [JsonPropertyName("voicing")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VoicingType Voicing { get; set; } = VoicingType.Polyphonic;
    
    /// <summary>
    /// Pitch type: pitched or unpitched.
    /// </summary>
    [JsonPropertyName("pitchType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PitchType PitchType { get; set; } = PitchType.Pitched;
    
    /// <summary>
    /// Whether this instrument is bundled with the app (true) or user-imported (false).
    /// </summary>
    [JsonPropertyName("isBundled")]
    public bool IsBundled { get; set; } = true;
    
    /// <summary>
    /// Gets the filename for this config based on display name.
    /// Invalid filename characters are replaced with underscores.
    /// </summary>
    [JsonIgnore]
    public string FileName
    {
        get
        {
            var safeName = InvalidFileNameChars.Replace(DisplayName, "_");
            return $"{safeName}.json";
        }
    }
}
