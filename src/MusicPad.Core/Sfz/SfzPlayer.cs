namespace MusicPad.Core.Sfz;

/// <summary>
/// Plays SFZ instruments by generating audio samples.
/// Supports polyphonic playback with configurable max voices.
/// </summary>
public class SfzPlayer
{
    private const int DefaultMaxVoices = 10;
    
    private readonly int _sampleRate;
    private readonly int _maxVoices;
    private SfzInstrument? _instrument;
    private readonly List<Voice> _voices = new();
    private readonly object _lock = new();

    public SfzPlayer(int sampleRate, int maxVoices = DefaultMaxVoices)
    {
        _sampleRate = sampleRate;
        _maxVoices = Math.Max(1, maxVoices);
    }

    /// <summary>
    /// Gets the number of currently active voices.
    /// </summary>
    public int ActiveVoiceCount
    {
        get
        {
            lock (_lock)
            {
                return _voices.Count;
            }
        }
    }

    /// <summary>
    /// Loads an SFZ instrument for playback.
    /// </summary>
    public void LoadInstrument(SfzInstrument instrument)
    {
        lock (_lock)
        {
            _instrument = instrument;
            _voices.Clear();
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

            // Check if this note is already playing
            if (_voices.Any(v => v.MidiNote == midiNote && v.EnvelopePhase != EnvelopePhase.Release))
                return;

            // Find matching regions
            var regions = _instrument.FindRegions(midiNote, velocity).ToList();
            if (regions.Count == 0)
                return;

            // Use the first matching region
            var region = regions[0];
            
            // Get sample data
            float[]? samples = GetSamplesForRegion(region);
            if (samples == null || samples.Length == 0)
                return;

            // Voice stealing if at max polyphony
            if (_voices.Count >= _maxVoices)
            {
                _voices.RemoveAt(0); // Remove oldest voice
            }

            // Calculate pitch ratio
            double pitchRatio = region.GetPitchRatio(midiNote);

            // Calculate envelope parameters in samples
            int attackSamples = Math.Max(1, (int)(_sampleRate * region.AmpegAttack));
            int holdSamples = (int)(_sampleRate * region.AmpegHold);
            int decaySamples = Math.Max(1, (int)(_sampleRate * region.AmpegDecay));
            int releaseSamples = Math.Max(1, (int)(_sampleRate * region.AmpegRelease));
            
            // Sustain level - ensure minimum audible level for instruments with very low sustain
            float sustainLevel = region.AmpegSustain / 100f;
            if (sustainLevel < 0.01f && (region.AmpegDecay > 0.1f || region.AmpegHold < 0.1f))
            {
                sustainLevel = Math.Max(sustainLevel, 0.5f);
            }
            
            // Clamp pitch ratio to prevent extremely slow or fast playback
            double clampedPitchRatio = Math.Clamp(pitchRatio, 0.1, 10.0);

            // Create voice
            var voice = new Voice
            {
                MidiNote = midiNote,
                Samples = samples,
                Position = region.Offset,
                EndPosition = region.End > 0 ? region.End : samples.Length - 1,
                PitchRatio = clampedPitchRatio,
                Volume = DbToLinear(region.Volume),
                AttackSamples = attackSamples,
                HoldSamples = holdSamples,
                DecaySamples = decaySamples,
                ReleaseSamples = releaseSamples,
                SustainLevel = sustainLevel,
                EnvelopePhase = EnvelopePhase.Attack,
                EnvelopePosition = 0,
                EnvelopeLevel = 0
            };

            _voices.Add(voice);
        }
    }

    /// <summary>
    /// Triggers a note-off event.
    /// </summary>
    public void NoteOff(int midiNote)
    {
        lock (_lock)
        {
            var voice = _voices.FirstOrDefault(v => v.MidiNote == midiNote && v.EnvelopePhase != EnvelopePhase.Release);
            if (voice != null)
            {
                voice.EnvelopePhase = EnvelopePhase.Release;
                voice.EnvelopePosition = 0;
                voice.ReleaseStartLevel = voice.EnvelopeLevel;
            }
        }
    }

    /// <summary>
    /// Stops all playing voices.
    /// </summary>
    public void StopAll()
    {
        lock (_lock)
        {
            foreach (var voice in _voices)
            {
                if (voice.EnvelopePhase != EnvelopePhase.Release)
                {
                    voice.EnvelopePhase = EnvelopePhase.Release;
                    voice.EnvelopePosition = 0;
                    voice.ReleaseStartLevel = voice.EnvelopeLevel;
                }
            }
        }
    }

    /// <summary>
    /// Generates audio samples into the provided buffer.
    /// </summary>
    public void GenerateSamples(float[] buffer)
    {
        Array.Clear(buffer, 0, buffer.Length);

        Voice[] snapshot;
        lock (_lock)
        {
            snapshot = _voices.ToArray();
        }

        if (snapshot.Length == 0)
            return;

        // Mix all voices
        for (int i = 0; i < buffer.Length; i++)
        {
            float sample = 0f;

            foreach (var voice in snapshot)
            {
                if (voice.IsFinished)
                    continue;

                // Get sample with linear interpolation
                float voiceSample = GetInterpolatedSample(voice);

                // Apply envelope
                float envelope = CalculateEnvelope(voice);

                // Apply volume and envelope, add to mix
                sample += voiceSample * voice.Volume * envelope;

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

            buffer[i] = sample;
        }

        // Remove finished voices
        lock (_lock)
        {
            for (int v = _voices.Count - 1; v >= 0; v--)
            {
                if (_voices[v].IsFinished)
                {
                    _voices.RemoveAt(v);
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
                    level = 1.0f;
                    if (voice.HoldSamples > 0)
                    {
                        voice.EnvelopePhase = EnvelopePhase.Hold;
                        voice.EnvelopePosition = 0;
                    }
                    else
                    {
                        voice.EnvelopePhase = EnvelopePhase.Decay;
                        voice.EnvelopePosition = 0;
                    }
                }
                break;

            case EnvelopePhase.Hold:
                level = 1.0f;
                voice.EnvelopePosition++;
                if (voice.EnvelopePosition >= voice.HoldSamples)
                {
                    voice.EnvelopePhase = EnvelopePhase.Decay;
                    voice.EnvelopePosition = 0;
                }
                break;

            case EnvelopePhase.Decay:
                {
                    float decayProgress = (float)voice.EnvelopePosition / voice.DecaySamples;
                    level = 1.0f - (1.0f - voice.SustainLevel) * decayProgress;
                    voice.EnvelopePosition++;
                    if (voice.EnvelopePosition >= voice.DecaySamples)
                    {
                        voice.EnvelopePhase = EnvelopePhase.Sustain;
                        level = voice.SustainLevel;
                    }
                }
                break;

            case EnvelopePhase.Sustain:
                level = voice.SustainLevel;
                break;

            case EnvelopePhase.Release:
                {
                    float releaseProgress = (float)voice.EnvelopePosition / voice.ReleaseSamples;
                    level = voice.ReleaseStartLevel * (1f - releaseProgress);
                    voice.EnvelopePosition++;
                    if (voice.EnvelopePosition >= voice.ReleaseSamples)
                    {
                        level = 0;
                        voice.IsFinished = true;
                    }
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
        public int MidiNote { get; set; }
        public float[] Samples { get; set; } = Array.Empty<float>();
        public int Position { get; set; }
        public int EndPosition { get; set; }
        public double FractionalPosition { get; set; }
        public double PitchRatio { get; set; } = 1.0;
        public float Volume { get; set; } = 1f;
        
        // Envelope (AHDSR)
        public int AttackSamples { get; set; }
        public int HoldSamples { get; set; }
        public int DecaySamples { get; set; }
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
        Hold,
        Decay,
        Sustain,
        Release
    }
}
