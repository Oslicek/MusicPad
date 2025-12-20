using MusicPad.Core.NoteProcessing;
using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.NoteProcessing;

/// <summary>
/// Tests for the Harmony chord effect processor.
/// Note: MainPage disables harmony (_harmonyAllowed = false) for monophonic instruments,
/// bypassing the Harmony class entirely. This ensures monophonic instruments like
/// flute, lead synths, etc. don't attempt to play chords.
/// </summary>
public class HarmonyTests
{
    /// <summary>
    /// When Harmony.IsEnabled is false, only the original note is returned.
    /// This is the same behavior as when MainPage bypasses harmony for monophonic instruments.
    /// </summary>
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
    
    #region Live Harmony Type Change Tests
    
    [Fact]
    public void GetActiveRootNotes_ReturnsRootNotesOnly()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Major;
        
        // Press C and E
        harmony.ProcessNoteOn(60);
        harmony.ProcessNoteOn(64);
        
        var rootNotes = harmony.GetActiveRootNotes();
        
        Assert.Equal(2, rootNotes.Count);
        Assert.Contains(60, rootNotes);
        Assert.Contains(64, rootNotes);
    }
    
    [Fact]
    public void ReharmonizeActiveNotes_ReturnsNotesToAddAndRemove()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Major;
        
        // Press C with Major: C(60), E(64), G(67)
        harmony.ProcessNoteOn(60);
        
        // Change to Minor: C(60), Eb(63), G(67)
        // Should remove E(64), add Eb(63)
        var (notesToRemove, notesToAdd) = harmony.ReharmonizeActiveNotes(HarmonyType.Minor);
        
        Assert.Contains(64, notesToRemove); // E should be removed
        Assert.Contains(63, notesToAdd);    // Eb should be added
        Assert.DoesNotContain(60, notesToRemove); // Root stays
        Assert.DoesNotContain(67, notesToRemove); // Fifth stays
    }
    
    [Fact]
    public void ReharmonizeActiveNotes_UpdatesInternalState()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Major;
        
        harmony.ProcessNoteOn(60);
        harmony.ReharmonizeActiveNotes(HarmonyType.Minor);
        
        // Now releasing the key should return Minor notes
        var notesOff = harmony.ProcessNoteOff(60);
        
        Assert.Contains(60, notesOff);  // Root
        Assert.Contains(63, notesOff);  // Minor 3rd (Eb)
        Assert.Contains(67, notesOff);  // Fifth
        Assert.DoesNotContain(64, notesOff); // Not Major 3rd
    }
    
    [Fact]
    public void ReharmonizeActiveNotes_FromMajorToOctave()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Major;
        
        // Major: C(60), E(64), G(67)
        harmony.ProcessNoteOn(60);
        
        // Octave: C(60), C(72)
        // Remove E(64), G(67), add C(72)
        var (notesToRemove, notesToAdd) = harmony.ReharmonizeActiveNotes(HarmonyType.Octave);
        
        Assert.Contains(64, notesToRemove); // E
        Assert.Contains(67, notesToRemove); // G
        Assert.Contains(72, notesToAdd);    // Octave C
    }
    
    [Fact]
    public void ReharmonizeActiveNotes_MultipleRootNotes()
    {
        var harmony = new Harmony();
        harmony.IsEnabled = true;
        harmony.Type = HarmonyType.Fifth;
        
        // Fifth on C: C(60), G(67)
        // Fifth on E: E(64), B(71)
        harmony.ProcessNoteOn(60);
        harmony.ProcessNoteOn(64);
        
        // Change to Octave:
        // C(60), C(72) - remove G(67), add C(72)
        // E(64), E(76) - remove B(71), add E(76)
        var (notesToRemove, notesToAdd) = harmony.ReharmonizeActiveNotes(HarmonyType.Octave);
        
        Assert.Contains(67, notesToRemove); // G
        Assert.Contains(71, notesToRemove); // B
        Assert.Contains(72, notesToAdd);    // Octave C
        Assert.Contains(76, notesToAdd);    // Octave E
    }
    
    #endregion
}

