using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.Models;

public class LowPassFilterSettingsTests
{
    [Fact]
    public void DefaultIsEnabled_IsFalse()
    {
        var lpf = new LowPassFilterSettings();
        
        Assert.False(lpf.IsEnabled);
    }

    [Fact]
    public void DefaultCutoff_IsMaximum()
    {
        var lpf = new LowPassFilterSettings();
        
        // Default cutoff should be at max (filter fully open)
        Assert.Equal(1.0f, lpf.Cutoff);
    }

    [Fact]
    public void DefaultResonance_IsMinimum()
    {
        var lpf = new LowPassFilterSettings();
        
        // Default resonance should be at minimum (no resonance)
        Assert.Equal(0.0f, lpf.Resonance);
    }

    [Fact]
    public void EnabledChanged_EventFires_WhenStateChanges()
    {
        var lpf = new LowPassFilterSettings();
        bool? eventValue = null;
        lpf.EnabledChanged += (s, e) => eventValue = e;
        
        lpf.IsEnabled = true;
        
        Assert.True(eventValue);
    }

    [Fact]
    public void EnabledChanged_EventDoesNotFire_WhenStateSame()
    {
        var lpf = new LowPassFilterSettings();
        int eventCount = 0;
        lpf.EnabledChanged += (s, e) => eventCount++;
        
        lpf.IsEnabled = false; // Same as default
        
        Assert.Equal(0, eventCount);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void Cutoff_CanBeSetWithinRange(float value)
    {
        var lpf = new LowPassFilterSettings();
        
        lpf.Cutoff = value;
        
        Assert.Equal(value, lpf.Cutoff);
    }

    [Theory]
    [InlineData(-0.1f, 0.0f)]
    [InlineData(1.1f, 1.0f)]
    [InlineData(-10f, 0.0f)]
    [InlineData(10f, 1.0f)]
    public void Cutoff_IsClamped(float input, float expected)
    {
        var lpf = new LowPassFilterSettings();
        
        lpf.Cutoff = input;
        
        Assert.Equal(expected, lpf.Cutoff);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void Resonance_CanBeSetWithinRange(float value)
    {
        var lpf = new LowPassFilterSettings();
        
        lpf.Resonance = value;
        
        Assert.Equal(value, lpf.Resonance);
    }

    [Theory]
    [InlineData(-0.1f, 0.0f)]
    [InlineData(1.1f, 1.0f)]
    public void Resonance_IsClamped(float input, float expected)
    {
        var lpf = new LowPassFilterSettings();
        
        lpf.Resonance = input;
        
        Assert.Equal(expected, lpf.Resonance);
    }

    [Fact]
    public void CutoffChanged_EventFires_WhenValueChanges()
    {
        var lpf = new LowPassFilterSettings();
        float? eventValue = null;
        lpf.CutoffChanged += (s, e) => eventValue = e;
        
        lpf.Cutoff = 0.5f;
        
        Assert.Equal(0.5f, eventValue);
    }

    [Fact]
    public void CutoffChanged_EventDoesNotFire_WhenValueSame()
    {
        var lpf = new LowPassFilterSettings();
        lpf.Cutoff = 0.5f;
        int eventCount = 0;
        lpf.CutoffChanged += (s, e) => eventCount++;
        
        lpf.Cutoff = 0.5f;
        
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void ResonanceChanged_EventFires_WhenValueChanges()
    {
        var lpf = new LowPassFilterSettings();
        float? eventValue = null;
        lpf.ResonanceChanged += (s, e) => eventValue = e;
        
        lpf.Resonance = 0.7f;
        
        Assert.Equal(0.7f, eventValue);
    }

    [Fact]
    public void ResonanceChanged_EventDoesNotFire_WhenValueSame()
    {
        var lpf = new LowPassFilterSettings();
        int eventCount = 0;
        lpf.ResonanceChanged += (s, e) => eventCount++;
        
        lpf.Resonance = 0.0f; // Same as default
        
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void GetCutoffFrequency_ReturnsFrequencyInHz()
    {
        var lpf = new LowPassFilterSettings();
        
        // At cutoff = 1.0, should return max frequency (e.g., 20kHz)
        lpf.Cutoff = 1.0f;
        float maxFreq = lpf.GetCutoffFrequencyHz();
        Assert.True(maxFreq >= 15000f);
        
        // At cutoff = 0.0, should return min frequency (e.g., 20Hz)
        lpf.Cutoff = 0.0f;
        float minFreq = lpf.GetCutoffFrequencyHz();
        Assert.True(minFreq <= 100f);
        Assert.True(minFreq >= 20f);
    }

    [Fact]
    public void GetResonanceQ_ReturnsQFactor()
    {
        var lpf = new LowPassFilterSettings();
        
        // At resonance = 0, Q should be ~0.707 (Butterworth)
        lpf.Resonance = 0.0f;
        float minQ = lpf.GetResonanceQ();
        Assert.True(minQ >= 0.5f && minQ <= 1.0f);
        
        // At resonance = 1, Q should be high (e.g., 10+)
        lpf.Resonance = 1.0f;
        float maxQ = lpf.GetResonanceQ();
        Assert.True(maxQ >= 5f);
    }
}

