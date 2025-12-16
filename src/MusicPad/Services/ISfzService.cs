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
    /// Gets the key range (min, max) of the current instrument.
    /// </summary>
    (int minKey, int maxKey) CurrentKeyRange { get; }
    
    /// <summary>
    /// Loads an SFZ instrument by name.
    /// </summary>
    Task LoadInstrumentAsync(string instrumentName);
    
    /// <summary>
    /// Plays a specific MIDI note.
    /// </summary>
    void NoteOn(int midiNote);
    
    /// <summary>
    /// Stops a specific MIDI note.
    /// </summary>
    void NoteOff(int midiNote);
    
    /// <summary>
    /// Stops all playing notes.
    /// </summary>
    void StopAll();
    
    /// <summary>
    /// Gets or sets the master volume (0.0 to 1.0).
    /// </summary>
    float Volume { get; set; }
}

