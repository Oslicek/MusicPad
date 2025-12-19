using MusicPad.Core.Models;

namespace MusicPad.Tests.Models;

public class AppSettingsTests
{
    [Fact]
    public void DefaultSettings_PianoKeyGlowEnabled_IsTrue()
    {
        var settings = new AppSettings();
        
        Assert.True(settings.PianoKeyGlowEnabled);
    }
    
    [Fact]
    public void DefaultSettings_PadGlowEnabled_IsTrue()
    {
        var settings = new AppSettings();
        
        Assert.True(settings.PadGlowEnabled);
    }
    
    [Fact]
    public void PianoKeyGlowEnabled_CanBeSetToFalse()
    {
        var settings = new AppSettings();
        
        settings.PianoKeyGlowEnabled = false;
        
        Assert.False(settings.PianoKeyGlowEnabled);
    }
    
    [Fact]
    public void PadGlowEnabled_CanBeSetToFalse()
    {
        var settings = new AppSettings();
        
        settings.PadGlowEnabled = false;
        
        Assert.False(settings.PadGlowEnabled);
    }
    
    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var settings = new AppSettings
        {
            PianoKeyGlowEnabled = false,
            PadGlowEnabled = false
        };
        
        var clone = settings.Clone();
        clone.PianoKeyGlowEnabled = true;
        clone.PadGlowEnabled = true;
        
        Assert.False(settings.PianoKeyGlowEnabled);
        Assert.False(settings.PadGlowEnabled);
        Assert.True(clone.PianoKeyGlowEnabled);
        Assert.True(clone.PadGlowEnabled);
    }
}

