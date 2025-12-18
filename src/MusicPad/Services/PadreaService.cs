using MusicPad.Core.Models;

namespace MusicPad.Services;

/// <summary>
/// In-memory implementation of padrea service.
/// </summary>
public class PadreaService : IPadreaService
{
    private readonly List<Padrea> _padreas = new();
    
    public IReadOnlyList<Padrea> AvailablePadreas => _padreas;
    
    public Padrea? CurrentPadrea { get; set; }

    public PadreaService()
    {
        // Create default chromatic padrea with contrasting pressed colors
        var fullRangePadrea = new Padrea
        {
            Id = "full-range",
            Name = "Full Range",
            Description = "Shows all chromatic notes from the instrument's range",
            NoteFilter = NoteFilterType.Chromatic,
            Columns = 6,           // 6 pads per row (half octave)
            RowsPerViewpage = 4,   // 4 rows = 24 notes = 2 octaves per page
            // Teal/cyan - pressed is bright white for aggressive contrast
            PadColor = "#4ECDC4",
            PadPressedColor = "#FFFFFF",      // Bright white - aggressive contrast
            PadAltColor = "#E8A838",
            PadAltPressedColor = "#FF0066"    // Hot pink - aggressive contrast
        };
        _padreas.Add(fullRangePadrea);
        
        // Create pentatonic padrea with neon colors and aggressive pressed states
        var pentatonicPadrea = new Padrea
        {
            Id = "pentatonic",
            Name = "Pentatonic",
            Description = "Major pentatonic scale - 5 notes per octave, 5 octaves per page",
            NoteFilter = NoteFilterType.PentatonicMajor,
            Columns = 5,           // 5 pads per row
            RowsPerViewpage = 5,   // 5 rows per viewpage = 5 octaves
            // Neon magenta - pressed is bright white/yellow for aggressive contrast
            PadColor = "#FF00FF",
            PadPressedColor = "#FFFF00",      // Bright yellow - aggressive contrast
            PadAltColor = "#00FFFF",
            PadAltPressedColor = "#FF3300"    // Hot orange-red - aggressive contrast
        };
        _padreas.Add(pentatonicPadrea);

        // Heptatonic scales padrea (default C Major)
        var scalesPadrea = new Padrea
        {
            Id = "scales",
            Name = "Scales",
            Description = "Select a heptatonic scale (Major, Minor modes, etc.)",
            NoteFilter = NoteFilterType.HeptatonicScale,
            ScaleType = ScaleType.Major,
            RootNote = 0, // C
            Columns = 7,           // 7 pads per row (one octave of scale)
            RowsPerViewpage = 4,   // 4 rows = 28 notes = 4 octaves per page
            Kind = PadreaKind.Grid,
            // Warm base, halftones highlighted
            PadColor = "#CD8B5A",
            PadPressedColor = "#F4B27A",
            PadAltColor = "#8B5A3A",          // Halftones darker
            PadAltPressedColor = "#FF9966"
        };
        _padreas.Add(scalesPadrea);

        // Piano padrea - chromatic keyboard
        var pianoPadrea = new Padrea
        {
            Id = "piano",
            Name = "Piano",
            Description = "Chromatic piano keyboard view",
            NoteFilter = NoteFilterType.Chromatic,
            Kind = PadreaKind.Piano,
            PadColor = "#FFFFFF",
            PadPressedColor = "#FFDD88",
            PadAltColor = "#666666",
            PadAltPressedColor = "#FFAA55"
        };
        _padreas.Add(pianoPadrea);
        
        CurrentPadrea = fullRangePadrea;
    }

    public Padrea CreatePadrea(string name)
    {
        var padrea = new Padrea
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name
        };
        _padreas.Add(padrea);
        return padrea;
    }

    public bool DeletePadrea(string id)
    {
        // Don't allow deleting the default padrea
        if (id == "default")
            return false;
            
        var padrea = _padreas.FirstOrDefault(p => p.Id == id);
        if (padrea == null)
            return false;
            
        _padreas.Remove(padrea);
        
        // If we deleted the current padrea, switch to default
        if (CurrentPadrea?.Id == id)
        {
            CurrentPadrea = _padreas.FirstOrDefault(p => p.Id == "default");
        }
        
        return true;
    }
}

