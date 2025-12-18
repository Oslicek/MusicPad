namespace MusicPad.Core.Audio;

/// <summary>
/// A simple chorus effect using modulated delay lines.
/// </summary>
public class Chorus
{
    private readonly float _sampleRate;
    private readonly float[] _delayBuffer;
    private int _writeIndex;
    private float _lfoPhase;
    
    private float _depth = 0.5f;
    private float _rate = 0.3f;
    private bool _isEnabled = false;
    
    // Delay parameters (in samples)
    private const float MinDelayMs = 7f;
    private const float MaxDelayMs = 30f;
    private const float MaxDelaySeconds = MaxDelayMs / 1000f;
    
    // LFO rate range (Hz)
    private const float MinRateHz = 0.1f;
    private const float MaxRateHz = 5f;
    
    // Mix ratio (wet/dry)
    private const float WetMix = 0.5f;
    private const float DryMix = 0.7f;

    public Chorus(int sampleRate = 44100)
    {
        _sampleRate = sampleRate;
        
        // Allocate delay buffer for max delay time
        int bufferSize = (int)(MaxDelaySeconds * sampleRate * 2) + 1024;
        _delayBuffer = new float[bufferSize];
        _writeIndex = 0;
        _lfoPhase = 0f;
    }

    /// <summary>
    /// Whether the chorus is active.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    /// <summary>
    /// Normalized depth (0-1). Controls modulation intensity.
    /// </summary>
    public float Depth
    {
        get => _depth;
        set => _depth = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Normalized rate (0-1). Controls LFO speed.
    /// </summary>
    public float Rate
    {
        get => _rate;
        set => _rate = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Process a single sample through the chorus.
    /// </summary>
    public float Process(float input)
    {
        if (!_isEnabled)
            return input;
        
        // Write input to delay buffer
        _delayBuffer[_writeIndex] = input;
        
        // Calculate LFO
        float rateHz = MinRateHz + _rate * (MaxRateHz - MinRateHz);
        float lfoIncrement = rateHz / _sampleRate;
        _lfoPhase += lfoIncrement;
        if (_lfoPhase >= 1f) _lfoPhase -= 1f;
        
        float lfo = MathF.Sin(_lfoPhase * 2f * MathF.PI);
        
        // Calculate modulated delay time
        float baseDelayMs = (MinDelayMs + MaxDelayMs) / 2f;
        float modulationMs = ((MaxDelayMs - MinDelayMs) / 2f) * _depth * lfo;
        float delayMs = baseDelayMs + modulationMs;
        float delaySamples = delayMs * _sampleRate / 1000f;
        
        // Read from delay buffer with linear interpolation
        float readPos = _writeIndex - delaySamples;
        while (readPos < 0) readPos += _delayBuffer.Length;
        
        int readIndex = (int)readPos;
        float frac = readPos - readIndex;
        
        int nextIndex = (readIndex + 1) % _delayBuffer.Length;
        float delayed = _delayBuffer[readIndex] * (1f - frac) + _delayBuffer[nextIndex] * frac;
        
        // Advance write index
        _writeIndex = (_writeIndex + 1) % _delayBuffer.Length;
        
        // Mix dry and wet signals
        return input * DryMix + delayed * WetMix * _depth;
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
    /// Reset the chorus state.
    /// </summary>
    public void Reset()
    {
        Array.Clear(_delayBuffer);
        _writeIndex = 0;
        _lfoPhase = 0f;
    }
}

