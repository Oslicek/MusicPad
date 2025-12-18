using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.Models;

public class ArpeggiatorSettingsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var settings = new ArpeggiatorSettings();
        
        Assert.False(settings.IsEnabled);
        Assert.Equal(ArpPattern.Up, settings.Pattern);
        Assert.Equal(0.5f, settings.Rate, 0.001f); // Mid-range default
    }

    [Fact]
    public void Rate_IsClamped()
    {
        var settings = new ArpeggiatorSettings();
        
        settings.Rate = -0.5f;
        Assert.Equal(0f, settings.Rate);
        
        settings.Rate = 1.5f;
        Assert.Equal(1f, settings.Rate);
        
        settings.Rate = 0.7f;
        Assert.Equal(0.7f, settings.Rate, 0.001f);
    }

    [Theory]
    [InlineData(ArpPattern.Up)]
    [InlineData(ArpPattern.Down)]
    [InlineData(ArpPattern.UpDown)]
    [InlineData(ArpPattern.Random)]
    public void Pattern_CanBeSetToAnyValue(ArpPattern pattern)
    {
        var settings = new ArpeggiatorSettings();
        
        settings.Pattern = pattern;
        
        Assert.Equal(pattern, settings.Pattern);
    }

    [Fact]
    public void EnabledChanged_FiresOnChange()
    {
        var settings = new ArpeggiatorSettings();
        bool eventFired = false;
        
        settings.EnabledChanged += (s, e) => eventFired = true;
        settings.IsEnabled = true;
        
        Assert.True(eventFired);
    }

    [Fact]
    public void RateChanged_FiresOnChange()
    {
        var settings = new ArpeggiatorSettings();
        bool eventFired = false;
        float receivedRate = 0f;
        
        settings.RateChanged += (s, e) =>
        {
            eventFired = true;
            receivedRate = e;
        };
        settings.Rate = 0.8f;
        
        Assert.True(eventFired);
        Assert.Equal(0.8f, receivedRate, 0.001f);
    }

    [Fact]
    public void PatternChanged_FiresOnChange()
    {
        var settings = new ArpeggiatorSettings();
        bool eventFired = false;
        ArpPattern receivedPattern = ArpPattern.Up;
        
        settings.PatternChanged += (s, e) =>
        {
            eventFired = true;
            receivedPattern = e;
        };
        settings.Pattern = ArpPattern.Down;
        
        Assert.True(eventFired);
        Assert.Equal(ArpPattern.Down, receivedPattern);
    }

    [Fact]
    public void Events_DoNotFireForSameValue()
    {
        var settings = new ArpeggiatorSettings();
        int eventCount = 0;
        
        settings.EnabledChanged += (s, e) => eventCount++;
        settings.RateChanged += (s, e) => eventCount++;
        settings.PatternChanged += (s, e) => eventCount++;
        
        settings.IsEnabled = false;
        settings.Rate = 0.5f;
        settings.Pattern = ArpPattern.Up;
        
        Assert.Equal(0, eventCount);
    }

    [Theory]
    [InlineData(ArpPattern.Up, 0)]
    [InlineData(ArpPattern.Down, 1)]
    [InlineData(ArpPattern.UpDown, 2)]
    [InlineData(ArpPattern.Random, 3)]
    public void ArpPattern_HasCorrectIndexMapping(ArpPattern pattern, int expectedIndex)
    {
        Assert.Equal(expectedIndex, (int)pattern);
    }

    [Fact]
    public void AllArpPatterns_AreDefined()
    {
        var patterns = Enum.GetValues<ArpPattern>();
        
        Assert.Equal(4, patterns.Length);
        Assert.Contains(ArpPattern.Up, patterns);
        Assert.Contains(ArpPattern.Down, patterns);
        Assert.Contains(ArpPattern.UpDown, patterns);
        Assert.Contains(ArpPattern.Random, patterns);
    }
}

