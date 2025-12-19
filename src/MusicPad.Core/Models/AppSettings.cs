namespace MusicPad.Core.Models;

/// <summary>
/// Application settings that can be persisted and modified by the user.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Whether piano keys should glow following the envelope when playing.
    /// Default: true
    /// </summary>
    public bool PianoKeyGlowEnabled { get; set; } = true;
    
    /// <summary>
    /// Whether square pads should glow following the envelope when playing.
    /// Default: true
    /// </summary>
    public bool PadGlowEnabled { get; set; } = true;
    
    /// <summary>
    /// Creates an independent copy of the settings.
    /// </summary>
    public AppSettings Clone()
    {
        return new AppSettings
        {
            PianoKeyGlowEnabled = PianoKeyGlowEnabled,
            PadGlowEnabled = PadGlowEnabled
        };
    }
}

