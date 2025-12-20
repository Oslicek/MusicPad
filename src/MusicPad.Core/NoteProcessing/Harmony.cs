using MusicPad.Core.Models;

namespace MusicPad.Core.NoteProcessing;

/// <summary>
/// Auto harmony processor that generates additional notes from a single input note.
/// </summary>
public class Harmony
{
    private readonly Dictionary<int, int[]> _activeNotes = new();
    
    public bool IsEnabled { get; set; }
    public HarmonyType Type { get; set; } = HarmonyType.Major;

    /// <summary>
    /// Processes a note on event and returns all notes to play (including harmonies).
    /// </summary>
    public int[] ProcessNoteOn(int midiNote)
    {
        if (!IsEnabled)
        {
            _activeNotes[midiNote] = new[] { midiNote };
            return new[] { midiNote };
        }

        var intervals = GetIntervals(Type);
        var notes = new List<int> { midiNote };

        foreach (var interval in intervals)
        {
            int harmonyNote = midiNote + interval;
            if (harmonyNote <= 127)
            {
                notes.Add(harmonyNote);
            }
        }

        var result = notes.ToArray();
        _activeNotes[midiNote] = result;
        return result;
    }

    /// <summary>
    /// Processes a note off event and returns all notes to stop.
    /// </summary>
    public int[] ProcessNoteOff(int midiNote)
    {
        if (_activeNotes.TryGetValue(midiNote, out var notes))
        {
            _activeNotes.Remove(midiNote);
            return notes;
        }

        return new[] { midiNote };
    }

    /// <summary>
    /// Clears all active note mappings.
    /// </summary>
    public void Reset()
    {
        _activeNotes.Clear();
    }
    
    /// <summary>
    /// Gets the root notes (originally pressed keys) that are currently active.
    /// </summary>
    public IReadOnlyList<int> GetActiveRootNotes()
    {
        return _activeNotes.Keys.ToList();
    }
    
    /// <summary>
    /// Reharmonizes all active notes with a new harmony type.
    /// Returns the notes to remove and notes to add for updating the arpeggiator.
    /// Also updates the internal state to track the new harmonized notes.
    /// </summary>
    public (int[] notesToRemove, int[] notesToAdd) ReharmonizeActiveNotes(HarmonyType newType)
    {
        var toRemove = new List<int>();
        var toAdd = new List<int>();
        
        var rootNotes = _activeNotes.Keys.ToList();
        var newIntervals = GetIntervals(newType);
        
        foreach (var rootNote in rootNotes)
        {
            var oldNotes = _activeNotes[rootNote];
            
            // Calculate new notes for this root
            var newNotes = new List<int> { rootNote };
            foreach (var interval in newIntervals)
            {
                int harmonyNote = rootNote + interval;
                if (harmonyNote <= 127)
                {
                    newNotes.Add(harmonyNote);
                }
            }
            
            // Find notes to remove (in old but not in new)
            foreach (var note in oldNotes)
            {
                if (!newNotes.Contains(note))
                {
                    toRemove.Add(note);
                }
            }
            
            // Find notes to add (in new but not in old)
            foreach (var note in newNotes)
            {
                if (!oldNotes.Contains(note))
                {
                    toAdd.Add(note);
                }
            }
            
            // Update internal state
            _activeNotes[rootNote] = newNotes.ToArray();
        }
        
        // Update the harmony type
        Type = newType;
        
        return (toRemove.ToArray(), toAdd.ToArray());
    }

    private static int[] GetIntervals(HarmonyType type)
    {
        return type switch
        {
            HarmonyType.Octave => new[] { 12 },           // Octave
            HarmonyType.Fifth => new[] { 7 },             // Perfect 5th
            HarmonyType.Major => new[] { 4, 7 },          // Major 3rd + Perfect 5th
            HarmonyType.Minor => new[] { 3, 7 },          // Minor 3rd + Perfect 5th
            _ => Array.Empty<int>()
        };
    }
}

