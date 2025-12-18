namespace MusicPad.Core.Models;

/// <summary>
/// Settings for a 4-band parametric equalizer.
/// Each band can boost or cut by ±12dB.
/// </summary>
public class EqualizerSettings
{
    private float _lowGain;
    private float _lowMidGain;
    private float _highMidGain;
    private float _highGain;

    /// <summary>
    /// Number of EQ bands.
    /// </summary>
    public const int BandCount = 4;

    /// <summary>
    /// Maximum boost/cut in decibels.
    /// </summary>
    public const float MaxDecibelRange = 12f;

    // Band center frequencies in Hz
    private static readonly float[] BandFrequencies = { 100f, 500f, 2000f, 8000f };
    private static readonly string[] BandNames = { "Low", "Low Mid", "High Mid", "High" };

    /// <summary>
    /// Low band gain (-1.0 to 1.0, maps to -12dB to +12dB).
    /// </summary>
    public float LowGain
    {
        get => _lowGain;
        set => SetBandGainInternal(0, ref _lowGain, value);
    }

    /// <summary>
    /// Low-mid band gain (-1.0 to 1.0, maps to -12dB to +12dB).
    /// </summary>
    public float LowMidGain
    {
        get => _lowMidGain;
        set => SetBandGainInternal(1, ref _lowMidGain, value);
    }

    /// <summary>
    /// High-mid band gain (-1.0 to 1.0, maps to -12dB to +12dB).
    /// </summary>
    public float HighMidGain
    {
        get => _highMidGain;
        set => SetBandGainInternal(2, ref _highMidGain, value);
    }

    /// <summary>
    /// High band gain (-1.0 to 1.0, maps to -12dB to +12dB).
    /// </summary>
    public float HighGain
    {
        get => _highGain;
        set => SetBandGainInternal(3, ref _highGain, value);
    }

    /// <summary>
    /// Event raised when any band's gain changes.
    /// </summary>
    public event EventHandler<BandChangedEventArgs>? BandChanged;

    /// <summary>
    /// Gets the gain for a specific band by index.
    /// </summary>
    public float GetGain(int bandIndex)
    {
        return bandIndex switch
        {
            0 => _lowGain,
            1 => _lowMidGain,
            2 => _highMidGain,
            3 => _highGain,
            _ => throw new ArgumentOutOfRangeException(nameof(bandIndex), "Band index must be 0-3")
        };
    }

    /// <summary>
    /// Sets the gain for a specific band by index.
    /// </summary>
    public void SetGain(int bandIndex, float value)
    {
        switch (bandIndex)
        {
            case 0: LowGain = value; break;
            case 1: LowMidGain = value; break;
            case 2: HighMidGain = value; break;
            case 3: HighGain = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(bandIndex), "Band index must be 0-3");
        }
    }

    /// <summary>
    /// Gets the center frequency for a band in Hz.
    /// </summary>
    public static float GetBandCenterFrequency(int bandIndex)
    {
        if (bandIndex < 0 || bandIndex >= BandCount)
            throw new ArgumentOutOfRangeException(nameof(bandIndex));
        return BandFrequencies[bandIndex];
    }

    /// <summary>
    /// Gets the display name for a band.
    /// </summary>
    public static string GetBandName(int bandIndex)
    {
        if (bandIndex < 0 || bandIndex >= BandCount)
            throw new ArgumentOutOfRangeException(nameof(bandIndex));
        return BandNames[bandIndex];
    }

    /// <summary>
    /// Converts normalized gain (-1 to 1) to decibels (-12 to +12).
    /// </summary>
    public static float GainToDecibels(float normalizedGain)
    {
        return normalizedGain * MaxDecibelRange;
    }

    /// <summary>
    /// Converts decibels to normalized gain.
    /// </summary>
    public static float DecibelsToGain(float decibels)
    {
        return Math.Clamp(decibels / MaxDecibelRange, -1f, 1f);
    }

    /// <summary>
    /// Resets all bands to flat (0 gain).
    /// </summary>
    public void Reset()
    {
        _lowGain = 0f;
        _lowMidGain = 0f;
        _highMidGain = 0f;
        _highGain = 0f;
        
        // Fire events for all bands
        BandChanged?.Invoke(this, new BandChangedEventArgs(0, 0f));
        BandChanged?.Invoke(this, new BandChangedEventArgs(1, 0f));
        BandChanged?.Invoke(this, new BandChangedEventArgs(2, 0f));
        BandChanged?.Invoke(this, new BandChangedEventArgs(3, 0f));
    }

    private void SetBandGainInternal(int bandIndex, ref float field, float value)
    {
        var clamped = Math.Clamp(value, -1f, 1f);
        if (Math.Abs(field - clamped) > float.Epsilon)
        {
            field = clamped;
            BandChanged?.Invoke(this, new BandChangedEventArgs(bandIndex, clamped));
        }
    }
}

/// <summary>
/// Event args for EQ band changes.
/// </summary>
public class BandChangedEventArgs : EventArgs
{
    public int BandIndex { get; }
    public float NewGain { get; }

    public BandChangedEventArgs(int bandIndex, float newGain)
    {
        BandIndex = bandIndex;
        NewGain = newGain;
    }
}

