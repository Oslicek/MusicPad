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

