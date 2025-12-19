using MusicPad.Core.Theme;

namespace MusicPad.Tests.Theme;

public class PaletteTests
{
    [Fact]
    public void DefaultPalette_HasCorrectCoreColors()
    {
        var palette = Palette.Default;
        
        Assert.Equal(0x8ECAE6u, palette.SkyBlue);
        Assert.Equal(0x219EBCu, palette.Teal);
        Assert.Equal(0x023047u, palette.Navy);
        Assert.Equal(0xFFB703u, palette.Amber);
        Assert.Equal(0xFB8500u, palette.Orange);
        Assert.Equal(0xFFFFFFu, palette.White);
        Assert.Equal(0x000000u, palette.Black);
    }
    
    [Fact]
    public void Palette_CanBeCreatedWithCustomColors()
    {
        var palette = new Palette(
            skyBlue: 0xAABBCC,
            teal: 0x112233,
            navy: 0x445566,
            amber: 0x778899,
            orange: 0xAABBCC,
            white: 0xFFFFFF,
            black: 0x000000
        );
        
        Assert.Equal(0xAABBCCu, palette.SkyBlue);
        Assert.Equal(0x112233u, palette.Teal);
    }
}

public class ComputedPaletteTests
{
    [Fact]
    public void ComputedPalette_Primary_EqualsTeal()
    {
        var palette = Palette.Default;
        var computed = new ComputedPalette(palette);
        
        Assert.Equal(ColorHelper.ToHex(palette.Teal), computed.Primary);
    }
    
    [Fact]
    public void ComputedPalette_PrimaryDark_IsDarkerThanTeal()
    {
        var palette = Palette.Default;
        var computed = new ComputedPalette(palette);
        
        // PrimaryDark should be a darker version of Teal
        var expectedDark = ColorHelper.ToHex(ColorHelper.Darker(palette.Teal, 0.2f));
        Assert.Equal(expectedDark, computed.PrimaryDark);
    }
    
    [Fact]
    public void ComputedPalette_PrimaryLight_IsLighterThanTeal()
    {
        var palette = Palette.Default;
        var computed = new ComputedPalette(palette);
        
        // PrimaryLight should be a lighter version of Teal
        var expectedLight = ColorHelper.ToHex(ColorHelper.Lighter(palette.Teal, 0.3f));
        Assert.Equal(expectedLight, computed.PrimaryLight);
    }
    
    [Fact]
    public void ComputedPalette_BackgroundMain_EqualsNavy()
    {
        var palette = Palette.Default;
        var computed = new ComputedPalette(palette);
        
        Assert.Equal(ColorHelper.ToHex(palette.Navy), computed.BackgroundMain);
    }
    
    [Fact]
    public void ComputedPalette_BackgroundEffect_IsLighterNavy()
    {
        var palette = Palette.Default;
        var computed = new ComputedPalette(palette);
        
        var expectedEffect = ColorHelper.ToHex(ColorHelper.Lighter(palette.Navy, 0.15f));
        Assert.Equal(expectedEffect, computed.BackgroundEffect);
    }
    
    [Fact]
    public void ComputedPalette_TextPrimary_EqualsSkyBlue()
    {
        var palette = Palette.Default;
        var computed = new ComputedPalette(palette);
        
        Assert.Equal(ColorHelper.ToHex(palette.SkyBlue), computed.TextPrimary);
    }
    
    [Fact]
    public void ComputedPalette_Accent_EqualsOrange()
    {
        var palette = Palette.Default;
        var computed = new ComputedPalette(palette);
        
        Assert.Equal(ColorHelper.ToHex(palette.Orange), computed.Accent);
    }
    
    [Fact]
    public void ComputedPalette_PianoWhiteKey_EqualsWhite()
    {
        var palette = Palette.Default;
        var computed = new ComputedPalette(palette);
        
        Assert.Equal(ColorHelper.ToHex(palette.White), computed.PianoWhiteKey);
    }
    
    [Fact]
    public void ComputedPalette_TransparentColors_HaveAlpha()
    {
        var palette = Palette.Default;
        var computed = new ComputedPalette(palette);
        
        // Semi-transparent colors should have alpha channel
        Assert.StartsWith("#40", computed.PianoStripHighlight);
        Assert.StartsWith("#40", computed.TextShadow);
    }
    
    [Fact]
    public void ComputedPalette_DifferentPalette_ProducesDifferentColors()
    {
        var defaultPalette = Palette.Default;
        var customPalette = new Palette(
            skyBlue: 0xFF0000,  // Red instead of sky blue
            teal: 0x00FF00,     // Green instead of teal
            navy: 0x0000FF,     // Blue instead of navy
            amber: 0xFFFF00,
            orange: 0xFF8800,
            white: 0xFFFFFF,
            black: 0x000000
        );
        
        var defaultComputed = new ComputedPalette(defaultPalette);
        var customComputed = new ComputedPalette(customPalette);
        
        Assert.NotEqual(defaultComputed.Primary, customComputed.Primary);
        Assert.NotEqual(defaultComputed.BackgroundMain, customComputed.BackgroundMain);
        Assert.NotEqual(defaultComputed.TextPrimary, customComputed.TextPrimary);
    }
}

