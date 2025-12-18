namespace MusicPad.Core.Audio;

/// <summary>
/// A 4-band parametric equalizer using biquad filters.
/// </summary>
public class Equalizer
{
    private readonly float _sampleRate;
    private readonly BiquadFilter[] _bands;
    private readonly float[] _gains = new float[4];
    
    // Band center frequencies
    private static readonly float[] BandFrequencies = { 100f, 500f, 2000f, 8000f };
    
    // Q factor for each band (bandwidth)
    private const float BandQ = 1.5f;

    public Equalizer(int sampleRate = 44100)
    {
        _sampleRate = sampleRate;
        _bands = new BiquadFilter[4];
        for (int i = 0; i < 4; i++)
        {
            _bands[i] = new BiquadFilter();
            UpdateBand(i);
        }
    }

    /// <summary>
    /// Sets the gain for a band (-1 to 1, maps to -12dB to +12dB).
    /// </summary>
    public void SetGain(int band, float normalizedGain)
    {
        if (band < 0 || band >= 4) return;
        _gains[band] = Math.Clamp(normalizedGain, -1f, 1f);
        UpdateBand(band);
    }

    /// <summary>
    /// Gets the gain for a band.
    /// </summary>
    public float GetGain(int band)
    {
        if (band < 0 || band >= 4) return 0f;
        return _gains[band];
    }

    private void UpdateBand(int band)
    {
        float freq = BandFrequencies[band];
        float gainDb = _gains[band] * 12f; // -12 to +12 dB
        _bands[band].SetPeakingEQ(_sampleRate, freq, BandQ, gainDb);
    }

    /// <summary>
    /// Process a single sample through all EQ bands.
    /// </summary>
    public float Process(float input)
    {
        float output = input;
        for (int i = 0; i < 4; i++)
        {
            output = _bands[i].Process(output);
        }
        return output;
    }

    /// <summary>
    /// Process a buffer of samples in-place.
    /// </summary>
    public void Process(float[] buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = Process(buffer[i]);
        }
    }

    /// <summary>
    /// Reset all filter states.
    /// </summary>
    public void Reset()
    {
        foreach (var band in _bands)
        {
            band.Reset();
        }
    }

    /// <summary>
    /// A single biquad filter stage.
    /// </summary>
    private class BiquadFilter
    {
        private float _a0, _a1, _a2, _b1, _b2;
        private float _z1, _z2;

        public BiquadFilter()
        {
            // Default to unity gain (pass-through)
            _a0 = 1f;
            _a1 = 0f;
            _a2 = 0f;
            _b1 = 0f;
            _b2 = 0f;
        }

        /// <summary>
        /// Configure as a peaking EQ filter.
        /// </summary>
        public void SetPeakingEQ(float sampleRate, float freq, float Q, float gainDb)
        {
            // If gain is essentially zero, use pass-through
            if (Math.Abs(gainDb) < 0.1f)
            {
                _a0 = 1f;
                _a1 = 0f;
                _a2 = 0f;
                _b1 = 0f;
                _b2 = 0f;
                return;
            }

            float A = MathF.Pow(10f, gainDb / 40f);
            float w0 = 2f * MathF.PI * freq / sampleRate;
            float cosW0 = MathF.Cos(w0);
            float sinW0 = MathF.Sin(w0);
            float alpha = sinW0 / (2f * Q);

            float b0 = 1f + alpha * A;
            float b1 = -2f * cosW0;
            float b2 = 1f - alpha * A;
            float a0 = 1f + alpha / A;
            float a1 = -2f * cosW0;
            float a2 = 1f - alpha / A;

            // Normalize
            _a0 = b0 / a0;
            _a1 = b1 / a0;
            _a2 = b2 / a0;
            _b1 = a1 / a0;
            _b2 = a2 / a0;
        }

        public float Process(float input)
        {
            // Direct Form II Transposed
            float output = _a0 * input + _z1;
            _z1 = _a1 * input - _b1 * output + _z2;
            _z2 = _a2 * input - _b2 * output;
            return output;
        }

        public void Reset()
        {
            _z1 = 0f;
            _z2 = 0f;
        }
    }
}

