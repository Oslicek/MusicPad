using MusicPad.Core.Models;

namespace MusicPad.Core.Audio;

/// <summary>
/// A Schroeder-style reverb effect with multiple algorithm presets.
/// Uses parallel comb filters followed by series all-pass filters.
/// </summary>
public class Reverb
{
    private readonly float _sampleRate;
    private bool _isEnabled;
    private float _level = 0.3f;
    private ReverbType _type = ReverbType.Room;

    // Comb filter delay lines (parallel)
    private readonly float[][] _combBuffers;
    private readonly int[] _combDelays;
    private readonly int[] _combIndices;
    private readonly float[] _combFeedback;
    private readonly float[] _combDamping;
    private readonly float[] _combFilterState;

    // All-pass filter delay lines (series)
    private readonly float[][] _apBuffers;
    private readonly int[] _apDelays;
    private readonly int[] _apIndices;

    // Base delay times in ms for comb filters (will be scaled by type)
    private static readonly float[] BaseCombDelaysMs = { 29.7f, 37.1f, 41.1f, 43.7f };
    // Base delay times in ms for all-pass filters
    private static readonly float[] BaseApDelaysMs = { 5.0f, 1.7f };
    
    // All-pass coefficient
    private const float ApCoeff = 0.5f;

    public Reverb(int sampleRate = 44100)
    {
        _sampleRate = sampleRate;

        // Initialize comb filters
        _combBuffers = new float[4][];
        _combDelays = new int[4];
        _combIndices = new int[4];
        _combFeedback = new float[4];
        _combDamping = new float[4];
        _combFilterState = new float[4];

        // Allocate max size buffers
        int maxCombSamples = (int)(100f * sampleRate / 1000f); // 100ms max
        for (int i = 0; i < 4; i++)
        {
            _combBuffers[i] = new float[maxCombSamples];
        }

        // Initialize all-pass filters
        _apBuffers = new float[2][];
        _apDelays = new int[2];
        _apIndices = new int[2];

        int maxApSamples = (int)(20f * sampleRate / 1000f); // 20ms max
        for (int i = 0; i < 2; i++)
        {
            _apBuffers[i] = new float[maxApSamples];
        }

        UpdatePreset();
    }

    /// <summary>
    /// Whether the reverb is active.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    /// <summary>
    /// Wet/dry mix level (0-1).
    /// </summary>
    public float Level
    {
        get => _level;
        set => _level = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Reverb algorithm type.
    /// </summary>
    public ReverbType Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                UpdatePreset();
            }
        }
    }

    private void UpdatePreset()
    {
        // Get preset parameters based on type
        float delayScale, feedback, damping;
        
        switch (_type)
        {
            case ReverbType.Room:
                delayScale = 1.0f;
                feedback = 0.75f;  // Shorter decay
                damping = 0.4f;    // More damping
                break;
            case ReverbType.Hall:
                delayScale = 1.3f;
                feedback = 0.85f;  // Medium decay
                damping = 0.3f;
                break;
            case ReverbType.Plate:
                delayScale = 0.9f;
                feedback = 0.88f;  // Dense
                damping = 0.15f;   // Brighter
                break;
            case ReverbType.Church:
            default:
                delayScale = 1.8f;
                feedback = 0.92f;  // Long decay
                damping = 0.25f;
                break;
        }

        // Update comb filter delays
        for (int i = 0; i < 4; i++)
        {
            float delayMs = BaseCombDelaysMs[i] * delayScale;
            _combDelays[i] = Math.Min((int)(delayMs * _sampleRate / 1000f), _combBuffers[i].Length - 1);
            _combFeedback[i] = feedback;
            _combDamping[i] = damping;
        }

        // Update all-pass delays
        for (int i = 0; i < 2; i++)
        {
            float delayMs = BaseApDelaysMs[i] * delayScale;
            _apDelays[i] = Math.Min((int)(delayMs * _sampleRate / 1000f), _apBuffers[i].Length - 1);
            _apDelays[i] = Math.Max(_apDelays[i], 1); // Ensure at least 1 sample delay
        }
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
            float input = buffer[i];
            
            // Sum output from parallel comb filters
            float combSum = 0f;
            for (int c = 0; c < 4; c++)
            {
                combSum += ProcessComb(c, input);
            }
            combSum *= 0.25f; // Average

            // Process through series all-pass filters
            float output = combSum;
            for (int a = 0; a < 2; a++)
            {
                output = ProcessAllPass(a, output);
            }

            // Mix dry and wet
            buffer[i] = input * (1f - _level) + output * _level;
        }
    }

    private float ProcessComb(int index, float input)
    {
        var buffer = _combBuffers[index];
        int delay = _combDelays[index];
        ref int writeIdx = ref _combIndices[index];
        
        // Read from delay line
        int readIdx = writeIdx - delay;
        if (readIdx < 0) readIdx += buffer.Length;
        float delayed = buffer[readIdx];

        // Apply damping (low-pass filter on feedback)
        ref float filterState = ref _combFilterState[index];
        float damping = _combDamping[index];
        filterState = delayed * (1f - damping) + filterState * damping;

        // Write to delay line with feedback
        float feedback = _combFeedback[index];
        buffer[writeIdx] = input + filterState * feedback;
        
        // Soft clip to prevent runaway
        buffer[writeIdx] = Math.Clamp(buffer[writeIdx], -2f, 2f);

        // Advance write index
        writeIdx = (writeIdx + 1) % buffer.Length;

        return delayed;
    }

    private float ProcessAllPass(int index, float input)
    {
        var buffer = _apBuffers[index];
        int delay = _apDelays[index];
        ref int writeIdx = ref _apIndices[index];

        // Read from delay line
        int readIdx = writeIdx - delay;
        if (readIdx < 0) readIdx += buffer.Length;
        float delayed = buffer[readIdx];

        // All-pass formula: y = -g*x + x_delayed + g*y_delayed
        float output = -ApCoeff * input + delayed;
        buffer[writeIdx] = input + ApCoeff * delayed;

        // Advance write index
        writeIdx = (writeIdx + 1) % buffer.Length;

        return output;
    }

    /// <summary>
    /// Reset reverb state (clears all delay lines).
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < 4; i++)
        {
            Array.Clear(_combBuffers[i]);
            _combIndices[i] = 0;
            _combFilterState[i] = 0f;
        }

        for (int i = 0; i < 2; i++)
        {
            Array.Clear(_apBuffers[i]);
            _apIndices[i] = 0;
        }
    }
}

