namespace MusicPad.Core.Recording;

/// <summary>
/// Types of events that can be recorded.
/// </summary>
public enum RecordedEventType
{
    /// <summary>Note pressed (pad touch start)</summary>
    NoteOn,
    /// <summary>Note released (pad touch end)</summary>
    NoteOff,
    /// <summary>Instrument changed</summary>
    InstrumentChange,
    /// <summary>Effect setting changed</summary>
    EffectChange
}

/// <summary>
/// A single recorded event with timestamp.
/// </summary>
public class RecordedEvent
{
    /// <summary>
    /// Time in milliseconds from the start of recording.
    /// </summary>
    public long TimestampMs { get; set; }
    
    /// <summary>
    /// Type of event.
    /// </summary>
    public RecordedEventType EventType { get; set; }
    
    /// <summary>
    /// MIDI note number (for NoteOn/NoteOff events).
    /// </summary>
    public int MidiNote { get; set; }
    
    /// <summary>
    /// Velocity (0-127, for NoteOn events).
    /// </summary>
    public int Velocity { get; set; } = 100;
    
    /// <summary>
    /// Instrument ID (for InstrumentChange events).
    /// </summary>
    public string? InstrumentId { get; set; }
    
    /// <summary>
    /// Effect settings JSON (for EffectChange events).
    /// </summary>
    public string? EffectData { get; set; }
}







