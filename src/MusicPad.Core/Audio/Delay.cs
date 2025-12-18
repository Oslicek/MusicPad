namespace MusicPad.Core.Audio;

/// <summary>
/// A simple delay effect with feedback.
/// </summary>
public class Delay
{
    private readonly float _sampleRate;
    private readonly float[] _delayBuffer;
    private int _writeIndex;
    
    private float _time = 0.4f;
    private float _feedback = 0.4f;
    private float _level = 0.5f;
    private bool _isEnabled = false;
    
    // Delay time range
    private const float MinDelayMs = 50f;
    private const float MaxDelayMs = 1000f;
    private const float MaxDelaySeconds = MaxDelayMs / 1000f;

    public Delay(int sampleRate = 44100)
    {
        _sampleRate = sampleRate;
        
        // Allocate buffer for max delay time
        int bufferSize = (int)(MaxDelaySeconds * sampleRate) + 1024;
        _delayBuffer = new float[bufferSize];
        _writeIndex = 0;
    }

    /// <summary>
    /// Whether the delay is active.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    /// <summary>
    /// Normalized delay time (0-1). Maps to 50ms - 1000ms.
    /// </summary>
    public float Time
    {
        get => _time;
        set => _time = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Feedback amount (0-1). Higher = more repeats.
    /// </summary>
    public float Feedback
    {
        get => _feedback;
        set => _feedback = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Wet signal level (0-1).
    /// </summary>
    public float Level
    {
        get => _level;
        set => _level = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Process a single sample through the delay.
    /// </summary>
    public float Process(float input)
    {
        if (!_isEnabled)
            return input;
        
        // Calculate delay time in samples
        float delayMs = MinDelayMs + _time * (MaxDelayMs - MinDelayMs);
        float delaySamples = delayMs * _sampleRate / 1000f;
        
        // Read from delay buffer with linear interpolation
        float readPos = _writeIndex - delaySamples;
        while (readPos < 0) readPos += _delayBuffer.Length;
        
        int readIndex = (int)readPos;
        float frac = readPos - readIndex;
        
        int nextIndex = (readIndex + 1) % _delayBuffer.Length;
        float delayed = _delayBuffer[readIndex] * (1f - frac) + _delayBuffer[nextIndex] * frac;
        
        // Write to delay buffer: input + feedback
        float feedbackAmount = _feedback * 0.9f; // Cap feedback to prevent runaway
        _delayBuffer[_writeIndex] = input + delayed * feedbackAmount;
        
        // Soft clip the buffer to prevent explosion
        _delayBuffer[_writeIndex] = Math.Clamp(_delayBuffer[_writeIndex], -2f, 2f);
        
        // Advance write index
        _writeIndex = (_writeIndex + 1) % _delayBuffer.Length;
        
        // Mix dry and wet
        return input + delayed * _level;
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
    /// Reset the delay state.
    /// </summary>
    public void Reset()
    {
        Array.Clear(_delayBuffer);
        _writeIndex = 0;
    }
}

