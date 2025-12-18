namespace MusicPad.Core.Models;

/// <summary>
/// Types of reverb algorithms.
/// </summary>
public enum ReverbType
{
    /// <summary>Small room reverb - short decay, tight reflections.</summary>
    Room = 0,
    
    /// <summary>Concert hall reverb - medium decay, spacious.</summary>
    Hall = 1,
    
    /// <summary>Plate reverb - bright, dense, metallic character.</summary>
    Plate = 2,
    
    /// <summary>Church/cathedral reverb - very long decay, huge space.</summary>
    Church = 3
}

