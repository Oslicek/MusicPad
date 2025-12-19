using MusicPad.Core.Models;

namespace MusicPad.Tests.Models;

public class InstrumentConfigTests
{
    [Fact]
    public void DefaultConfig_HasPolyphonicVoicing()
    {
        var config = new InstrumentConfig();
        
        Assert.Equal(VoicingType.Polyphonic, config.Voicing);
    }
    
    [Fact]
    public void DefaultConfig_IsPitched()
    {
        var config = new InstrumentConfig();
        
        Assert.Equal(PitchType.Pitched, config.PitchType);
    }
    
    [Fact]
    public void DefaultConfig_IsBundled()
    {
        var config = new InstrumentConfig();
        
        Assert.True(config.IsBundled);
    }
    
    [Fact]
    public void DefaultConfig_VersionIsOne()
    {
        var config = new InstrumentConfig();
        
        Assert.Equal(1, config.Version);
    }
    
    [Fact]
    public void Config_CanSetMonophonic()
    {
        var config = new InstrumentConfig
        {
            Voicing = VoicingType.Monophonic
        };
        
        Assert.Equal(VoicingType.Monophonic, config.Voicing);
    }
    
    [Fact]
    public void Config_CanSetUnpitched()
    {
        var config = new InstrumentConfig
        {
            PitchType = PitchType.Unpitched
        };
        
        Assert.Equal(PitchType.Unpitched, config.PitchType);
    }
    
    [Fact]
    public void Config_CanSetUserImported()
    {
        var config = new InstrumentConfig
        {
            IsBundled = false
        };
        
        Assert.False(config.IsBundled);
    }
    
    [Fact]
    public void Config_CanSetDisplayName()
    {
        var config = new InstrumentConfig
        {
            DisplayName = "My Custom Synth"
        };
        
        Assert.Equal("My Custom Synth", config.DisplayName);
    }
    
    [Fact]
    public void Config_CanSetSfzPath()
    {
        var config = new InstrumentConfig
        {
            SfzPath = "Petrof_sf2/000_Grandioso.sfz"
        };
        
        Assert.Equal("Petrof_sf2/000_Grandioso.sfz", config.SfzPath);
    }
    
    [Fact]
    public void Config_GeneratesValidFileName()
    {
        var config = new InstrumentConfig
        {
            DisplayName = "Piano"
        };
        
        Assert.Equal("Piano.json", config.FileName);
    }
    
    [Fact]
    public void Config_FileNameHandlesSpaces()
    {
        var config = new InstrumentConfig
        {
            DisplayName = "Acoustic Guitar"
        };
        
        Assert.Equal("Acoustic Guitar.json", config.FileName);
    }
    
    [Fact]
    public void Config_FileNameHandlesSpecialCharacters()
    {
        var config = new InstrumentConfig
        {
            DisplayName = "My/Custom:Synth"
        };
        
        // Special characters should be replaced with underscores
        Assert.Equal("My_Custom_Synth.json", config.FileName);
    }
}

