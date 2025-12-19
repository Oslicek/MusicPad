using Xunit;
using MusicPad.Core.Models;

namespace MusicPad.Tests.Services;

// Tests for InstrumentImportInfo and SfzInstrumentInfo are in MusicPad.Core.Models

/// <summary>
/// Tests for instrument configuration handling including SFZ and WAV file processing.
/// </summary>
public class InstrumentConfigTests
{
    [Fact]
    public void InstrumentConfig_DefaultValues_AreCorrect()
    {
        var config = new InstrumentConfig();
        
        Assert.Equal(string.Empty, config.DisplayName);
        Assert.Equal(string.Empty, config.SfzPath);
        Assert.Equal(VoicingType.Polyphonic, config.Voicing);
        Assert.Equal(PitchType.Pitched, config.PitchType);
        Assert.True(config.IsBundled);
    }
    
    [Fact]
    public void InstrumentConfig_ComputeFileName_DerivedFromDisplayName()
    {
        var config = new InstrumentConfig { DisplayName = "My Piano" };
        
        Assert.Equal("My Piano.json", config.ComputeFileName());
    }
    
    [Fact]
    public void InstrumentConfig_ComputeFileName_SanitizesSpecialCharacters()
    {
        var config = new InstrumentConfig { DisplayName = "Test:Instrument/Name" };
        
        var fileName = config.ComputeFileName();
        // FileName should not contain invalid path characters
        Assert.DoesNotContain(":", fileName);
        Assert.DoesNotContain("/", fileName);
    }
    
    [Theory]
    [InlineData("Piano", VoicingType.Polyphonic)]
    [InlineData("Flute", VoicingType.Monophonic)]
    public void InstrumentConfig_Voicing_CanBeSetCorrectly(string name, VoicingType voicing)
    {
        var config = new InstrumentConfig
        {
            DisplayName = name,
            Voicing = voicing
        };
        
        Assert.Equal(voicing, config.Voicing);
    }
    
    [Theory]
    [InlineData("Violin", PitchType.Pitched)]
    [InlineData("Drums", PitchType.Unpitched)]
    public void InstrumentConfig_PitchType_CanBeSetCorrectly(string name, PitchType pitchType)
    {
        var config = new InstrumentConfig
        {
            DisplayName = name,
            PitchType = pitchType
        };
        
        Assert.Equal(pitchType, config.PitchType);
    }
    
    [Fact]
    public void InstrumentConfig_SfzPath_ParsesCorrectly()
    {
        var config = new InstrumentConfig { SfzPath = "MyInstrument/instrument.sfz" };
        
        var parts = config.SfzPath.Split('/');
        
        Assert.Equal(2, parts.Length);
        Assert.Equal("MyInstrument", parts[0]);
        Assert.Equal("instrument.sfz", parts[1]);
    }
    
    [Fact]
    public void InstrumentConfig_UserInstrument_HasIsBundledFalse()
    {
        var config = new InstrumentConfig
        {
            DisplayName = "User Synth",
            IsBundled = false
        };
        
        Assert.False(config.IsBundled);
    }
}

/// <summary>
/// Tests for SFZ file structure validation.
/// </summary>
public class SfzFileValidationTests
{
    [Theory]
    [InlineData("test.sfz", true)]
    [InlineData("instrument.SFZ", true)]
    [InlineData("MyPiano.Sfz", true)]
    [InlineData("test.wav", false)]
    [InlineData("test.sf2", false)]
    [InlineData("test.txt", false)]
    public void IsSfzFile_ValidatesExtension(string fileName, bool expected)
    {
        var isSfz = fileName.EndsWith(".sfz", StringComparison.OrdinalIgnoreCase);
        Assert.Equal(expected, isSfz);
    }
    
    [Theory]
    [InlineData("sample.wav", true)]
    [InlineData("audio.WAV", true)]
    [InlineData("Sample.Wav", true)]
    [InlineData("test.mp3", false)]
    [InlineData("test.ogg", false)]
    [InlineData("test.sfz", false)]
    public void IsWavFile_ValidatesExtension(string fileName, bool expected)
    {
        var isWav = fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);
        Assert.Equal(expected, isWav);
    }
}

/// <summary>
/// Tests for instrument import info validation.
/// </summary>
public class InstrumentImportInfoTests
{
    [Fact]
    public void InstrumentImportInfo_DefaultValues()
    {
        var importInfo = new InstrumentImportInfo();
        
        Assert.Equal(string.Empty, importInfo.DisplayName);
        Assert.Equal(VoicingType.Polyphonic, importInfo.Voicing);
        Assert.Equal(PitchType.Pitched, importInfo.PitchType);
        Assert.Equal(0, importInfo.InstrumentIndex);
    }
    
    [Fact]
    public void InstrumentImportInfo_CanSetAllProperties()
    {
        var importInfo = new InstrumentImportInfo
        {
            DisplayName = "Test Synth",
            Voicing = VoicingType.Monophonic,
            PitchType = PitchType.Unpitched,
            InstrumentIndex = 5
        };
        
        Assert.Equal("Test Synth", importInfo.DisplayName);
        Assert.Equal(VoicingType.Monophonic, importInfo.Voicing);
        Assert.Equal(PitchType.Unpitched, importInfo.PitchType);
        Assert.Equal(5, importInfo.InstrumentIndex);
    }
}

/// <summary>
/// Tests for SFZ instrument info from analysis.
/// </summary>
public class SfzInstrumentInfoTests
{
    [Fact]
    public void SfzInstrumentInfo_DefaultValues()
    {
        var info = new SfzInstrumentInfo();
        
        Assert.Equal(string.Empty, info.SuggestedName);
        Assert.Equal(0, info.Index);
        Assert.Equal(0, info.RegionCount);
        Assert.Equal(string.Empty, info.NoteRange);
    }
    
    [Fact]
    public void SfzInstrumentInfo_CanSetAllProperties()
    {
        var info = new SfzInstrumentInfo
        {
            SuggestedName = "Grand Piano",
            Index = 0,
            RegionCount = 88,
            NoteRange = "A0-C8"
        };
        
        Assert.Equal("Grand Piano", info.SuggestedName);
        Assert.Equal(0, info.Index);
        Assert.Equal(88, info.RegionCount);
        Assert.Equal("A0-C8", info.NoteRange);
    }
}

