namespace MusicPad.Core.Models;

/// <summary>
/// Arpeggiator pattern types.
/// </summary>
public enum ArpPattern
{
    /// <summary>Play notes from lowest to highest.</summary>
    Up = 0,
    
    /// <summary>Play notes from highest to lowest.</summary>
    Down = 1,
    
    /// <summary>Play notes up then down (ping-pong).</summary>
    UpDown = 2,
    
    /// <summary>Play notes in random order.</summary>
    Random = 3
}

