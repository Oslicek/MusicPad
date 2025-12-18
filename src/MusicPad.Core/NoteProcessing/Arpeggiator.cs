using MusicPad.Core.Models;

namespace MusicPad.Core.NoteProcessing;

/// <summary>
/// Arpeggiator that cycles through held notes in a pattern.
/// </summary>
public class Arpeggiator
{
    private readonly SortedSet<int> _notes = new();
    private readonly Random _random = new();
    private int _currentIndex = 0;
    private bool _goingUp = true;
    
    // Rate maps to BPM: 0 = 60 BPM (1000ms), 1 = 480 BPM (125ms)
    private const float MinIntervalMs = 125f;  // 480 BPM
    private const float MaxIntervalMs = 500f;  // 120 BPM

    public bool IsEnabled { get; set; }
    public float Rate { get; set; } = 0.5f;
    public ArpPattern Pattern { get; set; } = ArpPattern.Up;

    /// <summary>
    /// Gets the currently held notes (sorted).
    /// </summary>
    public IReadOnlyCollection<int> ActiveNotes => _notes;

    /// <summary>
    /// Adds a note to the arpeggiator.
    /// </summary>
    public void AddNote(int midiNote)
    {
        _notes.Add(midiNote);
    }

    /// <summary>
    /// Removes a note from the arpeggiator.
    /// </summary>
    public void RemoveNote(int midiNote)
    {
        _notes.Remove(midiNote);
        
        // Adjust index if needed
        if (_notes.Count > 0 && _currentIndex >= _notes.Count)
        {
            _currentIndex = 0;
        }
    }

    /// <summary>
    /// Gets the next note in the arpeggio sequence.
    /// Returns null if disabled or no notes are held.
    /// </summary>
    public int? GetNextNote()
    {
        if (!IsEnabled || _notes.Count == 0)
            return null;

        var notesList = _notes.ToList();
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

    /// <summary>
    /// Gets the interval between notes in milliseconds based on rate.
    /// </summary>
    public float GetIntervalMs()
    {
        // Exponential mapping for more musical feel
        return MaxIntervalMs - Rate * (MaxIntervalMs - MinIntervalMs);
    }

    /// <summary>
    /// Clears all notes and resets state.
    /// </summary>
    public void Reset()
    {
        _notes.Clear();
        _currentIndex = 0;
        _goingUp = true;
    }
}

