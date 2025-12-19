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

public class NewPaletteTests
{
    [Fact]
    public void AbyssalForge_HasCorrectColors()
    {
        var palette = Palette.AbyssalForge;
        
        // Back: Deep Trench (10, 20, 30) = 0x0A141E
        Assert.Equal(0x0A141Eu, palette.Navy);
        // Front: Storm Slate (35, 55, 75) = 0x23374B
        Assert.Equal(0x23374Bu, palette.Teal);
        // Detail: Kelp Shadow (60, 90, 100) = 0x3C5A64
        Assert.Equal(0x3C5A64u, palette.SkyBlue);
        // Accent 1: Bio-Lume Cyan (0, 255, 205) = 0x00FFCD
        Assert.Equal(0x00FFCDu, palette.Amber);
        // Accent 2: Molten Amber (255, 170, 50) = 0xFFAA32
        Assert.Equal(0xFFAA32u, palette.Orange);
        Assert.Equal(0xFFFFFFu, palette.White);
        Assert.Equal(0x000000u, palette.Black);
    }
    
    [Fact]
    public void NorthernArchive_HasCorrectColors()
    {
        var palette = Palette.NorthernArchive;
        
        // Back: Midnight Fjord (15, 25, 35) = 0x0F1923
        Assert.Equal(0x0F1923u, palette.Navy);
        // Front: Arctic Steel (50, 75, 95) = 0x324B5F
        Assert.Equal(0x324B5Fu, palette.Teal);
        // Detail: Frost Glass (100, 130, 150) = 0x648296
        Assert.Equal(0x648296u, palette.SkyBlue);
        // Accent 1: Glacial White (220, 245, 255) = 0xDCF5FF
        Assert.Equal(0xDCF5FFu, palette.Amber);
        // Accent 2: Copper Compass (195, 115, 75) = 0xC3734B
        Assert.Equal(0xC3734Bu, palette.Orange);
        Assert.Equal(0xFFFFFFu, palette.White);
        Assert.Equal(0x000000u, palette.Black);
    }
    
    [Fact]
    public void WildEcho_HasCorrectColors()
    {
        var palette = Palette.WildEcho;
        
        // Back: Dark Seaweed (15, 30, 25) = 0x0F1E19
        Assert.Equal(0x0F1E19u, palette.Navy);
        // Front: Shore Stone (45, 65, 60) = 0x2D413C
        Assert.Equal(0x2D413Cu, palette.Teal);
        // Detail: Mist Green (85, 115, 105) = 0x557369
        Assert.Equal(0x557369u, palette.SkyBlue);
        // Accent 1: Electric Seafoam (100, 255, 180) = 0x64FFB4
        Assert.Equal(0x64FFB4u, palette.Amber);
        // Accent 2: Sunset Coral (255, 110, 100) = 0xFF6E64
        Assert.Equal(0xFF6E64u, palette.Orange);
        Assert.Equal(0xFFFFFFu, palette.White);
        Assert.Equal(0x000000u, palette.Black);
    }
    
    [Fact]
    public void AvailablePalettes_ContainsAllPalettes()
    {
        var palettes = PaletteService.AvailablePalettes;
        
        Assert.Contains(palettes, p => p.Name == "Default");
        Assert.Contains(palettes, p => p.Name == "Sunset");
        Assert.Contains(palettes, p => p.Name == "Forest");
        Assert.Contains(palettes, p => p.Name == "Neon");
        Assert.Contains(palettes, p => p.Name == "Abyssal Forge");
        Assert.Contains(palettes, p => p.Name == "Northern Archive");
        Assert.Contains(palettes, p => p.Name == "Wild Echo");
    }
}

public class PaletteServiceTests
{
    [Fact]
    public void SetPalette_ChangesPalette()
    {
        var service = PaletteService.Instance;
        var originalPrimary = service.Colors.Primary;
        
        service.SetPalette(Palette.AbyssalForge);
        
        Assert.NotEqual(originalPrimary, service.Colors.Primary);
        
        // Reset to default for other tests
        service.SetPalette(Palette.Default);
    }
    
    [Fact]
    public void SetPaletteByName_ChangesPalette()
    {
        var service = PaletteService.Instance;
        
        var result = service.SetPaletteByName("Wild Echo");
        
        Assert.True(result);
        Assert.Equal(Palette.WildEcho.Navy, service.CurrentPalette.Navy);
        
        // Reset to default
        service.SetPalette(Palette.Default);
    }
    
    [Fact]
    public void SetPaletteByName_InvalidName_ReturnsFalse()
    {
        var service = PaletteService.Instance;
        
        var result = service.SetPaletteByName("NonExistentPalette");
        
        Assert.False(result);
    }
    
    [Fact]
    public void PaletteChanged_EventFires()
    {
        var service = PaletteService.Instance;
        bool eventFired = false;
        
        service.PaletteChanged += (s, e) => eventFired = true;
        service.SetPalette(Palette.NorthernArchive);
        
        Assert.True(eventFired);
        
        // Reset
        service.SetPalette(Palette.Default);
    }
}

