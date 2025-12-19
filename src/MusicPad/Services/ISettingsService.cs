using MusicPad.Core.Models;

namespace MusicPad.Services;

/// <summary>
/// Service for managing application settings with persistence.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    AppSettings Settings { get; }
    
    /// <summary>
    /// Whether piano key glow effect is enabled.
    /// </summary>
    bool PianoKeyGlowEnabled { get; set; }
    
    /// <summary>
    /// Whether square pad glow effect is enabled.
    /// </summary>
    bool PadGlowEnabled { get; set; }
    
    /// <summary>
    /// The name of the currently selected color palette.
    /// </summary>
    string SelectedPalette { get; set; }
    
    /// <summary>
    /// Saves settings to persistent storage.
    /// </summary>
    void Save();
    
    /// <summary>
    /// Loads settings from persistent storage.
    /// </summary>
    void Load();
}

