using MusicPad.Core.Models;
using MusicPad.Core.NoteProcessing;
using MusicPad.Core.Recording;
using MusicPad.Core.Sfz;

namespace MusicPad.Services;

/// <summary>
/// Service interface for SFZ instrument playback.
/// </summary>
public interface ISfzService
{
    /// <summary>
    /// Gets or sets the current voicing mode (polyphonic or monophonic).
    /// </summary>
    VoicingType VoicingMode { get; set; }
    
    /// <summary>
    /// Gets the audio-thread arpeggiator for sample-accurate timing.
    /// </summary>
    AudioArpeggiator Arpeggiator { get; }
    
    /// <summary>
    /// Gets the list of available instruments.
    /// </summary>
    IReadOnlyList<string> AvailableInstruments { get; }
    
    /// <summary>
    /// Refreshes the list of available instruments.
    /// Call this after instrument order changes or new instruments are added/removed.
    /// </summary>
    void RefreshInstruments();
    
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
    /// Gets the unique MIDI notes from the current instrument (for unpitched instruments).
    /// </summary>
    IReadOnlyList<int> CurrentUniqueMidiNotes { get; }
    
    /// <summary>
    /// Gets the label for a specific MIDI note in the current instrument.
    /// For unpitched instruments, this returns the sample/region name.
    /// </summary>
    string GetNoteLabel(int midiNote);
    
    /// <summary>
    /// Loads an SFZ instrument by name.
    /// </summary>
    Task LoadInstrumentAsync(string instrumentName);
    
    /// <summary>
    /// Plays a specific MIDI note with default velocity.
    /// </summary>
    void NoteOn(int midiNote);
    
    /// <summary>
    /// Plays a specific MIDI note with specified velocity.
    /// </summary>
    void NoteOn(int midiNote, int velocity);
    
    /// <summary>
    /// Stops a specific MIDI note.
    /// </summary>
    void NoteOff(int midiNote);
    
    /// <summary>
    /// Stops all playing notes.
    /// </summary>
    void StopAll();
    
    /// <summary>
    /// Mutes all playing notes with a very short release to avoid clicks.
    /// Used by the mute button on keyboards/pads.
    /// </summary>
    void Mute();
    
    /// <summary>
    /// Gets the current envelope level for a note (0.0 = silent, 1.0 = full).
    /// Returns 0 if the note is not currently playing.
    /// Used for visual feedback that follows the sound's amplitude.
    /// </summary>
    float GetNoteEnvelopeLevel(int midiNote);
    
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
    
    // ========== Audio-Thread Playback ==========
    
    /// <summary>
    /// Loads recorded events for audio-thread playback.
    /// </summary>
    void LoadPlaybackEvents(IReadOnlyList<RecordedEvent> events);
    
    /// <summary>
    /// Starts audio-thread playback.
    /// </summary>
    void StartPlayback();
    
    /// <summary>
    /// Stops audio-thread playback.
    /// </summary>
    void StopPlayback();
    
    /// <summary>
    /// Whether audio-thread playback is currently active.
    /// </summary>
    bool IsPlaybackActive { get; }
    
    /// <summary>
    /// Event fired when playback state changes (started/stopped).
    /// </summary>
    event EventHandler<bool>? PlaybackStateChanged;
    
    /// <summary>
    /// Event fired when a UI event (instrument/effect change) needs to be processed.
    /// These are dispatched from the audio thread and need UI thread handling.
    /// </summary>
    event EventHandler<RecordedEvent>? PlaybackUiEvent;
    
    // ========== Offline Rendering ==========
    
    /// <summary>
    /// Generates audio samples directly (for offline rendering).
    /// Includes all effects processing (LPF, EQ, Chorus, Delay, Reverb).
    /// </summary>
    /// <param name="buffer">Buffer to fill with samples (mono).</param>
    void GenerateSamples(float[] buffer);
    
    /// <summary>
    /// Resets the player and all effects state (for clean offline rendering).
    /// </summary>
    void ResetState();
}

