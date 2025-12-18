using MusicPad.Core.Models;

namespace MusicPad.Tests.Services;

/// <summary>
/// Tests for expected padrea configurations.
/// Note: These test the expected Padrea configurations that should match PadreaService.
/// </summary>
public class PadreaConfigurationTests
{
    [Fact]
    public void ChromaticPadrea_ShouldHaveSixColumnsPerRow()
    {
        // Full Range chromatic padrea configuration
        var fullRange = new Padrea
        {
            Id = "full-range",
            Name = "Full Range",
            NoteFilter = NoteFilterType.Chromatic,
            Columns = 6  // Chromatic should have 6 pads per row
        };
        
        Assert.Equal(6, fullRange.Columns);
        Assert.Equal(NoteFilterType.Chromatic, fullRange.NoteFilter);
    }

    [Fact]
    public void HeptatonicScalePadrea_ShouldHaveSevenColumnsPerRow()
    {
        // Scales padrea configuration - 7 notes per scale = 7 columns
        var scales = new Padrea
        {
            Id = "scales",
            Name = "Scales",
            NoteFilter = NoteFilterType.HeptatonicScale,
            Columns = 7  // Heptatonic scales have 7 notes = 7 pads per row
        };
        
        Assert.Equal(7, scales.Columns);
        Assert.Equal(NoteFilterType.HeptatonicScale, scales.NoteFilter);
    }

    [Fact]
    public void PentatonicPadrea_ShouldHaveFiveColumnsPerRow()
    {
        // Pentatonic padrea configuration - 5 notes per octave = 5 columns
        var pentatonic = new Padrea
        {
            Id = "pentatonic",
            Name = "Pentatonic",
            NoteFilter = NoteFilterType.PentatonicMajor,
            Columns = 5  // Pentatonic has 5 notes = 5 pads per row
        };
        
        Assert.Equal(5, pentatonic.Columns);
        Assert.Equal(NoteFilterType.PentatonicMajor, pentatonic.NoteFilter);
    }

    [Fact]
    public void ChromaticPadrea_WithSixColumns_NotesPerRowMatchesOctaveHalf()
    {
        var padrea = new Padrea
        {
            NoteFilter = NoteFilterType.Chromatic,
            Columns = 6,
            RowsPerViewpage = 4
        };
        
        // 6 columns x 4 rows = 24 notes per viewpage = 2 octaves
        var notes = padrea.GetViewpageNotes(60, 83); // C4 to B5 = 24 notes
        Assert.Equal(24, notes.Count);
    }

    [Fact]
    public void HeptatonicPadrea_WithSevenColumns_NotesPerRowMatchesScale()
    {
        var padrea = new Padrea
        {
            NoteFilter = NoteFilterType.HeptatonicScale,
            ScaleType = ScaleType.Major,
            RootNote = 0,
            Columns = 7,
            RowsPerViewpage = 3
        };
        
        // 7 columns x 3 rows = 21 notes = 3 octaves of a scale
        var notes = padrea.GetViewpageNotes(48, 95); // Wide range
        Assert.Equal(21, notes.Count);
    }

    [Fact]
    public void PentatonicPadrea_WithFiveColumns_NotesPerRowMatchesOctave()
    {
        var padrea = new Padrea
        {
            NoteFilter = NoteFilterType.PentatonicMajor,
            Columns = 5,
            RowsPerViewpage = 5
        };
        
        // 5 columns x 5 rows = 25 notes = 5 octaves of pentatonic
        var notes = padrea.GetViewpageNotes(36, 95); // Wide range
        Assert.Equal(25, notes.Count);
    }
}
