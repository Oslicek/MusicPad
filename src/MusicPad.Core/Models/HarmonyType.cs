namespace MusicPad.Core.Models;

/// <summary>
/// Types of auto harmony.
/// </summary>
public enum HarmonyType
{
    /// <summary>Adds note one octave up (+12 semitones).</summary>
    Octave = 0,
    
    /// <summary>Adds perfect fifth (+7 semitones) - power chord.</summary>
    Fifth = 1,
    
    /// <summary>Adds major third and fifth (+4, +7) - major triad.</summary>
    Major = 2,
    
    /// <summary>Adds minor third and fifth (+3, +7) - minor triad.</summary>
    Minor = 3
}

