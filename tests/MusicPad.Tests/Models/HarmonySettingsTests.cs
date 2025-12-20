using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.Models;

public class HarmonySettingsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var settings = new HarmonySettings();
        
        Assert.False(settings.IsEnabled);
        Assert.True(settings.IsAllowed); // Harmony allowed by default
        Assert.Equal(HarmonyType.Major, settings.Type);
    }

    [Theory]
    [InlineData(HarmonyType.Octave)]
    [InlineData(HarmonyType.Fifth)]
    [InlineData(HarmonyType.Major)]
    [InlineData(HarmonyType.Minor)]
    public void Type_CanBeSetToAnyValue(HarmonyType type)
    {
        var settings = new HarmonySettings();
        
        settings.Type = type;
        
        Assert.Equal(type, settings.Type);
    }

    [Fact]
    public void EnabledChanged_FiresOnChange()
    {
        var settings = new HarmonySettings();
        bool eventFired = false;
        
        settings.EnabledChanged += (s, e) => eventFired = true;
        settings.IsEnabled = true;
        
        Assert.True(eventFired);
    }

    [Fact]
    public void EnabledChanged_DoesNotFireForSameValue()
    {
        var settings = new HarmonySettings();
        int eventCount = 0;
        
        settings.EnabledChanged += (s, e) => eventCount++;
        settings.IsEnabled = false; // Same as default
        
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void TypeChanged_FiresOnChange()
    {
        var settings = new HarmonySettings();
        bool eventFired = false;
        HarmonyType receivedType = HarmonyType.Major;
        
        settings.TypeChanged += (s, e) =>
        {
            eventFired = true;
            receivedType = e;
        };
        settings.Type = HarmonyType.Minor;
        
        Assert.True(eventFired);
        Assert.Equal(HarmonyType.Minor, receivedType);
    }

    [Fact]
    public void TypeChanged_DoesNotFireForSameValue()
    {
        var settings = new HarmonySettings();
        int eventCount = 0;
        
        settings.TypeChanged += (s, e) => eventCount++;
        settings.Type = HarmonyType.Major; // Same as default
        
        Assert.Equal(0, eventCount);
    }

    [Theory]
    [InlineData(HarmonyType.Octave, 0)]
    [InlineData(HarmonyType.Fifth, 1)]
    [InlineData(HarmonyType.Major, 2)]
    [InlineData(HarmonyType.Minor, 3)]
    public void HarmonyType_HasCorrectIndexMapping(HarmonyType type, int expectedIndex)
    {
        Assert.Equal(expectedIndex, (int)type);
    }

    [Fact]
    public void AllHarmonyTypes_AreDefined()
    {
        var types = Enum.GetValues<HarmonyType>();
        
        Assert.Equal(4, types.Length);
        Assert.Contains(HarmonyType.Octave, types);
        Assert.Contains(HarmonyType.Fifth, types);
        Assert.Contains(HarmonyType.Major, types);
        Assert.Contains(HarmonyType.Minor, types);
    }
    
    #region IsAllowed Tests (for monophonic instrument handling)
    
    [Fact]
    public void AllowedChanged_FiresOnChange()
    {
        var settings = new HarmonySettings();
        bool eventFired = false;
        bool receivedValue = true;
        
        settings.AllowedChanged += (s, e) =>
        {
            eventFired = true;
            receivedValue = e;
        };
        settings.IsAllowed = false;
        
        Assert.True(eventFired);
        Assert.False(receivedValue);
    }
    
    [Fact]
    public void AllowedChanged_DoesNotFireForSameValue()
    {
        var settings = new HarmonySettings();
        int eventCount = 0;
        
        settings.AllowedChanged += (s, e) => eventCount++;
        settings.IsAllowed = true; // Same as default
        
        Assert.Equal(0, eventCount);
    }
    
    [Fact]
    public void IsAllowed_FalseForMonophonicInstruments()
    {
        // This test documents the expected behavior:
        // When switching to a monophonic instrument, IsAllowed should be set to false
        // and the UI should disable the harmony controls
        var settings = new HarmonySettings();
        settings.IsEnabled = true; // Harmony was on
        
        // Simulate switching to monophonic instrument
        settings.IsAllowed = false;
        
        Assert.False(settings.IsAllowed);
        // Note: The UI layer (MainPage) is responsible for also setting IsEnabled = false
    }
    
    #endregion
}

