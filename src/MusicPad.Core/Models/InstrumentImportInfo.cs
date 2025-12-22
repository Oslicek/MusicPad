namespace MusicPad.Core.Models;

/// <summary>
/// Information about an instrument to import from an SFZ file.
/// </summary>
public class InstrumentImportInfo
{
    /// <summary>
    /// Display name for the imported instrument.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Voicing type for the imported instrument.
    /// </summary>
    public VoicingType Voicing { get; set; } = VoicingType.Polyphonic;
    
    /// <summary>
    /// Pitch type for the imported instrument.
    /// </summary>
    public PitchType PitchType { get; set; } = PitchType.Pitched;
    
    /// <summary>
    /// Index of the instrument within the SFZ file (for multi-instrument SFZ files).
    /// </summary>
    public int InstrumentIndex { get; set; }
}

/// <summary>
/// Information about an instrument found in an SFZ file during analysis.
/// </summary>
public class SfzInstrumentInfo
{
    /// <summary>
    /// Suggested display name based on SFZ metadata.
    /// </summary>
    public string SuggestedName { get; set; } = string.Empty;
    
    /// <summary>
    /// Index of this instrument within the SFZ file.
    /// </summary>
    public int Index { get; set; }
    
    /// <summary>
    /// Number of regions/samples in this instrument.
    /// </summary>
    public int RegionCount { get; set; }
    
    /// <summary>
    /// Note range of this instrument.
    /// </summary>
    public string NoteRange { get; set; } = string.Empty;
}








