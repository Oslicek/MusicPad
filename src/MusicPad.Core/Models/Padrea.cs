namespace MusicPad.Core.Models;

/// <summary>
/// Represents a pad area configuration - defines how pads are arranged and what they play.
/// </summary>
public class Padrea
{
    /// <summary>
    /// Unique identifier for the padrea.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for the padrea.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description of the padrea.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Minimum MIDI note for the pad range (0-127).
    /// If null, uses the instrument's minimum.
    /// </summary>
    public int? MinNote { get; set; }
    
    /// <summary>
    /// Maximum MIDI note for the pad range (0-127).
    /// If null, uses the instrument's maximum.
    /// </summary>
    public int? MaxNote { get; set; }
    
    /// <summary>
    /// Number of columns (pads per row) in the pad grid.
    /// If null, calculated automatically.
    /// </summary>
    public int? Columns { get; set; }
    
    /// <summary>
    /// Number of rows per viewpage.
    /// If null, shows all available rows.
    /// </summary>
    public int? RowsPerViewpage { get; set; }
    
    /// <summary>
    /// The note filter type that determines which notes are shown.
    /// </summary>
    public NoteFilterType NoteFilter { get; set; } = NoteFilterType.Chromatic;
    
    /// <summary>
    /// Current viewpage index (0 = lowest notes).
    /// </summary>
    public int CurrentViewpage { get; set; }
    
    /// <summary>
    /// Color for normal (unpressed) pads in hex format.
    /// </summary>
    public string? PadColor { get; set; }
    
    /// <summary>
    /// Color for pressed pads in hex format.
    /// </summary>
    public string? PadPressedColor { get; set; }
    
    /// <summary>
    /// Alternate color for distinguishing notes (e.g., sharps in chromatic).
    /// </summary>
    public string? PadAltColor { get; set; }
    
    /// <summary>
    /// Alternate pressed color.
    /// </summary>
    public string? PadAltPressedColor { get; set; }

    public override string ToString() => Name;
    
    /// <summary>
    /// Gets the MIDI notes that pass through the filter for a given range.
    /// </summary>
    public List<int> GetFilteredNotes(int minNote, int maxNote)
    {
        var notes = new List<int>();
        for (int note = minNote; note <= maxNote; note++)
        {
            if (PassesFilter(note))
            {
                notes.Add(note);
            }
        }
        return notes;
    }
    
    /// <summary>
    /// Checks if a MIDI note passes the current filter.
    /// </summary>
    public bool PassesFilter(int midiNote)
    {
        int noteInOctave = midiNote % 12;
        
        return NoteFilter switch
        {
            NoteFilterType.Chromatic => true,
            NoteFilterType.PentatonicMajor => noteInOctave is 0 or 2 or 4 or 7 or 9, // C, D, E, G, A
            NoteFilterType.PentatonicMinor => noteInOctave is 0 or 3 or 5 or 7 or 10, // C, Eb, F, G, Bb
            NoteFilterType.WhiteKeys => noteInOctave is 0 or 2 or 4 or 5 or 7 or 9 or 11, // C, D, E, F, G, A, B
            NoteFilterType.BlackKeys => noteInOctave is 1 or 3 or 6 or 8 or 10, // C#, D#, F#, G#, A#
            _ => true
        };
    }
    
    /// <summary>
    /// Gets notes for the current viewpage.
    /// </summary>
    public List<int> GetViewpageNotes(int instrumentMinNote, int instrumentMaxNote)
    {
        var allFilteredNotes = GetFilteredNotes(instrumentMinNote, instrumentMaxNote);
        
        if (!RowsPerViewpage.HasValue || !Columns.HasValue)
        {
            return allFilteredNotes;
        }
        
        int notesPerViewpage = RowsPerViewpage.Value * Columns.Value;
        int startIndex = CurrentViewpage * notesPerViewpage;
        
        if (startIndex >= allFilteredNotes.Count)
        {
            startIndex = 0;
            CurrentViewpage = 0;
        }
        
        return allFilteredNotes.Skip(startIndex).Take(notesPerViewpage).ToList();
    }
    
    /// <summary>
    /// Gets total number of viewpages for the given instrument range.
    /// </summary>
    public int GetTotalViewpages(int instrumentMinNote, int instrumentMaxNote)
    {
        if (!RowsPerViewpage.HasValue || !Columns.HasValue)
        {
            return 1;
        }
        
        var allFilteredNotes = GetFilteredNotes(instrumentMinNote, instrumentMaxNote);
        int notesPerViewpage = RowsPerViewpage.Value * Columns.Value;
        
        return Math.Max(1, (int)Math.Ceiling((double)allFilteredNotes.Count / notesPerViewpage));
    }
    
    /// <summary>
    /// Moves to the next viewpage (higher notes).
    /// </summary>
    public bool NextViewpage(int instrumentMinNote, int instrumentMaxNote)
    {
        int total = GetTotalViewpages(instrumentMinNote, instrumentMaxNote);
        if (CurrentViewpage < total - 1)
        {
            CurrentViewpage++;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Moves to the previous viewpage (lower notes).
    /// </summary>
    public bool PreviousViewpage(int instrumentMinNote, int instrumentMaxNote)
    {
        if (CurrentViewpage > 0)
        {
            CurrentViewpage--;
            return true;
        }
        return false;
    }
}

/// <summary>
/// Types of note filters for padreas.
/// </summary>
public enum NoteFilterType
{
    /// <summary>All 12 notes per octave.</summary>
    Chromatic,
    
    /// <summary>Major pentatonic: C, D, E, G, A (5 notes per octave).</summary>
    PentatonicMajor,
    
    /// <summary>Minor pentatonic: C, Eb, F, G, Bb (5 notes per octave).</summary>
    PentatonicMinor,
    
    /// <summary>White keys only: C, D, E, F, G, A, B (7 notes per octave).</summary>
    WhiteKeys,
    
    /// <summary>Black keys only: C#, D#, F#, G#, A# (5 notes per octave).</summary>
    BlackKeys
}

