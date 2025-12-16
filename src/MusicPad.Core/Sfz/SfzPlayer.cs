namespace MusicPad.Core.Sfz;

/// <summary>
/// Plays SFZ instruments by generating audio samples.
/// Currently supports single-voice (monophonic) playback.
/// </summary>
public class SfzPlayer
{
    private readonly int _sampleRate;
    private SfzInstrument? _instrument;
    private Voice? _activeVoice;
    private readonly object _lock = new();

    public SfzPlayer(int sampleRate)
    {
        _sampleRate = sampleRate;
    }

    /// <summary>
    /// Loads an SFZ instrument for playback.
    /// </summary>
    public void LoadInstrument(SfzInstrument instrument)
    {
        lock (_lock)
        {
            _instrument = instrument;
            _activeVoice = null; // Clear any active notes
        }
    }

    /// <summary>
    /// Triggers a note-on event.
    /// </summary>
    public void NoteOn(int midiNote, int velocity = 100)
    {
        lock (_lock)
        {
            if (_instrument == null)
                return;

            // Find matching regions
            var regions = _instrument.FindRegions(midiNote, velocity).ToList();
            if (regions.Count == 0)
                return;

            // For now, just use the first matching region (mono playback)
            var region = regions[0];
            
            // Get sample data
            float[]? samples = GetSamplesForRegion(region);
            if (samples == null || samples.Length == 0)
                return;

            // Calculate pitch ratio
            double pitchRatio = region.GetPitchRatio(midiNote);

            // Calculate envelope parameters in samples
            int attackSamples = Math.Max(1, (int)(_sampleRate * region.AmpegAttack));
            int releaseSamples = Math.Max(1, (int)(_sampleRate * region.AmpegRelease));
            float sustainLevel = region.AmpegSustain / 100f;

            // Create voice
            _activeVoice = new Voice
            {
                Samples = samples,
                Position = region.Offset,
                EndPosition = region.End > 0 ? region.End : samples.Length - 1,
                PitchRatio = pitchRatio,
                Volume = DbToLinear(region.Volume),
                AttackSamples = attackSamples,
                ReleaseSamples = releaseSamples,
                SustainLevel = sustainLevel,
                EnvelopePhase = EnvelopePhase.Attack,
                EnvelopePosition = 0,
                EnvelopeLevel = 0
            };
        }
    }

    /// <summary>
    /// Triggers a note-off event.
    /// </summary>
    public void NoteOff(int midiNote)
    {
        lock (_lock)
        {
            if (_activeVoice != null && _activeVoice.EnvelopePhase != EnvelopePhase.Release)
            {
                _activeVoice.EnvelopePhase = EnvelopePhase.Release;
                _activeVoice.EnvelopePosition = 0;
                _activeVoice.ReleaseStartLevel = _activeVoice.EnvelopeLevel;
            }
        }
    }

    /// <summary>
    /// Generates audio samples into the provided buffer.
    /// </summary>
    public void GenerateSamples(float[] buffer)
    {
        Array.Clear(buffer, 0, buffer.Length);

        lock (_lock)
        {
            if (_activeVoice == null)
                return;

            var voice = _activeVoice;

            for (int i = 0; i < buffer.Length; i++)
            {
                if (voice.IsFinished)
                {
                    _activeVoice = null;
                    break;
                }

                // Get sample with linear interpolation
                float sample = GetInterpolatedSample(voice);

                // Apply envelope
                float envelope = CalculateEnvelope(voice);

                // Apply volume and envelope
                buffer[i] = sample * voice.Volume * envelope;

                // Advance position
                voice.FractionalPosition += voice.PitchRatio;
                while (voice.FractionalPosition >= 1.0)
                {
                    voice.FractionalPosition -= 1.0;
                    voice.Position++;
                }

                // Check if we've reached the end
                if (voice.Position >= voice.EndPosition)
                {
                    voice.IsFinished = true;
                }
            }
        }
    }

    private float[]? GetSamplesForRegion(SfzRegion region)
    {
        if (_instrument == null)
            return null;

        // Check for test samples first
        if (_instrument.TestSamples != null)
            return _instrument.TestSamples;

        // Try to get loaded samples
        var samplePath = _instrument.GetSamplePath(region);
        if (_instrument.LoadedSamples.TryGetValue(samplePath, out var wavData))
            return wavData.Samples;

        return null;
    }

    private static float GetInterpolatedSample(Voice voice)
    {
        int pos = voice.Position;
        if (pos < 0 || pos >= voice.Samples.Length - 1)
            return 0f;

        float frac = (float)voice.FractionalPosition;
        float s0 = voice.Samples[pos];
        float s1 = voice.Samples[Math.Min(pos + 1, voice.Samples.Length - 1)];
        
        return s0 + (s1 - s0) * frac;
    }

    private float CalculateEnvelope(Voice voice)
    {
        float level = voice.EnvelopeLevel;

        switch (voice.EnvelopePhase)
        {
            case EnvelopePhase.Attack:
                level = (float)voice.EnvelopePosition / voice.AttackSamples;
                voice.EnvelopePosition++;
                if (voice.EnvelopePosition >= voice.AttackSamples)
                {
                    voice.EnvelopePhase = EnvelopePhase.Sustain;
                    level = voice.SustainLevel;
                }
                break;

            case EnvelopePhase.Sustain:
                level = voice.SustainLevel;
                break;

            case EnvelopePhase.Release:
                float releaseProgress = (float)voice.EnvelopePosition / voice.ReleaseSamples;
                level = voice.ReleaseStartLevel * (1f - releaseProgress);
                voice.EnvelopePosition++;
                if (voice.EnvelopePosition >= voice.ReleaseSamples)
                {
                    level = 0;
                    voice.IsFinished = true;
                }
                break;
        }

        voice.EnvelopeLevel = level;
        return level;
    }

    private static float DbToLinear(float db)
    {
        if (db == 0)
            return 1f;
        return (float)Math.Pow(10, db / 20.0);
    }

    private class Voice
    {
        public float[] Samples { get; set; } = Array.Empty<float>();
        public int Position { get; set; }
        public int EndPosition { get; set; }
        public double FractionalPosition { get; set; }
        public double PitchRatio { get; set; } = 1.0;
        public float Volume { get; set; } = 1f;
        
        // Envelope
        public int AttackSamples { get; set; }
        public int ReleaseSamples { get; set; }
        public float SustainLevel { get; set; } = 1f;
        public EnvelopePhase EnvelopePhase { get; set; }
        public int EnvelopePosition { get; set; }
        public float EnvelopeLevel { get; set; }
        public float ReleaseStartLevel { get; set; }
        
        public bool IsFinished { get; set; }
    }

    private enum EnvelopePhase
    {
        Attack,
        Sustain,
        Release
    }
}

