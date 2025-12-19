namespace MusicPad.Core.Theme;

/// <summary>
/// Manages the active color palette and provides computed colors.
/// </summary>
public class PaletteService
{
    private static PaletteService? _instance;
    private static readonly object _lock = new();
    
    /// <summary>
    /// Gets the singleton instance of the PaletteService.
    /// </summary>
    public static PaletteService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new PaletteService();
                }
            }
            return _instance;
        }
    }
    
    private Palette _currentPalette;
    private ComputedPalette _computedPalette;
    
    /// <summary>
    /// Event raised when the palette changes.
    /// </summary>
    public event EventHandler? PaletteChanged;
    
    private PaletteService()
    {
        _currentPalette = Palette.Default;
        _computedPalette = new ComputedPalette(_currentPalette);
    }
    
    /// <summary>
    /// Gets the current base palette.
    /// </summary>
    public Palette CurrentPalette => _currentPalette;
    
    /// <summary>
    /// Gets the current computed colors.
    /// </summary>
    public ComputedPalette Colors => _computedPalette;
    
    /// <summary>
    /// Gets all available palettes.
    /// </summary>
    public static IReadOnlyList<(string Name, Palette Palette)> AvailablePalettes { get; } = new List<(string, Palette)>
    {
        ("Default", Palette.Default),
        ("Sunset", Palette.Sunset),
        ("Forest", Palette.Forest),
        ("Neon", Palette.Neon)
    };
    
    /// <summary>
    /// Sets the active palette and recomputes all colors.
    /// </summary>
    public void SetPalette(Palette palette)
    {
        _currentPalette = palette;
        _computedPalette = new ComputedPalette(palette);
        PaletteChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Sets the palette by name.
    /// </summary>
    public bool SetPaletteByName(string name)
    {
        var found = AvailablePalettes.FirstOrDefault(p => 
            string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        
        if (found.Palette != null)
        {
            SetPalette(found.Palette);
            return true;
        }
        
        return false;
    }
}

