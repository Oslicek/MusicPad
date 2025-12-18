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
    /// Gets the key range (min, max) of the current instrument.
    /// </summary>
    (int minKey, int maxKey) CurrentKeyRange { get; }
    
    /// <summary>
    /// Gets the currently loaded instrument with full metadata.
    /// </summary>
    SfzInstrument? CurrentInstrument { get; }
    
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
    
    /// <summary>
    /// Gets or sets whether the low-pass filter is enabled.
    /// </summary>
    bool LpfEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets the LPF cutoff (0.0 to 1.0).
    /// </summary>
    float LpfCutoff { get; set; }
    
    /// <summary>
    /// Gets or sets the LPF resonance (0.0 to 1.0).
    /// </summary>
    float LpfResonance { get; set; }
    
    /// <summary>
    /// Sets the EQ band gain (-1.0 to 1.0 for -12dB to +12dB).
    /// </summary>
    void SetEqBandGain(int band, float normalizedGain);
    
    /// <summary>
    /// Gets or sets whether the chorus is enabled.
    /// </summary>
    bool ChorusEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets the chorus depth (0.0 to 1.0).
    /// </summary>
    float ChorusDepth { get; set; }
    
    /// <summary>
    /// Gets or sets the chorus rate (0.0 to 1.0).
    /// </summary>
    float ChorusRate { get; set; }
    
    /// <summary>
    /// Gets or sets whether the delay is enabled.
    /// </summary>
    bool DelayEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets the delay time (0.0 to 1.0).
    /// </summary>
    float DelayTime { get; set; }
    
    /// <summary>
    /// Gets or sets the delay feedback (0.0 to 1.0).
    /// </summary>
    float DelayFeedback { get; set; }
    
    /// <summary>
    /// Gets or sets the delay level (0.0 to 1.0).
    /// </summary>
    float DelayLevel { get; set; }
    
    /// <summary>
    /// Gets or sets whether the reverb is enabled.
    /// </summary>
    bool ReverbEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets the reverb level/mix (0.0 to 1.0).
    /// </summary>
    float ReverbLevel { get; set; }
    
    /// <summary>
    /// Gets or sets the reverb type.
    /// </summary>
    MusicPad.Core.Models.ReverbType ReverbType { get; set; }
}

