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
            PadColor = AppColors.PadChromaticNormal,
            PadPressedColor = AppColors.PadChromaticPressed,
            PadAltColor = AppColors.PadChromaticAlt,
            PadAltPressedColor = AppColors.PadChromaticAltPressed
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
            // Neon magenta - pressed is bright yellow for aggressive contrast
            PadColor = AppColors.PadPentatonicNormal,
            PadPressedColor = AppColors.PadPentatonicPressed,
            PadAltColor = AppColors.PadPentatonicAlt,
            PadAltPressedColor = AppColors.PadPentatonicAltPressed
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
            // Warm copper base, halftones darker
            PadColor = AppColors.PadScaleNormal,
            PadPressedColor = AppColors.PadScalePressed,
            PadAltColor = AppColors.PadScaleAlt,
            PadAltPressedColor = AppColors.PadScaleAltPressed
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
            PadColor = AppColors.PianoWhiteKey,
            PadPressedColor = AppColors.PianoWhiteKeyPressed,
            PadAltColor = AppColors.PianoBlackKey,
            PadAltPressedColor = AppColors.PianoBlackKeyPressed
        };
        _padreas.Add(pianoPadrea);
        
        // Pitch-Volume padrea - continuous surface where X = pitch, Y = volume
        var pitchVolumePadrea = new Padrea
        {
            Id = "pitch-volume",
            Name = "Pitch-Volume",
            Description = "Continuous surface: horizontal = pitch, vertical = volume",
            NoteFilter = NoteFilterType.Chromatic,
            Kind = PadreaKind.PitchVolume,
            // Teal for the surface with amber glow on touch
            PadColor = AppColors.Teal,
            PadPressedColor = AppColors.Accent,  // Orange - most aggressive accent
            PadAltColor = AppColors.SkyBlue,
            PadAltPressedColor = AppColors.Amber
        };
        _padreas.Add(pitchVolumePadrea);
        
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

