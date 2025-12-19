namespace MusicPad.Core.Theme;

/// <summary>
/// Represents a color palette with 7 core colors.
/// All other colors in the app are derived from these core colors.
/// </summary>
public class Palette
{
    /// <summary>Light sky blue - light accent, text, highlights</summary>
    public uint SkyBlue { get; }
    
    /// <summary>Teal - primary interactive, buttons, links</summary>
    public uint Teal { get; }
    
    /// <summary>Dark navy - base background, dark surfaces</summary>
    public uint Navy { get; }
    
    /// <summary>Amber - main accent, selected states, knobs</summary>
    public uint Amber { get; }
    
    /// <summary>Orange - secondary accent, pressed states</summary>
    public uint Orange { get; }
    
    /// <summary>Pure white - piano keys, high contrast elements</summary>
    public uint White { get; }
    
    /// <summary>Pure black - shadows, true dark</summary>
    public uint Black { get; }
    
    public Palette(uint skyBlue, uint teal, uint navy, uint amber, uint orange, uint white, uint black)
    {
        SkyBlue = skyBlue;
        Teal = teal;
        Navy = navy;
        Amber = amber;
        Orange = orange;
        White = white;
        Black = black;
    }
    
    /// <summary>
    /// The default palette used by MusicPad.
    /// </summary>
    public static Palette Default { get; } = new Palette(
        skyBlue: 0x8ECAE6,
        teal: 0x219EBC,
        navy: 0x023047,
        amber: 0xFFB703,
        orange: 0xFB8500,
        white: 0xFFFFFF,
        black: 0x000000
    );
    
    /// <summary>
    /// A warm sunset-themed palette.
    /// </summary>
    public static Palette Sunset { get; } = new Palette(
        skyBlue: 0xFFD6A5,  // Peach
        teal: 0xFF6B6B,     // Coral
        navy: 0x2D1B2E,     // Dark purple
        amber: 0xFFC145,    // Gold
        orange: 0xFF4500,   // Red-orange
        white: 0xFFFAF0,    // Floral white
        black: 0x1A0A1A     // Very dark purple
    );
    
    /// <summary>
    /// A cool forest-themed palette.
    /// </summary>
    public static Palette Forest { get; } = new Palette(
        skyBlue: 0xA8D5BA,  // Light sage
        teal: 0x2D6A4F,     // Forest green
        navy: 0x1B2D1B,     // Dark forest
        amber: 0xD4A574,    // Tan/wood
        orange: 0xB85C38,   // Rusty brown
        white: 0xF5F5DC,    // Beige
        black: 0x0D1A0D     // Very dark green
    );
    
    /// <summary>
    /// A purple/pink neon palette.
    /// </summary>
    public static Palette Neon { get; } = new Palette(
        skyBlue: 0xE0AAFF,  // Light purple
        teal: 0xC77DFF,     // Bright purple
        navy: 0x10002B,     // Very dark purple
        amber: 0xFF00FF,    // Magenta
        orange: 0xFF0080,   // Hot pink
        white: 0xFFFFFF,
        black: 0x000000
    );
}

