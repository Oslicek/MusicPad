using MusicPad.Core.NoteProcessing;
using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.NoteProcessing;

public class HarmonyTests
{
    [Fact]
    public void WhenDisabled_ReturnsOnlyInputNote()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = false;
        
        var notes = harmony.ProcessNoteOn(60); // Middle C
        
        Assert.Single(notes);
        Assert.Equal(60, notes[0]);
    }

    [Fact]
    public void Octave_AddsNoteOneOctaveUp()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Octave;
        
        var notes = harmony.ProcessNoteOn(60); // Middle C
        
        Assert.Equal(2, notes.Length);
        Assert.Contains(60, notes);  // Root
        Assert.Contains(72, notes);  // +12 (octave)
    }

    [Fact]
    public void Fifth_AddsPerfectFifth()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Fifth;
        
        var notes = harmony.ProcessNoteOn(60); // C
        
        Assert.Equal(2, notes.Length);
        Assert.Contains(60, notes);  // Root (C)
        Assert.Contains(67, notes);  // +7 (G, perfect 5th)
    }

    [Fact]
    public void Major_AddsMajorTriad()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Major;
        
        var notes = harmony.ProcessNoteOn(60); // C
        
        Assert.Equal(3, notes.Length);
        Assert.Contains(60, notes);  // Root (C)
        Assert.Contains(64, notes);  // +4 (E, major 3rd)
        Assert.Contains(67, notes);  // +7 (G, perfect 5th)
    }

    [Fact]
    public void Minor_AddsMinorTriad()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Minor;
        
        var notes = harmony.ProcessNoteOn(60); // C
        
        Assert.Equal(3, notes.Length);
        Assert.Contains(60, notes);  // Root (C)
        Assert.Contains(63, notes);  // +3 (Eb, minor 3rd)
        Assert.Contains(67, notes);  // +7 (G, perfect 5th)
    }

    [Fact]
    public void ProcessNoteOff_ReturnsAllHarmonizedNotes()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Major;
        
        // First trigger note on to establish mapping
        harmony.ProcessNoteOn(60);
        
        // Now note off should return all related notes
        var notesOff = harmony.ProcessNoteOff(60);
        
        Assert.Equal(3, notesOff.Length);
        Assert.Contains(60, notesOff);
        Assert.Contains(64, notesOff);
        Assert.Contains(67, notesOff);
    }

    [Fact]
    public void HighNotes_DoNotExceedMidiRange()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Octave;
        
        var notes = harmony.ProcessNoteOn(120); // Very high note
        
        // Should not have notes above 127
        Assert.All(notes, n => Assert.True(n <= 127));
    }

    [Fact]
    public void MultipleNotesOn_TrackedSeparately()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Fifth;
        
        var notes1 = harmony.ProcessNoteOn(60); // C
        var notes2 = harmony.ProcessNoteOn(64); // E
        
        Assert.Equal(2, notes1.Length); // C + G
        Assert.Equal(2, notes2.Length); // E + B
        
        var off1 = harmony.ProcessNoteOff(60);
        Assert.Equal(2, off1.Length);
        Assert.Contains(60, off1);
        Assert.Contains(67, off1);
    }

    [Fact]
    public void ChangingType_AffectsNewNotes()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Major;
        
        var majorNotes = harmony.ProcessNoteOn(60);
        harmony.ProcessNoteOff(60);
        
        harmony.Type = HarmonyType.Minor;
        var minorNotes = harmony.ProcessNoteOn(60);
        
        // Major has E (64), Minor has Eb (63)
        Assert.Contains(64, majorNotes);
        Assert.Contains(63, minorNotes);
    }
}

