namespace MusicPad.Core.Sfz;

/// <summary>
/// Represents a single region in an SFZ instrument.
/// A region defines how a sample should be played for a range of keys/velocities.
/// </summary>
public class SfzRegion
{
    // Sample reference
    public string Sample { get; set; } = string.Empty;
    public int Offset { get; set; }
    public int End { get; set; }
    
    // Key range
    public int LoKey { get; set; } = 0;
    public int HiKey { get; set; } = 127;
    public int? Key { get; set; } // Single key (sets both lokey and hikey)
    public int PitchKeycenter { get; set; } = 60; // Root key of the sample
    
    // Velocity range
    public int LoVel { get; set; } = 0;
    public int HiVel { get; set; } = 127;
    
    // Loop settings
    public int LoopStart { get; set; }
    public int LoopEnd { get; set; }
    public LoopMode LoopMode { get; set; } = LoopMode.NoLoop;
    
    // Tuning
    public int Tune { get; set; } // Fine tuning in cents
    public int Transpose { get; set; } // Transposition in semitones
    
    // Volume and pan
    public float Volume { get; set; } // Volume adjustment in dB
    public float Pan { get; set; } // -100 (left) to 100 (right)
    
    // Envelope (AHDSR)
    public float AmpegAttack { get; set; } = 0.001f;
    public float AmpegHold { get; set; }
    public float AmpegDecay { get; set; }
    public float AmpegSustain { get; set; } = 100f;
    public float AmpegRelease { get; set; } = 0.001f;
    
    // Label for debugging
    public string? RegionLabel { get; set; }

    /// <summary>
    /// Checks if this region matches a given MIDI note and velocity.
    /// </summary>
    public bool Matches(int midiNote, int velocity = 100)
    {
        int lo = Key ?? LoKey;
        int hi = Key ?? HiKey;
        return midiNote >= lo && midiNote <= hi && velocity >= LoVel && velocity <= HiVel;
    }

    /// <summary>
    /// Calculates the playback pitch ratio for a given MIDI note.
    /// </summary>
    public double GetPitchRatio(int midiNote)
    {
        int semitones = midiNote - PitchKeycenter + Transpose;
        double cents = semitones * 100.0 + Tune;
        return Math.Pow(2.0, cents / 1200.0);
    }
}

public enum LoopMode
{
    NoLoop,
    LoopContinuous,
    LoopSustain,
    OneShot
}

