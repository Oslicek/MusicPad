using MusicPad.Core.Models;

namespace MusicPad.Tests.Models;

public class PadreaTests
{
    [Fact]
    public void Padrea_DefaultValues_AreCorrect()
    {
        var padrea = new Padrea();
        
        Assert.Equal(string.Empty, padrea.Id);
        Assert.Equal(string.Empty, padrea.Name);
        Assert.Null(padrea.Description);
        Assert.Null(padrea.MinNote);
        Assert.Null(padrea.MaxNote);
        Assert.Null(padrea.Columns);
        Assert.Null(padrea.RowsPerViewpage);
        Assert.Equal(NoteFilterType.Chromatic, padrea.NoteFilter);
        Assert.Equal(0, padrea.CurrentViewpage);
    }

    [Fact]
    public void Padrea_ToString_ReturnsName()
    {
        var padrea = new Padrea { Name = "My Padrea" };
        
        Assert.Equal("My Padrea", padrea.ToString());
    }

    [Fact]
    public void Padrea_CanSetAllProperties()
    {
        var padrea = new Padrea
        {
            Id = "test-id",
            Name = "Test Padrea",
            Description = "A test padrea",
            MinNote = 36,
            MaxNote = 84,
            Columns = 5,
            RowsPerViewpage = 5,
            NoteFilter = NoteFilterType.PentatonicMajor,
            PadColor = "#FF00FF",
            PadPressedColor = "#FF66FF"
        };

        Assert.Equal("test-id", padrea.Id);
        Assert.Equal("Test Padrea", padrea.Name);
        Assert.Equal("A test padrea", padrea.Description);
        Assert.Equal(36, padrea.MinNote);
        Assert.Equal(84, padrea.MaxNote);
        Assert.Equal(5, padrea.Columns);
        Assert.Equal(5, padrea.RowsPerViewpage);
        Assert.Equal(NoteFilterType.PentatonicMajor, padrea.NoteFilter);
        Assert.Equal("#FF00FF", padrea.PadColor);
    }

    #region Note Filter Tests

    [Theory]
    [InlineData(0, true)]   // C
    [InlineData(2, true)]   // D
    [InlineData(4, true)]   // E
    [InlineData(7, true)]   // G
    [InlineData(9, true)]   // A
    [InlineData(1, false)]  // C#
    [InlineData(3, false)]  // D#
    [InlineData(5, false)]  // F
    [InlineData(6, false)]  // F#
    [InlineData(8, false)]  // G#
    [InlineData(10, false)] // A#
    [InlineData(11, false)] // B
    public void PentatonicMajor_FiltersCorrectly(int noteInOctave, bool shouldPass)
    {
        var padrea = new Padrea { NoteFilter = NoteFilterType.PentatonicMajor };
        
        // Test across multiple octaves
        Assert.Equal(shouldPass, padrea.PassesFilter(60 + noteInOctave)); // Octave 4
        Assert.Equal(shouldPass, padrea.PassesFilter(48 + noteInOctave)); // Octave 3
    }

    [Fact]
    public void Chromatic_AllowsAllNotes()
    {
        var padrea = new Padrea { NoteFilter = NoteFilterType.Chromatic };
        
        for (int note = 0; note < 128; note++)
        {
            Assert.True(padrea.PassesFilter(note));
        }
    }

    [Fact]
    public void GetFilteredNotes_ReturnsOnlyMatchingNotes()
    {
        var padrea = new Padrea { NoteFilter = NoteFilterType.PentatonicMajor };
        
        // One octave from C4 to B4
        var notes = padrea.GetFilteredNotes(60, 71);
        
        // Should have 5 notes: C4, D4, E4, G4, A4
        Assert.Equal(5, notes.Count);
        Assert.Contains(60, notes); // C4
        Assert.Contains(62, notes); // D4
        Assert.Contains(64, notes); // E4
        Assert.Contains(67, notes); // G4
        Assert.Contains(69, notes); // A4
    }

    #endregion

    #region Viewpage Tests

    [Fact]
    public void GetTotalViewpages_CalculatesCorrectly()
    {
        var padrea = new Padrea
        {
            NoteFilter = NoteFilterType.PentatonicMajor,
            Columns = 5,
            RowsPerViewpage = 5
        };
        
        // 25 notes per viewpage (5 cols x 5 rows)
        // C2 (36) to B2 (47) = one octave = 5 pentatonic notes
        // We need exactly 25 notes = 5 octaves
        // C2 (36) to A6 (93) should give us 25 notes
        var notes = padrea.GetFilteredNotes(36, 93);
        Assert.Equal(25, notes.Count); // Verify our range gives 25 notes
        
        int total = padrea.GetTotalViewpages(36, 93);
        Assert.Equal(1, total); // Exactly 25 notes fits in one page
    }

    [Fact]
    public void GetTotalViewpages_RoundsUp()
    {
        var padrea = new Padrea
        {
            NoteFilter = NoteFilterType.PentatonicMajor,
            Columns = 5,
            RowsPerViewpage = 5
        };
        
        // C2 (36) to C8 (108) = more than 25 notes
        int total = padrea.GetTotalViewpages(36, 108);
        
        Assert.True(total >= 2);
    }

    [Fact]
    public void GetViewpageNotes_ReturnsCorrectSubset()
    {
        var padrea = new Padrea
        {
            NoteFilter = NoteFilterType.PentatonicMajor,
            Columns = 5,
            RowsPerViewpage = 1 // Only 5 notes per page
        };
        
        // First viewpage
        padrea.CurrentViewpage = 0;
        var page0 = padrea.GetViewpageNotes(60, 83); // C4 to B5
        Assert.Equal(5, page0.Count);
        
        // Second viewpage
        padrea.CurrentViewpage = 1;
        var page1 = padrea.GetViewpageNotes(60, 83);
        Assert.Equal(5, page1.Count);
        
        // Pages should have different notes
        Assert.NotEqual(page0, page1);
    }

    [Fact]
    public void NextViewpage_IncrementsAndReturnsTrue()
    {
        var padrea = new Padrea
        {
            NoteFilter = NoteFilterType.PentatonicMajor,
            Columns = 5,
            RowsPerViewpage = 1
        };
        
        Assert.Equal(0, padrea.CurrentViewpage);
        bool moved = padrea.NextViewpage(36, 96);
        Assert.True(moved);
        Assert.Equal(1, padrea.CurrentViewpage);
    }

    [Fact]
    public void PreviousViewpage_DecrementsAndReturnsTrue()
    {
        var padrea = new Padrea
        {
            NoteFilter = NoteFilterType.PentatonicMajor,
            Columns = 5,
            RowsPerViewpage = 1,
            CurrentViewpage = 2
        };
        
        bool moved = padrea.PreviousViewpage(36, 96);
        Assert.True(moved);
        Assert.Equal(1, padrea.CurrentViewpage);
    }

    [Fact]
    public void PreviousViewpage_AtZero_ReturnsFalse()
    {
        var padrea = new Padrea
        {
            CurrentViewpage = 0
        };
        
        bool moved = padrea.PreviousViewpage(36, 96);
        Assert.False(moved);
        Assert.Equal(0, padrea.CurrentViewpage);
    }

    #endregion
}

