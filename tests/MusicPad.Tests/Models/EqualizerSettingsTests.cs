using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.Models;

public class EqualizerSettingsTests
{
    [Fact]
    public void Default_AllBandsAreFlat()
    {
        var eq = new EqualizerSettings();
        
        // All bands should default to 0 (flat/neutral)
        Assert.Equal(0.0f, eq.LowGain);
        Assert.Equal(0.0f, eq.LowMidGain);
        Assert.Equal(0.0f, eq.HighMidGain);
        Assert.Equal(0.0f, eq.HighGain);
    }

    [Fact]
    public void BandCount_IsFour()
    {
        Assert.Equal(4, EqualizerSettings.BandCount);
    }

    [Theory]
    [InlineData(0, "Low")]
    [InlineData(1, "Low Mid")]
    [InlineData(2, "High Mid")]
    [InlineData(3, "High")]
    public void GetBandName_ReturnsCorrectName(int index, string expectedName)
    {
        Assert.Equal(expectedName, EqualizerSettings.GetBandName(index));
    }

    [Theory]
    [InlineData(-1.0f)]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void LowGain_CanBeSetWithinRange(float value)
    {
        var eq = new EqualizerSettings();
        
        eq.LowGain = value;
        
        Assert.Equal(value, eq.LowGain);
    }

    [Theory]
    [InlineData(-1.5f, -1.0f)]
    [InlineData(1.5f, 1.0f)]
    public void LowGain_IsClamped(float input, float expected)
    {
        var eq = new EqualizerSettings();
        
        eq.LowGain = input;
        
        Assert.Equal(expected, eq.LowGain);
    }

    [Theory]
    [InlineData(-1.0f)]
    [InlineData(0.0f)]
    [InlineData(1.0f)]
    public void LowMidGain_CanBeSetWithinRange(float value)
    {
        var eq = new EqualizerSettings();
        
        eq.LowMidGain = value;
        
        Assert.Equal(value, eq.LowMidGain);
    }

    [Theory]
    [InlineData(-1.0f)]
    [InlineData(0.0f)]
    [InlineData(1.0f)]
    public void HighMidGain_CanBeSetWithinRange(float value)
    {
        var eq = new EqualizerSettings();
        
        eq.HighMidGain = value;
        
        Assert.Equal(value, eq.HighMidGain);
    }

    [Theory]
    [InlineData(-1.0f)]
    [InlineData(0.0f)]
    [InlineData(1.0f)]
    public void HighGain_CanBeSetWithinRange(float value)
    {
        var eq = new EqualizerSettings();
        
        eq.HighGain = value;
        
        Assert.Equal(value, eq.HighGain);
    }

    [Fact]
    public void GetGain_ReturnsCorrectBandValue()
    {
        var eq = new EqualizerSettings
        {
            LowGain = -0.5f,
            LowMidGain = 0.2f,
            HighMidGain = 0.3f,
            HighGain = 0.8f
        };
        
        Assert.Equal(-0.5f, eq.GetGain(0));
        Assert.Equal(0.2f, eq.GetGain(1));
        Assert.Equal(0.3f, eq.GetGain(2));
        Assert.Equal(0.8f, eq.GetGain(3));
    }

    [Fact]
    public void SetGain_SetsCorrectBandValue()
    {
        var eq = new EqualizerSettings();
        
        eq.SetGain(0, -0.5f);
        eq.SetGain(1, 0.2f);
        eq.SetGain(2, 0.3f);
        eq.SetGain(3, 0.8f);
        
        Assert.Equal(-0.5f, eq.LowGain);
        Assert.Equal(0.2f, eq.LowMidGain);
        Assert.Equal(0.3f, eq.HighMidGain);
        Assert.Equal(0.8f, eq.HighGain);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(10)]
    public void GetGain_ThrowsForInvalidIndex(int index)
    {
        var eq = new EqualizerSettings();
        
        Assert.Throws<ArgumentOutOfRangeException>(() => eq.GetGain(index));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void SetGain_ThrowsForInvalidIndex(int index)
    {
        var eq = new EqualizerSettings();
        
        Assert.Throws<ArgumentOutOfRangeException>(() => eq.SetGain(index, 0.5f));
    }

    [Fact]
    public void BandChanged_EventFires_WhenGainChanges()
    {
        var eq = new EqualizerSettings();
        int? eventBand = null;
        float? eventValue = null;
        eq.BandChanged += (s, e) =>
        {
            eventBand = e.BandIndex;
            eventValue = e.NewGain;
        };
        
        eq.LowGain = 0.5f;
        
        Assert.Equal(0, eventBand);
        Assert.Equal(0.5f, eventValue);
    }

    [Fact]
    public void BandChanged_EventFires_ForAllBands()
    {
        var eq = new EqualizerSettings();
        var events = new List<(int band, float gain)>();
        eq.BandChanged += (s, e) => events.Add((e.BandIndex, e.NewGain));
        
        eq.LowGain = 0.1f;
        eq.LowMidGain = 0.2f;
        eq.HighMidGain = 0.3f;
        eq.HighGain = 0.4f;
        
        Assert.Equal(4, events.Count);
        Assert.Equal((0, 0.1f), events[0]);
        Assert.Equal((1, 0.2f), events[1]);
        Assert.Equal((2, 0.3f), events[2]);
        Assert.Equal((3, 0.4f), events[3]);
    }

    [Fact]
    public void BandChanged_EventDoesNotFire_WhenValueSame()
    {
        var eq = new EqualizerSettings();
        int eventCount = 0;
        eq.BandChanged += (s, e) => eventCount++;
        
        eq.LowGain = 0.0f; // Same as default
        
        Assert.Equal(0, eventCount);
    }

    [Theory]
    [InlineData(0, 100f)]    // Low band ~100Hz
    [InlineData(1, 500f)]    // Low-mid ~500Hz
    [InlineData(2, 2000f)]   // High-mid ~2kHz
    [InlineData(3, 8000f)]   // High ~8kHz
    public void GetBandFrequency_ReturnsApproximateFrequency(int band, float expectedApprox)
    {
        float freq = EqualizerSettings.GetBandCenterFrequency(band);
        
        // Allow 50% tolerance for approximate values
        Assert.True(freq >= expectedApprox * 0.5f && freq <= expectedApprox * 2.0f,
            $"Band {band} frequency {freq} not in expected range around {expectedApprox}");
    }

    [Fact]
    public void GainToDecibels_ConvertsCorrectly()
    {
        // 0 normalized = 0 dB
        Assert.Equal(0f, EqualizerSettings.GainToDecibels(0f), 1);
        
        // 1 normalized = +12 dB (max boost)
        Assert.Equal(12f, EqualizerSettings.GainToDecibels(1f), 1);
        
        // -1 normalized = -12 dB (max cut)
        Assert.Equal(-12f, EqualizerSettings.GainToDecibels(-1f), 1);
    }

    [Fact]
    public void Reset_SetsAllBandsToFlat()
    {
        var eq = new EqualizerSettings
        {
            LowGain = 0.5f,
            LowMidGain = -0.3f,
            HighMidGain = 0.7f,
            HighGain = -0.2f
        };
        
        eq.Reset();
        
        Assert.Equal(0.0f, eq.LowGain);
        Assert.Equal(0.0f, eq.LowMidGain);
        Assert.Equal(0.0f, eq.HighMidGain);
        Assert.Equal(0.0f, eq.HighGain);
    }
}

