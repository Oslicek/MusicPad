using MusicPad.Core.Theme;

namespace MusicPad.Tests.Theme;

public class ColorHelperTests
{
    #region Lighter Tests
    
    [Fact]
    public void Lighter_WithBlack_ReturnsLighterColor()
    {
        // Black = 0x000000
        uint black = 0x000000;
        
        uint result = ColorHelper.Lighter(black, 0.5f);
        
        // Should be gray (halfway to white)
        Assert.Equal(0x808080u, result);
    }
    
    [Fact]
    public void Lighter_WithWhite_ReturnsWhite()
    {
        uint white = 0xFFFFFF;
        
        uint result = ColorHelper.Lighter(white, 0.5f);
        
        // White can't get lighter
        Assert.Equal(0xFFFFFFu, result);
    }
    
    [Fact]
    public void Lighter_WithZeroAmount_ReturnsSameColor()
    {
        uint navy = 0x023047;
        
        uint result = ColorHelper.Lighter(navy, 0f);
        
        Assert.Equal(navy, result);
    }
    
    [Fact]
    public void Lighter_WithFullAmount_ReturnsWhite()
    {
        uint navy = 0x023047;
        
        uint result = ColorHelper.Lighter(navy, 1f);
        
        Assert.Equal(0xFFFFFFu, result);
    }
    
    #endregion
    
    #region Darker Tests
    
    [Fact]
    public void Darker_WithWhite_ReturnsDarkerColor()
    {
        uint white = 0xFFFFFF;
        
        uint result = ColorHelper.Darker(white, 0.5f);
        
        // Should be gray (halfway to black)
        Assert.Equal(0x808080u, result);
    }
    
    [Fact]
    public void Darker_WithBlack_ReturnsBlack()
    {
        uint black = 0x000000;
        
        uint result = ColorHelper.Darker(black, 0.5f);
        
        // Black can't get darker
        Assert.Equal(0x000000u, result);
    }
    
    [Fact]
    public void Darker_WithZeroAmount_ReturnsSameColor()
    {
        uint teal = 0x219EBC;
        
        uint result = ColorHelper.Darker(teal, 0f);
        
        Assert.Equal(teal, result);
    }
    
    [Fact]
    public void Darker_WithFullAmount_ReturnsBlack()
    {
        uint teal = 0x219EBC;
        
        uint result = ColorHelper.Darker(teal, 1f);
        
        Assert.Equal(0x000000u, result);
    }
    
    #endregion
    
    #region Mix Tests
    
    [Fact]
    public void Mix_TwoColors_Half_ReturnsAverage()
    {
        uint black = 0x000000;
        uint white = 0xFFFFFF;
        
        uint result = ColorHelper.Mix(black, white, 0.5f);
        
        // Should be gray
        Assert.Equal(0x808080u, result);
    }
    
    [Fact]
    public void Mix_WithZeroRatio_ReturnsFirstColor()
    {
        uint navy = 0x023047;
        uint amber = 0xFFB703;
        
        uint result = ColorHelper.Mix(navy, amber, 0f);
        
        Assert.Equal(navy, result);
    }
    
    [Fact]
    public void Mix_WithFullRatio_ReturnsSecondColor()
    {
        uint navy = 0x023047;
        uint amber = 0xFFB703;
        
        uint result = ColorHelper.Mix(navy, amber, 1f);
        
        Assert.Equal(amber, result);
    }
    
    #endregion
    
    #region WithAlpha Tests
    
    [Fact]
    public void WithAlpha_AddsAlphaChannel()
    {
        uint teal = 0x219EBC;
        
        string result = ColorHelper.WithAlpha(teal, 0x40);
        
        Assert.Equal("#40219EBC", result);
    }
    
    [Fact]
    public void WithAlpha_FullOpacity()
    {
        uint navy = 0x023047;
        
        string result = ColorHelper.WithAlpha(navy, 0xFF);
        
        Assert.Equal("#FF023047", result);
    }
    
    [Fact]
    public void WithAlpha_ZeroOpacity()
    {
        uint white = 0xFFFFFF;
        
        string result = ColorHelper.WithAlpha(white, 0x00);
        
        Assert.Equal("#00FFFFFF", result);
    }
    
    #endregion
    
    #region ToHex Tests
    
    [Fact]
    public void ToHex_ConvertsToString()
    {
        uint navy = 0x023047;
        
        string result = ColorHelper.ToHex(navy);
        
        Assert.Equal("#023047", result);
    }
    
    [Fact]
    public void ToHex_PreservesLeadingZeros()
    {
        uint darkColor = 0x010203;
        
        string result = ColorHelper.ToHex(darkColor);
        
        Assert.Equal("#010203", result);
    }
    
    #endregion
}

