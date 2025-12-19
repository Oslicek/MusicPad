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
    
    /// <summary>
    /// Abyssal Forge - deep ocean and molten metal theme.
    /// </summary>
    public static Palette AbyssalForge { get; } = new Palette(
        skyBlue: 0x3C5A64,  // Detail: Kelp Shadow (60, 90, 100)
        teal: 0x23374B,     // Front: Storm Slate (35, 55, 75)
        navy: 0x0A141E,     // Back: Deep Trench (10, 20, 30)
        amber: 0x00FFCD,    // Accent 1: Bio-Lume Cyan (0, 255, 205)
        orange: 0xFFAA32,   // Accent 2: Molten Amber (255, 170, 50)
        white: 0xFFFFFF,
        black: 0x000000
    );
    
    /// <summary>
    /// Northern Archive - arctic and copper theme.
    /// </summary>
    public static Palette NorthernArchive { get; } = new Palette(
        skyBlue: 0x648296,  // Detail: Frost Glass (100, 130, 150)
        teal: 0x324B5F,     // Front: Arctic Steel (50, 75, 95)
        navy: 0x0F1923,     // Back: Midnight Fjord (15, 25, 35)
        amber: 0xDCF5FF,    // Accent 1: Glacial White (220, 245, 255)
        orange: 0xC3734B,   // Accent 2: Copper Compass (195, 115, 75)
        white: 0xFFFFFF,
        black: 0x000000
    );
    
    /// <summary>
    /// Wild Echo - natural seaweed and coral theme.
    /// </summary>
    public static Palette WildEcho { get; } = new Palette(
        skyBlue: 0x557369,  // Detail: Mist Green (85, 115, 105)
        teal: 0x2D413C,     // Front: Shore Stone (45, 65, 60)
        navy: 0x0F1E19,     // Back: Dark Seaweed (15, 30, 25)
        amber: 0x64FFB4,    // Accent 1: Electric Seafoam (100, 255, 180)
        orange: 0xFF6E64,   // Accent 2: Sunset Coral (255, 110, 100)
        white: 0xFFFFFF,
        black: 0x000000
    );
}

