using System.Diagnostics;

namespace MusicPad.Core.Recording;

/// <summary>
/// Manages an active recording session, capturing events with timestamps.
/// </summary>
public class RecordingSession
{
    private readonly Stopwatch _stopwatch = new();
    private readonly List<RecordedEvent> _events = new();
    private readonly object _lock = new();
    
    private string? _currentInstrumentId;
    private bool _isRecording;
    
    /// <summary>
    /// Whether recording is currently active.
    /// </summary>
    public bool IsRecording => _isRecording;
    
    /// <summary>
    /// Elapsed time in milliseconds since recording started.
    /// </summary>
    public long ElapsedMs => _stopwatch.ElapsedMilliseconds;
    
    /// <summary>
    /// Number of events recorded so far.
    /// </summary>
    public int EventCount
    {
        get
        {
            lock (_lock)
            {
                return _events.Count;
            }
        }
    }
    
    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="initialInstrumentId">The instrument selected when recording starts.</param>
    public void Start(string? initialInstrumentId = null)
    {
        lock (_lock)
        {
            _events.Clear();
            _currentInstrumentId = initialInstrumentId;
            _isRecording = true;
            _stopwatch.Restart();
        }
    }
    
    /// <summary>
    /// Stops recording and returns the recorded events.
    /// </summary>
    public (List<RecordedEvent> Events, long DurationMs) Stop()
    {
        lock (_lock)
        {
            _stopwatch.Stop();
            _isRecording = false;
            var duration = _stopwatch.ElapsedMilliseconds;
            var events = new List<RecordedEvent>(_events);
            return (events, duration);
        }
    }
    
    /// <summary>
    /// Records a note on event (pad touched).
    /// </summary>
    public void RecordNoteOn(int midiNote, int velocity = 100)
    {
        if (!_isRecording) return;
        
        lock (_lock)
        {
            _events.Add(new RecordedEvent
            {
                TimestampMs = _stopwatch.ElapsedMilliseconds,
                EventType = RecordedEventType.NoteOn,
                MidiNote = midiNote,
                Velocity = velocity
            });
        }
    }
    
    /// <summary>
    /// Records a note off event (pad released).
    /// </summary>
    public void RecordNoteOff(int midiNote)
    {
        if (!_isRecording) return;
        
        lock (_lock)
        {
            _events.Add(new RecordedEvent
            {
                TimestampMs = _stopwatch.ElapsedMilliseconds,
                EventType = RecordedEventType.NoteOff,
                MidiNote = midiNote
            });
        }
    }
    
    /// <summary>
    /// Records an instrument change.
    /// </summary>
    public void RecordInstrumentChange(string instrumentId)
    {
        if (!_isRecording) return;
        if (instrumentId == _currentInstrumentId) return;
        
        lock (_lock)
        {
            _currentInstrumentId = instrumentId;
            _events.Add(new RecordedEvent
            {
                TimestampMs = _stopwatch.ElapsedMilliseconds,
                EventType = RecordedEventType.InstrumentChange,
                InstrumentId = instrumentId
            });
        }
    }
    
    /// <summary>
    /// Records an effect settings change.
    /// </summary>
    public void RecordEffectChange(string effectDataJson)
    {
        if (!_isRecording) return;
        
        lock (_lock)
        {
            _events.Add(new RecordedEvent
            {
                TimestampMs = _stopwatch.ElapsedMilliseconds,
                EventType = RecordedEventType.EffectChange,
                EffectData = effectDataJson
            });
        }
    }
}



