using MusicPad.Core.Models;
using MusicPad.Core.Sfz;

namespace MusicPad.Tests.Models;

/// <summary>
/// Tests for unpitched instrument padrea behavior.
/// </summary>
public class UnpitchedPadreaTests
{
    #region Padrea Configuration Tests
    
    [Fact]
    public void Padrea_KindUnpitched_Exists()
    {
        // The Unpitched kind should exist in the enum
        Assert.True(Enum.IsDefined(typeof(PadreaKind), PadreaKind.Unpitched));
    }
    
    [Fact]
    public void UnpitchedPadrea_HasCorrectKind()
    {
        var padrea = new Padrea
        {
            Id = "drums",
            Name = "Drums",
            Kind = PadreaKind.Unpitched
        };
        
        Assert.Equal(PadreaKind.Unpitched, padrea.Kind);
    }
    
    #endregion
    
    #region Region-Based Pad Mapping Tests
    
    [Fact]
    public void SfzInstrument_GetUniqueMidiNotes_ReturnsDistinctNotes()
    {
        // Arrange - create an instrument with multiple regions at different notes
        var instrument = new SfzInstrument
        {
            Name = "Test Drums",
            BasePath = ""
        };
        
        // Add regions at specific MIDI notes (like a drum kit)
        instrument.Regions.Add(new SfzRegion { Key = 36, Sample = "kick.wav", RegionLabel = "Kick" });
        instrument.Regions.Add(new SfzRegion { Key = 38, Sample = "snare.wav", RegionLabel = "Snare" });
        instrument.Regions.Add(new SfzRegion { Key = 42, Sample = "hihat.wav", RegionLabel = "Hi-Hat" });
        instrument.Regions.Add(new SfzRegion { Key = 36, Sample = "kick_loud.wav", RegionLabel = "Kick Loud" }); // Velocity layer
        
        // Act - get unique MIDI notes
        var notes = instrument.GetUniqueMidiNotes();
        
        // Assert - should return distinct notes in order
        Assert.Equal(3, notes.Count);
        Assert.Equal(36, notes[0]);
        Assert.Equal(38, notes[1]);
        Assert.Equal(42, notes[2]);
    }
    
    [Fact]
    public void SfzInstrument_GetRegionLabel_ReturnsSampleNameWithoutExtension()
    {
        var instrument = new SfzInstrument
        {
            Name = "Test Drums",
            BasePath = ""
        };
        
        instrument.Regions.Add(new SfzRegion { Key = 36, Sample = "samples/kick_drum.wav" });
        
        // Get label for MIDI note 36
        var label = instrument.GetRegionLabel(36);
        
        Assert.Equal("kick_drum", label);
    }
    
    [Fact]
    public void SfzInstrument_GetRegionLabel_PrefersRegionLabelOverSampleName()
    {
        var instrument = new SfzInstrument
        {
            Name = "Test Drums",
            BasePath = ""
        };
        
        instrument.Regions.Add(new SfzRegion 
        { 
            Key = 36, 
            Sample = "samples/bd01.wav", 
            RegionLabel = "Kick" 
        });
        
        // Get label for MIDI note 36
        var label = instrument.GetRegionLabel(36);
        
        Assert.Equal("Kick", label);
    }
    
    [Fact]
    public void SfzInstrument_GetRegionLabel_ReturnsNoteNameIfNoLabel()
    {
        var instrument = new SfzInstrument
        {
            Name = "Test Drums",
            BasePath = ""
        };
        
        // Add region without sample or label
        instrument.Regions.Add(new SfzRegion { Key = 60, Sample = "" });
        
        // Get label for MIDI note 60 (C4)
        var label = instrument.GetRegionLabel(60);
        
        Assert.Equal("C4", label);
    }
    
    #endregion
    
    #region Unpitched Padrea Note Generation Tests
    
    [Fact]
    public void UnpitchedPadrea_GetFilteredNotes_ReturnsAllMidiNotesFromInstrumentRange()
    {
        // For unpitched instruments, we pass the unique MIDI notes from the instrument
        // rather than applying chromatic filtering
        var padrea = new Padrea
        {
            Id = "drums",
            Name = "Drums",
            Kind = PadreaKind.Unpitched,
            NoteFilter = NoteFilterType.Chromatic // Chromatic for unpitched = all passed notes
        };
        
        // For unpitched, the min/max should be the actual unique MIDI notes
        // Using the GetFilteredNotes method with explicit min/max
        var notes = padrea.GetFilteredNotes(36, 46);
        
        // Should include all notes 36-46 with chromatic filter
        Assert.Equal(11, notes.Count);
    }
    
    [Fact]
    public void UnpitchedPadrea_GridLayout_ShouldBeSquareish()
    {
        // Unpitched padrea should have a layout that accommodates the pads
        var padrea = new Padrea
        {
            Id = "drums",
            Name = "Drums",
            Kind = PadreaKind.Unpitched,
            Columns = 4,           // 4 pads per row for a 4x4 grid
            RowsPerViewpage = 4    // 4 rows = 16 pads per page
        };
        
        Assert.Equal(4, padrea.Columns);
        Assert.Equal(4, padrea.RowsPerViewpage);
    }
    
    #endregion
}




