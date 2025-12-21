namespace MusicPad.Core.Recording;

/// <summary>
/// Audio-thread playback with sample-accurate timing.
/// Similar to AudioArpeggiator, this is designed to be called from the audio playback loop
/// for precise, jitter-free playback timing.
/// </summary>
public class AudioPlayback
{
    private readonly int _sampleRate;
    private readonly object _lock = new();
    
    private List<RecordedEvent>? _events;
    private int _eventIndex;
    private long _sampleCounter;
    private bool _isPlaying;
    
    // Events that need UI thread handling (instrument changes, effect changes)
    private readonly Queue<RecordedEvent> _pendingUiEvents = new();
    
    public AudioPlayback(int sampleRate)
    {
        _sampleRate = sampleRate;
    }
    
    /// <summary>
    /// Whether playback is currently active.
    /// </summary>
    public bool IsPlaying
    {
        get
        {
            lock (_lock)
            {
                return _isPlaying;
            }
        }
    }
    
    /// <summary>
    /// Loads events for playback. Must be called before StartPlayback.
    /// </summary>
    public void LoadEvents(IReadOnlyList<RecordedEvent> events)
    {
        lock (_lock)
        {
            _events = events.ToList();
            _eventIndex = 0;
            _sampleCounter = 0;
        }
    }
    
    /// <summary>
    /// Starts playback from the beginning.
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            if (_events == null || _events.Count == 0)
                return;
            
            _eventIndex = 0;
            _sampleCounter = 0;
            _isPlaying = true;
            _pendingUiEvents.Clear();
        }
    }
    
    /// <summary>
    /// Stops playback.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            _isPlaying = false;
        }
    }
    
    /// <summary>
    /// Processes a buffer of samples and returns note events that should be triggered.
    /// Called from the audio thread for sample-accurate timing.
    /// </summary>
    /// <param name="bufferSamples">Number of samples in the current buffer.</param>
    /// <returns>List of note events (NoteOn/NoteOff) to be processed.</returns>
    public List<PlaybackNoteEvent> ProcessBuffer(int bufferSamples)
    {
        var result = new List<PlaybackNoteEvent>();
        
        lock (_lock)
        {
            if (!_isPlaying || _events == null)
            {
                return result;
            }
            
            long bufferEndSample = _sampleCounter + bufferSamples;
            
            // Process all events that fall within this buffer
            while (_eventIndex < _events.Count)
            {
                var evt = _events[_eventIndex];
                long eventSample = MsToSamples(evt.TimestampMs);
                
                if (eventSample >= bufferEndSample)
                {
                    // This event is for a future buffer
                    break;
                }
                
                // Event is within this buffer - process it
                switch (evt.EventType)
                {
                    case RecordedEventType.NoteOn:
                        result.Add(new PlaybackNoteEvent
                        {
                            MidiNote = evt.MidiNote,
                            Velocity = evt.Velocity,
                            IsNoteOn = true
                        });
                        break;
                    
                    case RecordedEventType.NoteOff:
                        result.Add(new PlaybackNoteEvent
                        {
                            MidiNote = evt.MidiNote,
                            Velocity = 0,
                            IsNoteOn = false
                        });
                        break;
                    
                    case RecordedEventType.InstrumentChange:
                    case RecordedEventType.EffectChange:
                        // Queue for UI thread handling
                        _pendingUiEvents.Enqueue(evt);
                        break;
                }
                
                _eventIndex++;
            }
            
            _sampleCounter = bufferEndSample;
            
            // Check if we've reached the end
            if (_eventIndex >= _events.Count)
            {
                _isPlaying = false;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Gets any pending UI events (instrument/effect changes) that need to be
    /// dispatched to the UI thread. Call this after ProcessBuffer.
    /// </summary>
    public List<RecordedEvent> GetPendingUiEvents()
    {
        lock (_lock)
        {
            if (_pendingUiEvents.Count == 0)
                return new List<RecordedEvent>();
            
            var events = _pendingUiEvents.ToList();
            _pendingUiEvents.Clear();
            return events;
        }
    }
    
    private long MsToSamples(long ms)
    {
        return (ms * _sampleRate) / 1000;
    }
}

/// <summary>
/// A note event from playback.
/// </summary>
public readonly struct PlaybackNoteEvent
{
    public int MidiNote { get; init; }
    public int Velocity { get; init; }
    public bool IsNoteOn { get; init; }
}




