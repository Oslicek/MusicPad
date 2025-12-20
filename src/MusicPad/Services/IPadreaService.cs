using MusicPad.Core.Models;

namespace MusicPad.Services;

/// <summary>
/// Service for managing pad areas (padreas).
/// </summary>
public interface IPadreaService
{
    /// <summary>
    /// Gets all available padreas.
    /// </summary>
    IReadOnlyList<Padrea> AvailablePadreas { get; }
    
    /// <summary>
    /// Gets or sets the currently selected padrea.
    /// </summary>
    Padrea? CurrentPadrea { get; set; }
    
    /// <summary>
    /// Gets padreas available for the specified pitch type.
    /// Pitched instruments get all padreas except unpitched.
    /// Unpitched instruments get only the unpitched padrea.
    /// </summary>
    IReadOnlyList<Padrea> GetPadreasForPitchType(PitchType pitchType);
    
    /// <summary>
    /// Creates a new padrea.
    /// </summary>
    Padrea CreatePadrea(string name);
    
    /// <summary>
    /// Deletes a padrea by ID.
    /// </summary>
    bool DeletePadrea(string id);
}


