using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.Models;

public class EffectSelectorTests
{
    [Fact]
    public void DefaultSelection_IsArpHarmony()
    {
        var selector = new EffectSelector();
        
        Assert.Equal(EffectType.ArpHarmony, selector.SelectedEffect);
    }

    [Fact]
    public void IsSelected_ReturnsTrueForSelectedEffect()
    {
        var selector = new EffectSelector();
        
        Assert.True(selector.IsSelected(EffectType.ArpHarmony));
        Assert.False(selector.IsSelected(EffectType.EQ));
        Assert.False(selector.IsSelected(EffectType.Chorus));
        Assert.False(selector.IsSelected(EffectType.Delay));
        Assert.False(selector.IsSelected(EffectType.Reverb));
    }

    [Fact]
    public void SetSelectedEffect_ChangesSelection()
    {
        var selector = new EffectSelector();
        
        selector.SelectedEffect = EffectType.Chorus;
        
        Assert.Equal(EffectType.Chorus, selector.SelectedEffect);
        Assert.True(selector.IsSelected(EffectType.Chorus));
        Assert.False(selector.IsSelected(EffectType.ArpHarmony));
    }

    [Fact]
    public void SetSelectedEffect_RaisesSelectionChangedEvent()
    {
        var selector = new EffectSelector();
        EffectType? eventEffect = null;
        selector.SelectionChanged += (s, e) => eventEffect = e;
        
        selector.SelectedEffect = EffectType.Delay;
        
        Assert.Equal(EffectType.Delay, eventEffect);
    }

    [Fact]
    public void SetSelectedEffect_SameValue_DoesNotRaiseEvent()
    {
        var selector = new EffectSelector();
        int eventCount = 0;
        selector.SelectionChanged += (s, e) => eventCount++;
        
        selector.SelectedEffect = EffectType.ArpHarmony; // Same as default
        
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void OnlyOneEffectCanBeSelectedAtATime()
    {
        var selector = new EffectSelector();
        
        // Select each effect and verify only one is selected
        foreach (var effect in EffectSelector.AllEffects)
        {
            selector.SelectedEffect = effect;
            
            int selectedCount = 0;
            foreach (var checkEffect in EffectSelector.AllEffects)
            {
                if (selector.IsSelected(checkEffect))
                    selectedCount++;
            }
            
            Assert.Equal(1, selectedCount);
        }
    }

    [Fact]
    public void AllEffects_ContainsAllTypesInOrder()
    {
        var effects = EffectSelector.AllEffects;
        
        Assert.Equal(5, effects.Count);
        Assert.Equal(EffectType.ArpHarmony, effects[0]);
        Assert.Equal(EffectType.EQ, effects[1]);
        Assert.Equal(EffectType.Chorus, effects[2]);
        Assert.Equal(EffectType.Delay, effects[3]);
        Assert.Equal(EffectType.Reverb, effects[4]);
    }
}
