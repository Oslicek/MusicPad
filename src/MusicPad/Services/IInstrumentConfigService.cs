using MusicPad.Core.Models;

namespace MusicPad.Services;

/// <summary>
/// Service for managing instrument configurations including bundled and user-imported instruments.
/// </summary>
public interface IInstrumentConfigService
{
    /// <summary>
    /// Gets all instrument configurations in display order (user instruments first, then bundled).
    /// </summary>
    Task<List<InstrumentConfig>> GetAllInstrumentsAsync();
    
    /// <summary>
    /// Gets only user-imported instrument configurations.
    /// </summary>
    Task<List<InstrumentConfig>> GetUserInstrumentsAsync();
    
    /// <summary>
    /// Gets only bundled instrument configurations.
    /// </summary>
    Task<List<InstrumentConfig>> GetBundledInstrumentsAsync();
    
    /// <summary>
    /// Gets a specific instrument configuration by filename.
    /// </summary>
    Task<InstrumentConfig?> GetInstrumentAsync(string configFileName);
    
    /// <summary>
    /// Saves or updates an instrument configuration.
    /// </summary>
    Task SaveInstrumentAsync(InstrumentConfig config);
    
    /// <summary>
    /// Deletes a user-imported instrument (cannot delete bundled instruments).
    /// </summary>
    Task<bool> DeleteInstrumentAsync(string configFileName);
    
    /// <summary>
    /// Renames a user-imported instrument (changes display name only).
    /// Returns the new filename if rename caused file rename, null if only display name changed.
    /// </summary>
    Task<string?> RenameInstrumentAsync(string configFileName, string newDisplayName);
    
    /// <summary>
    /// Updates the order of all instruments.
    /// </summary>
    Task SaveOrderAsync(List<string> orderedFileNames);
    
    /// <summary>
    /// Imports an SFZ file and creates instrument configuration(s).
    /// Returns the list of created config file names.
    /// </summary>
    Task<List<string>> ImportSfzAsync(string sfzSourcePath, List<InstrumentImportInfo> instruments);
    
    /// <summary>
    /// Analyzes an SFZ file and returns the instruments it contains.
    /// </summary>
    Task<List<SfzInstrumentInfo>> AnalyzeSfzAsync(Stream sfzStream, string fileName);
}

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

