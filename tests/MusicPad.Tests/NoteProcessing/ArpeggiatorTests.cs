using MusicPad.Core.NoteProcessing;
using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.NoteProcessing;

public class ArpeggiatorTests
{
    [Fact]
    public void WhenDisabled_DoesNotArpeggiate()
    {
        var arp = new Arpeggiator();
        arp.IsEnabled = false;
        arp.AddNote(60);
        arp.AddNote(64);
        
        // Should return null when disabled (no arpeggiation)
        var note = arp.GetNextNote();
        
        Assert.Null(note);
    }

    [Fact]
    public void WhenEnabled_WithNoNotes_ReturnsNull()
    {
        var arp = new Arpeggiator();
        arp.IsEnabled = true;
        
        var note = arp.GetNextNote();
        
        Assert.Null(note);
    }

    [Fact]
    public void UpPattern_PlaysNotesAscending()
    {
        var arp = new Arpeggiator();
        arp.IsEnabled = true;
        arp.Pattern = ArpPattern.Up;
        
        arp.AddNote(60); // C
        arp.AddNote(64); // E
        arp.AddNote(67); // G
        
        Assert.Equal(60, arp.GetNextNote());
        Assert.Equal(64, arp.GetNextNote());
        Assert.Equal(67, arp.GetNextNote());
        Assert.Equal(60, arp.GetNextNote()); // Wraps around
    }

    [Fact]
    public void DownPattern_PlaysNotesDescending()
    {
        var arp = new Arpeggiator();
        arp.IsEnabled = true;
        arp.Pattern = ArpPattern.Down;
        
        arp.AddNote(60);
        arp.AddNote(64);
        arp.AddNote(67);
        
        Assert.Equal(67, arp.GetNextNote());
        Assert.Equal(64, arp.GetNextNote());
        Assert.Equal(60, arp.GetNextNote());
        Assert.Equal(67, arp.GetNextNote()); // Wraps around
    }

    [Fact]
    public void UpDownPattern_PlaysUpThenDown()
    {
        var arp = new Arpeggiator();
        arp.IsEnabled = true;
        arp.Pattern = ArpPattern.UpDown;
        
        arp.AddNote(60);
        arp.AddNote(64);
        arp.AddNote(67);
        
        // Up: 60, 64, 67
        Assert.Equal(60, arp.GetNextNote());
        Assert.Equal(64, arp.GetNextNote());
        Assert.Equal(67, arp.GetNextNote());
        // Down: 64, 60 (skip top note to avoid repeat)
        Assert.Equal(64, arp.GetNextNote());
        Assert.Equal(60, arp.GetNextNote());
        // Up again: 64, 67 (skip bottom to avoid repeat)
        Assert.Equal(64, arp.GetNextNote());
    }

    [Fact]
    public void RandomPattern_PlaysAllNotes()
    {
        var arp = new Arpeggiator();
        arp.IsEnabled = true;
        arp.Pattern = ArpPattern.Random;
        
        arp.AddNote(60);
        arp.AddNote(64);
        arp.AddNote(67);
        
        var playedNotes = new HashSet<int>();
        for (int i = 0; i < 30; i++) // Play many notes
        {
            var note = arp.GetNextNote();
            Assert.NotNull(note);
            playedNotes.Add(note.Value);
        }
        
        // All notes should have been played at least once
        Assert.Contains(60, playedNotes);
        Assert.Contains(64, playedNotes);
        Assert.Contains(67, playedNotes);
    }

    [Fact]
    public void RemoveNote_UpdatesSequence()
    {
        var arp = new Arpeggiator();
        arp.IsEnabled = true;
        arp.Pattern = ArpPattern.Up;
        
        arp.AddNote(60);
        arp.AddNote(64);
        arp.AddNote(67);
        
        Assert.Equal(60, arp.GetNextNote());
        
        arp.RemoveNote(64); // Remove middle note
        
        // Next cycle should skip 64
        Assert.Equal(67, arp.GetNextNote());
        Assert.Equal(60, arp.GetNextNote());
        Assert.Equal(67, arp.GetNextNote());
    }

    [Fact]
    public void SingleNote_RepeatsItself()
    {
        var arp = new Arpeggiator();
        arp.IsEnabled = true;
        arp.Pattern = ArpPattern.Up;
        
        arp.AddNote(60);
        
        Assert.Equal(60, arp.GetNextNote());
        Assert.Equal(60, arp.GetNextNote());
        Assert.Equal(60, arp.GetNextNote());
    }

    [Fact]
    public void Reset_ClearsAllNotes()
    {
        var arp = new Arpeggiator();
        arp.IsEnabled = true;
        arp.AddNote(60);
        arp.AddNote(64);
        
        arp.Reset();
        
        Assert.Null(arp.GetNextNote());
    }

    [Fact]
    public void GetBpmFromRate_MapsCorrectly()
    {
        var arp = new Arpeggiator();
        
        // Rate 0 = slowest, Rate 1 = fastest
        arp.Rate = 0f;
        Assert.True(arp.GetIntervalMs() > 400); // Slow
        
        arp.Rate = 1f;
        Assert.True(arp.GetIntervalMs() < 150); // Fast
    }

    [Fact]
    public void ActiveNotes_ReturnsCurrentNotes()
    {
        var arp = new Arpeggiator();
        arp.AddNote(60);
        arp.AddNote(64);
        arp.AddNote(67);
        
        var notes = arp.ActiveNotes;
        
        Assert.Equal(3, notes.Count);
        Assert.Contains(60, notes);
        Assert.Contains(64, notes);
        Assert.Contains(67, notes);
    }
}

