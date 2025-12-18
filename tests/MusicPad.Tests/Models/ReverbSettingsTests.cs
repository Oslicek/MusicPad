using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.Models;

public class ReverbSettingsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var settings = new ReverbSettings();
        
        Assert.Equal(0.3f, settings.Level, 0.001f);
        Assert.Equal(ReverbType.Room, settings.Type);
        Assert.False(settings.IsEnabled);
    }

    [Fact]
    public void Level_IsClamped()
    {
        var settings = new ReverbSettings();
        
        settings.Level = -0.5f;
        Assert.Equal(0f, settings.Level);
        
        settings.Level = 1.5f;
        Assert.Equal(1f, settings.Level);
        
        settings.Level = 0.7f;
        Assert.Equal(0.7f, settings.Level, 0.001f);
    }

    [Theory]
    [InlineData(ReverbType.Room)]
    [InlineData(ReverbType.Hall)]
    [InlineData(ReverbType.Plate)]
    [InlineData(ReverbType.Church)]
    public void Type_CanBeSetToAnyValue(ReverbType type)
    {
        var settings = new ReverbSettings();
        
        settings.Type = type;
        
        Assert.Equal(type, settings.Type);
    }

    [Fact]
    public void EnabledChanged_FiresOnChange()
    {
        var settings = new ReverbSettings();
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
    public void EnabledChanged_DoesNotFireForSameValue()
    {
        var settings = new ReverbSettings();
        int eventCount = 0;
        
        settings.EnabledChanged += (s, e) => eventCount++;
        
        settings.IsEnabled = false; // Same as default
        
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void LevelChanged_FiresOnChange()
    {
        var settings = new ReverbSettings();
        bool eventFired = false;
        float receivedValue = 0f;
        
        settings.LevelChanged += (s, e) =>
        {
            eventFired = true;
            receivedValue = e;
        };
        
        settings.Level = 0.8f;
        
        Assert.True(eventFired);
        Assert.Equal(0.8f, receivedValue, 0.001f);
    }

    [Fact]
    public void LevelChanged_DoesNotFireForSameValue()
    {
        var settings = new ReverbSettings();
        int eventCount = 0;
        
        settings.LevelChanged += (s, e) => eventCount++;
        
        settings.Level = 0.3f; // Same as default
        
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void TypeChanged_FiresOnChange()
    {
        var settings = new ReverbSettings();
        bool eventFired = false;
        ReverbType receivedValue = ReverbType.Room;
        
        settings.TypeChanged += (s, e) =>
        {
            eventFired = true;
            receivedValue = e;
        };
        
        settings.Type = ReverbType.Hall;
        
        Assert.True(eventFired);
        Assert.Equal(ReverbType.Hall, receivedValue);
    }

    [Fact]
    public void TypeChanged_DoesNotFireForSameValue()
    {
        var settings = new ReverbSettings();
        int eventCount = 0;
        
        settings.TypeChanged += (s, e) => eventCount++;
        
        settings.Type = ReverbType.Room; // Same as default
        
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void TypeSelector_OnlyOneTypeCanBeSelected()
    {
        var settings = new ReverbSettings();
        
        // Start with Room (default)
        Assert.Equal(ReverbType.Room, settings.Type);
        
        // Select Hall - should deselect Room
        settings.Type = ReverbType.Hall;
        Assert.Equal(ReverbType.Hall, settings.Type);
        Assert.NotEqual(ReverbType.Room, settings.Type);
        
        // Select Plate - should deselect Hall
        settings.Type = ReverbType.Plate;
        Assert.Equal(ReverbType.Plate, settings.Type);
        Assert.NotEqual(ReverbType.Hall, settings.Type);
        
        // Select Church - should deselect Plate
        settings.Type = ReverbType.Church;
        Assert.Equal(ReverbType.Church, settings.Type);
        Assert.NotEqual(ReverbType.Plate, settings.Type);
        
        // Select Room again - should deselect Church
        settings.Type = ReverbType.Room;
        Assert.Equal(ReverbType.Room, settings.Type);
        Assert.NotEqual(ReverbType.Church, settings.Type);
    }

    [Fact]
    public void TypeSelector_SelectingSameTypeDoesNotFireEvent()
    {
        var settings = new ReverbSettings();
        settings.Type = ReverbType.Hall;
        
        int eventCount = 0;
        settings.TypeChanged += (s, e) => eventCount++;
        
        // Select same type again
        settings.Type = ReverbType.Hall;
        
        Assert.Equal(0, eventCount);
    }

    [Theory]
    [InlineData(ReverbType.Room, 0)]
    [InlineData(ReverbType.Hall, 1)]
    [InlineData(ReverbType.Plate, 2)]
    [InlineData(ReverbType.Church, 3)]
    public void TypeSelector_CorrectIndexMapping(ReverbType type, int expectedIndex)
    {
        // Verify enum values map to expected indices for button layout
        Assert.Equal(expectedIndex, (int)type);
    }

    [Fact]
    public void AllReverbTypes_AreDefined()
    {
        var types = Enum.GetValues<ReverbType>();
        
        Assert.Equal(4, types.Length);
        Assert.Contains(ReverbType.Room, types);
        Assert.Contains(ReverbType.Hall, types);
        Assert.Contains(ReverbType.Plate, types);
        Assert.Contains(ReverbType.Church, types);
    }
}

