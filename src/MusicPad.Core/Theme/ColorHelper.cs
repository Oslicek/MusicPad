namespace MusicPad.Core.Theme;

/// <summary>
/// Helper methods for color manipulation.
/// Colors are represented as uint (0xRRGGBB format).
/// </summary>
public static class ColorHelper
{
    /// <summary>
    /// Makes a color lighter by blending it towards white.
    /// </summary>
    /// <param name="color">The base color (0xRRGGBB)</param>
    /// <param name="amount">Amount to lighten (0.0 = no change, 1.0 = white)</param>
    /// <returns>The lightened color</returns>
    public static uint Lighter(uint color, float amount)
    {
        return Mix(color, 0xFFFFFF, amount);
    }
    
    /// <summary>
    /// Makes a color darker by blending it towards black.
    /// </summary>
    /// <param name="color">The base color (0xRRGGBB)</param>
    /// <param name="amount">Amount to darken (0.0 = no change, 1.0 = black)</param>
    /// <returns>The darkened color</returns>
    public static uint Darker(uint color, float amount)
    {
        return Mix(color, 0x000000, amount);
    }
    
    /// <summary>
    /// Mixes two colors together.
    /// </summary>
    /// <param name="color1">First color (0xRRGGBB)</param>
    /// <param name="color2">Second color (0xRRGGBB)</param>
    /// <param name="ratio">Blend ratio (0.0 = color1, 1.0 = color2)</param>
    /// <returns>The blended color</returns>
    public static uint Mix(uint color1, uint color2, float ratio)
    {
        ratio = Math.Clamp(ratio, 0f, 1f);
        
        int r1 = (int)((color1 >> 16) & 0xFF);
        int g1 = (int)((color1 >> 8) & 0xFF);
        int b1 = (int)(color1 & 0xFF);
        
        int r2 = (int)((color2 >> 16) & 0xFF);
        int g2 = (int)((color2 >> 8) & 0xFF);
        int b2 = (int)(color2 & 0xFF);
        
        int r = (int)Math.Round(r1 + (r2 - r1) * ratio);
        int g = (int)Math.Round(g1 + (g2 - g1) * ratio);
        int b = (int)Math.Round(b1 + (b2 - b1) * ratio);
        
        return (uint)((r << 16) | (g << 8) | b);
    }
    
    /// <summary>
    /// Adds an alpha channel to a color and returns as hex string.
    /// </summary>
    /// <param name="color">The base color (0xRRGGBB)</param>
    /// <param name="alpha">Alpha value (0x00 = transparent, 0xFF = opaque)</param>
    /// <returns>Hex string in #AARRGGBB format</returns>
    public static string WithAlpha(uint color, byte alpha)
    {
        return $"#{alpha:X2}{color:X6}";
    }
    
    /// <summary>
    /// Converts a uint color to hex string.
    /// </summary>
    /// <param name="color">The color (0xRRGGBB)</param>
    /// <returns>Hex string in #RRGGBB format</returns>
    public static string ToHex(uint color)
    {
        return $"#{color:X6}";
    }
}

