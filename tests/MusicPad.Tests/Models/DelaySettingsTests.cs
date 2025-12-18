using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.Models;

public class DelaySettingsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var settings = new DelaySettings();
        
        Assert.Equal(0.4f, settings.Time, 0.001f);
        Assert.Equal(0.4f, settings.Feedback, 0.001f);
        Assert.Equal(0.5f, settings.Level, 0.001f);
        Assert.False(settings.IsEnabled);
    }

    [Fact]
    public void Time_IsClamped()
    {
        var settings = new DelaySettings();
        
        settings.Time = -0.5f;
        Assert.Equal(0f, settings.Time);
        
        settings.Time = 1.5f;
        Assert.Equal(1f, settings.Time);
        
        settings.Time = 0.75f;
        Assert.Equal(0.75f, settings.Time, 0.001f);
    }

    [Fact]
    public void Feedback_IsClamped()
    {
        var settings = new DelaySettings();
        
        settings.Feedback = -0.5f;
        Assert.Equal(0f, settings.Feedback);
        
        settings.Feedback = 1.5f;
        Assert.Equal(1f, settings.Feedback);
        
        settings.Feedback = 0.6f;
        Assert.Equal(0.6f, settings.Feedback, 0.001f);
    }

    [Fact]
    public void Level_IsClamped()
    {
        var settings = new DelaySettings();
        
        settings.Level = -0.5f;
        Assert.Equal(0f, settings.Level);
        
        settings.Level = 1.5f;
        Assert.Equal(1f, settings.Level);
        
        settings.Level = 0.8f;
        Assert.Equal(0.8f, settings.Level, 0.001f);
    }

    [Fact]
    public void EnabledChanged_FiresOnChange()
    {
        var settings = new DelaySettings();
        bool eventFired = false;
        bool receivedValue = false;
        
        settings.EnabledChanged += (s, e) =>
        {
            eventFired = true;
            receivedValue = e;
        };
        
        settings.IsEnabled = true;
        
        Assert.True(eventFired);
        Assert.True(receivedValue);
    }

    [Fact]
    public void TimeChanged_FiresOnChange()
    {
        var settings = new DelaySettings();
        bool eventFired = false;
        float receivedValue = 0f;
        
        settings.TimeChanged += (s, e) =>
        {
            eventFired = true;
            receivedValue = e;
        };
        
        settings.Time = 0.7f;
        
        Assert.True(eventFired);
        Assert.Equal(0.7f, receivedValue, 0.001f);
    }

    [Fact]
    public void FeedbackChanged_FiresOnChange()
    {
        var settings = new DelaySettings();
        bool eventFired = false;
        float receivedValue = 0f;
        
        settings.FeedbackChanged += (s, e) =>
        {
            eventFired = true;
            receivedValue = e;
        };
        
        settings.Feedback = 0.6f;
        
        Assert.True(eventFired);
        Assert.Equal(0.6f, receivedValue, 0.001f);
    }

    [Fact]
    public void LevelChanged_FiresOnChange()
    {
        var settings = new DelaySettings();
        bool eventFired = false;
        float receivedValue = 0f;
        
        settings.LevelChanged += (s, e) =>
        {
            eventFired = true;
            receivedValue = e;
        };
        
        settings.Level = 0.3f;
        
        Assert.True(eventFired);
        Assert.Equal(0.3f, receivedValue, 0.001f);
    }

    [Fact]
    public void Events_DoNotFireForSameValue()
    {
        var settings = new DelaySettings();
        int eventCount = 0;
        
        settings.EnabledChanged += (s, e) => eventCount++;
        settings.TimeChanged += (s, e) => eventCount++;
        settings.FeedbackChanged += (s, e) => eventCount++;
        settings.LevelChanged += (s, e) => eventCount++;
        
        // Set to same default values
        settings.IsEnabled = false;
        settings.Time = 0.4f;
        settings.Feedback = 0.4f;
        settings.Level = 0.5f;
        
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void Reset_RestoresDefaults()
    {
        var settings = new DelaySettings();
        
        settings.IsEnabled = true;
        settings.Time = 1.0f;
        settings.Feedback = 0.9f;
        settings.Level = 0.1f;
        
        settings.Reset();
        
        Assert.False(settings.IsEnabled);
        Assert.Equal(0.4f, settings.Time, 0.001f);
        Assert.Equal(0.4f, settings.Feedback, 0.001f);
        Assert.Equal(0.5f, settings.Level, 0.001f);
    }
}

