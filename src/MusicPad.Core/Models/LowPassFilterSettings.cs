namespace MusicPad.Core.Models;

/// <summary>
/// Settings for a low-pass filter effect.
/// Cutoff controls the filter frequency, Resonance controls the Q factor.
/// </summary>
public class LowPassFilterSettings
{
    private float _cutoff = 1.0f;  // Default: fully open
    private float _resonance = 0.0f;  // Default: no resonance
    private bool _isEnabled = false; // Default: off

    // Frequency range constants (in Hz)
    private const float MinFrequency = 20f;
    private const float MaxFrequency = 20000f;
    
    // Q factor range
    private const float MinQ = 0.707f;  // Butterworth
    private const float MaxQ = 10f;

    /// <summary>
    /// Whether the LPF is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                EnabledChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary>
    /// Event raised when enabled state changes.
    /// </summary>
    public event EventHandler<bool>? EnabledChanged;

    /// <summary>
    /// Normalized cutoff value (0.0 to 1.0).
    /// 0.0 = minimum frequency, 1.0 = maximum frequency (filter open).
    /// </summary>
    public float Cutoff
    {
        get => _cutoff;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(_cutoff - clamped) > float.Epsilon)
            {
                _cutoff = clamped;
                CutoffChanged?.Invoke(this, clamped);
            }
        }
    }

    /// <summary>
    /// Normalized resonance value (0.0 to 1.0).
    /// 0.0 = minimum resonance (Butterworth), 1.0 = maximum resonance.
    /// </summary>
    public float Resonance
    {
        get => _resonance;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(_resonance - clamped) > float.Epsilon)
            {
                _resonance = clamped;
                ResonanceChanged?.Invoke(this, clamped);
            }
        }
    }

    /// <summary>
    /// Event raised when cutoff value changes.
    /// </summary>
    public event EventHandler<float>? CutoffChanged;

    /// <summary>
    /// Event raised when resonance value changes.
    /// </summary>
    public event EventHandler<float>? ResonanceChanged;

    /// <summary>
    /// Converts normalized cutoff to frequency in Hz using logarithmic scale.
    /// </summary>
    public float GetCutoffFrequencyHz()
    {
        // Use logarithmic scale for more musical response
        double logMin = Math.Log(MinFrequency);
        double logMax = Math.Log(MaxFrequency);
        double logFreq = logMin + _cutoff * (logMax - logMin);
        return (float)Math.Exp(logFreq);
    }

    /// <summary>
    /// Converts normalized resonance to Q factor.
    /// </summary>
    public float GetResonanceQ()
    {
        // Linear interpolation from min to max Q
        return MinQ + _resonance * (MaxQ - MinQ);
    }

    /// <summary>
    /// Resets filter to default state (fully open, no resonance).
    /// </summary>
    public void Reset()
    {
        Cutoff = 1.0f;
        Resonance = 0.0f;
    }
}

