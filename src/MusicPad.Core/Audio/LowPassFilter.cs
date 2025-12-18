namespace MusicPad.Core.Audio;

/// <summary>
/// A resonant low-pass filter using a simple IIR biquad design.
/// </summary>
public class LowPassFilter
{
    private float _cutoff = 1.0f;
    private float _resonance = 0.0f;
    private bool _isEnabled = false;
    
    // Filter state
    private float _z1 = 0f;
    private float _z2 = 0f;
    private readonly float _sampleRate;
    
    // Biquad coefficients
    private float _a0, _a1, _a2, _b1, _b2;

    public LowPassFilter(int sampleRate = 44100)
    {
        _sampleRate = sampleRate;
        UpdateCoefficients();
    }

    /// <summary>
    /// Whether the filter is active.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    /// <summary>
    /// Normalized cutoff (0-1). 0 = min freq, 1 = max freq.
    /// </summary>
    public float Cutoff
    {
        get => _cutoff;
        set
        {
            _cutoff = Math.Clamp(value, 0f, 1f);
            UpdateCoefficients();
        }
    }

    /// <summary>
    /// Normalized resonance (0-1). 0 = no resonance, 1 = max resonance.
    /// </summary>
    public float Resonance
    {
        get => _resonance;
        set
        {
            _resonance = Math.Clamp(value, 0f, 1f);
            UpdateCoefficients();
        }
    }

    private void UpdateCoefficients()
    {
        // Convert normalized cutoff to frequency (20Hz - 20kHz, logarithmic)
        float minFreq = 20f;
        float maxFreq = Math.Min(20000f, _sampleRate * 0.45f);
        float logMin = MathF.Log(minFreq);
        float logMax = MathF.Log(maxFreq);
        float freq = MathF.Exp(logMin + _cutoff * (logMax - logMin));
        
        // Q factor: 0.707 (Butterworth) to 10 (very resonant)
        float qMin = 0.707f;
        float qMax = 10f;
        float Q = qMin + _resonance * (qMax - qMin);
        
        // Biquad LPF coefficients (RBJ Audio EQ Cookbook)
        float w0 = 2f * MathF.PI * freq / _sampleRate;
        float cosW0 = MathF.Cos(w0);
        float sinW0 = MathF.Sin(w0);
        float alpha = sinW0 / (2f * Q);
        
        float b0 = (1f - cosW0) / 2f;
        float b1 = 1f - cosW0;
        float b2 = (1f - cosW0) / 2f;
        float a0 = 1f + alpha;
        float a1 = -2f * cosW0;
        float a2 = 1f - alpha;
        
        // Normalize
        _a0 = b0 / a0;
        _a1 = b1 / a0;
        _a2 = b2 / a0;
        _b1 = a1 / a0;
        _b2 = a2 / a0;
    }

    /// <summary>
    /// Process a single sample through the filter.
    /// </summary>
    public float Process(float input)
    {
        if (!_isEnabled)
            return input;
        
        // Direct Form II Transposed
        float output = _a0 * input + _z1;
        _z1 = _a1 * input - _b1 * output + _z2;
        _z2 = _a2 * input - _b2 * output;
        
        return output;
    }

    /// <summary>
    /// Process a buffer of samples in-place.
    /// </summary>
    public void Process(float[] buffer)
    {
        if (!_isEnabled)
            return;
        
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = Process(buffer[i]);
        }
    }

    /// <summary>
    /// Reset filter state (call when changing instruments or stopping playback).
    /// </summary>
    public void Reset()
    {
        _z1 = 0f;
        _z2 = 0f;
    }
}
