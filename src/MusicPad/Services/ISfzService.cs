using MusicPad.Core.Sfz;

namespace MusicPad.Services;

/// <summary>
/// Service interface for SFZ instrument playback.
/// </summary>
public interface ISfzService
{
    /// <summary>
    /// Gets the list of available instruments.
    /// </summary>
    IReadOnlyList<string> AvailableInstruments { get; }
    
    /// <summary>
    /// Gets the currently loaded instrument name.
    /// </summary>
    string? CurrentInstrumentName { get; }
    
    /// <summary>
    /// Loads an SFZ instrument by name.
    /// </summary>
    Task LoadInstrumentAsync(string instrumentName);
    
    /// <summary>
    /// Plays a note (triggers note-on with middle key of instrument).
    /// </summary>
    void PlayNote();
    
    /// <summary>
    /// Stops the currently playing note.
    /// </summary>
    void StopNote();
}

