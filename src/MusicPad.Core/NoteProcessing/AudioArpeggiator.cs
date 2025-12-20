using MusicPad.Core.Models;

namespace MusicPad.Core.NoteProcessing;

/// <summary>
/// Event types for arpeggiator audio events.
/// </summary>
public enum ArpEventType
{
    NoteOn,
    NoteOff
}

/// <summary>
/// An arpeggiator event to be processed by the audio engine.
/// </summary>
public readonly struct ArpEvent
{
    public int MidiNote { get; init; }
    public ArpEventType EventType { get; init; }
    public int Velocity { get; init; }
}

/// <summary>
/// Audio-thread arpeggiator with sample-accurate timing.
/// This class is designed to be called from the audio playback loop
/// for precise, jitter-free timing.
/// </summary>
public class AudioArpeggiator
{
    private readonly int _sampleRate;
    private readonly SortedSet<int> _notes = new();
    private readonly Random _random = new();
    private readonly object _lock = new();
    
    private int _currentIndex = 0;
    private bool _goingUp = true;
    private long _sampleCounter = 0;
    private long _nextTriggerSample = 0;
    private int? _currentPlayingNote = null;
    private int _intervalSamples;
    private bool _needsImmediateTrigger = false;
    private bool _wasEnabled = false;

    // Rate maps to BPM: 0 = 60 BPM (1000ms), 1 = 480 BPM (125ms)
    private const float MinIntervalMs = 125f;  // 480 BPM
    private const float MaxIntervalMs = 500f;  // 120 BPM

    public AudioArpeggiator(int sampleRate)
    {
        _sampleRate = sampleRate;
        _intervalSamples = MsToSamples(MaxIntervalMs * 0.5f + MinIntervalMs * 0.5f);
    }

    /// <summary>
    /// Gets or sets whether the arpeggiator is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the arpeggio pattern.
    /// </summary>
    public ArpPattern Pattern { get; set; } = ArpPattern.Up;

    /// <summary>
    /// Gets the currently held notes.
    /// </summary>
    public IReadOnlyCollection<int> ActiveNotes
    {
        get
        {
            lock (_lock)
            {
                return _notes.ToList();
            }
        }
    }

    /// <summary>
    /// Sets the interval between notes in milliseconds.
    /// </summary>
    public void SetIntervalMs(float ms)
    {
        lock (_lock)
        {
            _intervalSamples = MsToSamples(ms);
        }
    }

    /// <summary>
    /// Sets the interval using the normalized rate (0-1).
    /// </summary>
    public void SetRate(float rate)
    {
        float intervalMs = MaxIntervalMs - rate * (MaxIntervalMs - MinIntervalMs);
        SetIntervalMs(intervalMs);
    }

    /// <summary>
    /// Adds a note to the arpeggiator.
    /// </summary>
    public void AddNote(int midiNote)
    {
        lock (_lock)
        {
            bool wasEmpty = _notes.Count == 0;
            _notes.Add(midiNote);
            
            // If this is the first note, trigger immediately
            if (wasEmpty && IsEnabled)
            {
                _needsImmediateTrigger = true;
            }
        }
    }

    /// <summary>
    /// Removes a note from the arpeggiator.
    /// </summary>
    public void RemoveNote(int midiNote)
    {
        lock (_lock)
        {
            _notes.Remove(midiNote);
            
            // Adjust index if needed
            if (_notes.Count > 0 && _currentIndex >= _notes.Count)
            {
                _currentIndex = 0;
            }
        }
    }

    /// <summary>
    /// Resets the arpeggiator state and returns any pending note-off events.
    /// </summary>
    public List<ArpEvent> Reset()
    {
        var events = new List<ArpEvent>();
        
        lock (_lock)
        {
            if (_currentPlayingNote.HasValue)
            {
                events.Add(new ArpEvent
                {
                    MidiNote = _currentPlayingNote.Value,
                    EventType = ArpEventType.NoteOff,
                    Velocity = 0
                });
                _currentPlayingNote = null;
            }
            
            _notes.Clear();
            _currentIndex = 0;
            _goingUp = true;
            _sampleCounter = 0;
            _nextTriggerSample = 0;
            _needsImmediateTrigger = false;
        }
        
        return events;
    }

    /// <summary>
    /// Processes an audio buffer and returns any arp events that should occur.
    /// Call this from the audio thread for each buffer.
    /// </summary>
    public List<ArpEvent> ProcessBuffer(int bufferSamples)
    {
        var events = new List<ArpEvent>();
        
        lock (_lock)
        {
            // Handle enable/disable state changes
            if (!IsEnabled && _wasEnabled)
            {
                // Just disabled - stop current note
                if (_currentPlayingNote.HasValue)
                {
                    events.Add(new ArpEvent
                    {
                        MidiNote = _currentPlayingNote.Value,
                        EventType = ArpEventType.NoteOff,
                        Velocity = 0
                    });
                    _currentPlayingNote = null;
                }
                _wasEnabled = false;
                _sampleCounter += bufferSamples;
                return events;
            }
            
            if (IsEnabled && !_wasEnabled)
            {
                // Just enabled - trigger immediately
                _wasEnabled = true;
                _needsImmediateTrigger = true;
                _nextTriggerSample = _sampleCounter;
            }
            
            if (!IsEnabled || _notes.Count == 0)
            {
                // Not active - stop any playing note
                if (_currentPlayingNote.HasValue)
                {
                    events.Add(new ArpEvent
                    {
                        MidiNote = _currentPlayingNote.Value,
                        EventType = ArpEventType.NoteOff,
                        Velocity = 0
                    });
                    _currentPlayingNote = null;
                }
                _sampleCounter += bufferSamples;
                return events;
            }
            
            // Check if we need to trigger a note
            bool shouldTrigger = _needsImmediateTrigger || _sampleCounter >= _nextTriggerSample;
            
            if (shouldTrigger)
            {
                _needsImmediateTrigger = false;
                
                // Stop previous note
                if (_currentPlayingNote.HasValue)
                {
                    events.Add(new ArpEvent
                    {
                        MidiNote = _currentPlayingNote.Value,
                        EventType = ArpEventType.NoteOff,
                        Velocity = 0
                    });
                }
                
                // Get next note
                int nextNote = GetNextNote();
                
                events.Add(new ArpEvent
                {
                    MidiNote = nextNote,
                    EventType = ArpEventType.NoteOn,
                    Velocity = 100
                });
                
                _currentPlayingNote = nextNote;
                _nextTriggerSample = _sampleCounter + _intervalSamples;
            }
            
            _sampleCounter += bufferSamples;
        }
        
        return events;
    }

    private int GetNextNote()
    {
        // Assumes lock is held
        var notesList = _notes.ToList();
        
        if (notesList.Count == 0)
            return 60; // Fallback
        
        int note;
        
        switch (Pattern)
        {
            case ArpPattern.Up:
                note = notesList[_currentIndex];
                _currentIndex = (_currentIndex + 1) % notesList.Count;
                break;

            case ArpPattern.Down:
                int downIndex = notesList.Count - 1 - _currentIndex;
                note = notesList[downIndex];
                _currentIndex = (_currentIndex + 1) % notesList.Count;
                break;

            case ArpPattern.UpDown:
                note = notesList[_currentIndex];
                if (_goingUp)
                {
                    _currentIndex++;
                    if (_currentIndex >= notesList.Count)
                    {
                        _currentIndex = Math.Max(0, notesList.Count - 2);
                        _goingUp = false;
                    }
                }
                else
                {
                    _currentIndex--;
                    if (_currentIndex < 0)
                    {
                        _currentIndex = Math.Min(1, notesList.Count - 1);
                        _goingUp = true;
                    }
                }
                break;

            case ArpPattern.Random:
            default:
                note = notesList[_random.Next(notesList.Count)];
                break;
        }

        return note;
    }

    private int MsToSamples(float ms)
    {
        return Math.Max(1, (int)(_sampleRate * ms / 1000f));
    }
}

