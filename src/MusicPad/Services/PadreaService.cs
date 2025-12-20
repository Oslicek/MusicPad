using MusicPad.Core.Models;
using MusicPad.Core.Theme;

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
        // Create default chromatic padrea - colors come from dynamic palette
        var fullRangePadrea = new Padrea
        {
            Id = "full-range",
            Name = "Full Range",
            Description = "Shows all chromatic notes from the instrument's range",
            NoteFilter = NoteFilterType.Chromatic,
            Columns = 6,           // 6 pads per row (half octave)
            RowsPerViewpage = 6    // 6 rows = 36 notes = 3 octaves per page
            // Colors are null - drawable will use dynamic palette defaults
        };
        _padreas.Add(fullRangePadrea);
        
        // Create pentatonic padrea - colors come from dynamic palette
        var pentatonicPadrea = new Padrea
        {
            Id = "pentatonic",
            Name = "Pentatonic",
            Description = "Major pentatonic scale - 5 notes per octave, 5 octaves per page",
            NoteFilter = NoteFilterType.PentatonicMajor,
            Columns = 5,           // 5 pads per row
            RowsPerViewpage = 5    // 5 rows per viewpage = 5 octaves
            // Colors are null - drawable will use dynamic palette defaults
        };
        _padreas.Add(pentatonicPadrea);

        // Heptatonic scales padrea (default C Major) - colors come from dynamic palette
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
            Kind = PadreaKind.Grid
            // Colors are null - drawable will use dynamic palette defaults
        };
        _padreas.Add(scalesPadrea);

        // Piano padrea - chromatic keyboard
        var pianoPadrea = new Padrea
        {
            Id = "piano",
            Name = "Piano",
            Description = "Chromatic piano keyboard view",
            NoteFilter = NoteFilterType.Chromatic,
            Kind = PadreaKind.Piano
            // Colors are null - drawable will use dynamic palette defaults
        };
        _padreas.Add(pianoPadrea);
        
        // Pitch-Volume padrea - continuous surface where X = pitch, Y = volume
        var pitchVolumePadrea = new Padrea
        {
            Id = "pitch-volume",
            Name = "Pitch-Volume",
            Description = "Continuous surface: horizontal = pitch, vertical = volume",
            NoteFilter = NoteFilterType.Chromatic,
            Kind = PadreaKind.PitchVolume
            // Colors are null - drawable will use dynamic palette defaults
        };
        _padreas.Add(pitchVolumePadrea);
        
        // Unpitched padrea - for drums/percussion
        var unpitchedPadrea = new Padrea
        {
            Id = "unpitched",
            Name = "Unpitched",
            Description = "Drum/percussion pads - each pad triggers a different sound",
            NoteFilter = NoteFilterType.Chromatic,
            Kind = PadreaKind.Unpitched,
            Columns = 4,           // 4 pads per row
            RowsPerViewpage = 4    // 4 rows = 16 pads per page
            // Colors are null - drawable will use dynamic palette defaults
        };
        _padreas.Add(unpitchedPadrea);
        
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
    
    public IReadOnlyList<Padrea> GetPadreasForPitchType(PitchType pitchType)
    {
        if (pitchType == PitchType.Unpitched)
        {
            // Unpitched instruments only get the unpitched padrea
            return _padreas.Where(p => p.Kind == PadreaKind.Unpitched).ToList();
        }
        else
        {
            // Pitched instruments get all padreas except unpitched
            return _padreas.Where(p => p.Kind != PadreaKind.Unpitched).ToList();
        }
    }
}

