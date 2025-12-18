using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.Models;

public class ChorusSettingsTests
{
    [Fact]
    public void DefaultIsEnabled_IsFalse()
    {
        var chorus = new ChorusSettings();
        Assert.False(chorus.IsEnabled);
    }

    [Fact]
    public void DefaultDepth_IsMiddle()
    {
        var chorus = new ChorusSettings();
        Assert.Equal(0.5f, chorus.Depth);
    }

    [Fact]
    public void DefaultRate_IsLow()
    {
        var chorus = new ChorusSettings();
        Assert.Equal(0.3f, chorus.Rate);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void Depth_CanBeSetWithinRange(float value)
    {
        var chorus = new ChorusSettings();
        chorus.Depth = value;
        Assert.Equal(value, chorus.Depth);
    }

    [Theory]
    [InlineData(-0.1f, 0.0f)]
    [InlineData(1.1f, 1.0f)]
    public void Depth_IsClamped(float input, float expected)
    {
        var chorus = new ChorusSettings();
        chorus.Depth = input;
        Assert.Equal(expected, chorus.Depth);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void Rate_CanBeSetWithinRange(float value)
    {
        var chorus = new ChorusSettings();
        chorus.Rate = value;
        Assert.Equal(value, chorus.Rate);
    }

    [Theory]
    [InlineData(-0.1f, 0.0f)]
    [InlineData(1.1f, 1.0f)]
    public void Rate_IsClamped(float input, float expected)
    {
        var chorus = new ChorusSettings();
        chorus.Rate = input;
        Assert.Equal(expected, chorus.Rate);
    }

    [Fact]
    public void EnabledChanged_EventFires_WhenStateChanges()
    {
        var chorus = new ChorusSettings();
        bool? eventValue = null;
        chorus.EnabledChanged += (s, e) => eventValue = e;
        
        chorus.IsEnabled = true;
        
        Assert.True(eventValue);
    }

    [Fact]
    public void DepthChanged_EventFires_WhenValueChanges()
    {
        var chorus = new ChorusSettings();
        float? eventValue = null;
        chorus.DepthChanged += (s, e) => eventValue = e;
        
        chorus.Depth = 0.8f;
        
        Assert.Equal(0.8f, eventValue);
    }

    [Fact]
    public void RateChanged_EventFires_WhenValueChanges()
    {
        var chorus = new ChorusSettings();
        float? eventValue = null;
        chorus.RateChanged += (s, e) => eventValue = e;
        
        chorus.Rate = 0.7f;
        
        Assert.Equal(0.7f, eventValue);
    }

    [Fact]
    public void Reset_RestoresDefaults()
    {
        var chorus = new ChorusSettings();
        chorus.IsEnabled = true;
        chorus.Depth = 0.9f;
        chorus.Rate = 0.1f;
        
        chorus.Reset();
        
        Assert.False(chorus.IsEnabled);
        Assert.Equal(0.5f, chorus.Depth);
        Assert.Equal(0.3f, chorus.Rate);
    }
}

