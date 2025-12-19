using System.Diagnostics;

namespace MusicPad.Core.Sfz;

/// <summary>
/// Plays SFZ instruments by generating audio samples.
/// Supports polyphonic playback with configurable max voices.
/// Uses a fixed voice pool with intelligent allocation like real acoustic instruments.
/// </summary>
public class SfzPlayer
{
    private const int DefaultMaxVoices = 10;
    
    private readonly int _sampleRate;
    private readonly int _maxVoices;
    private SfzInstrument? _instrument;
    private readonly Voice[] _voices;
    private readonly object _lock = new();

    public SfzPlayer(int sampleRate, int maxVoices = DefaultMaxVoices)
    {
        _sampleRate = sampleRate;
        _maxVoices = Math.Max(1, maxVoices);
        
        // Pre-create voice pool
        _voices = new Voice[_maxVoices];
        for (int i = 0; i < _maxVoices; i++)
        {
            _voices[i] = new Voice();
        }
    }

    /// <summary>
    /// Gets the number of currently active (non-idle) voices.
    /// </summary>
    public int ActiveVoiceCount
    {
        get
        {
            lock (_lock)
            {
                return _voices.Count(v => v.EnvelopePhase != EnvelopePhase.Idle);
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
            // Reset all voices to idle
            foreach (var voice in _voices)
            {
                voice.EnvelopePhase = EnvelopePhase.Idle;
                voice.IsFinished = false;
            }
        }
    }

    /// <summary>
    /// Allocates a voice for a new note using the priority:
    /// 1. Idle voice (preferred - no interruption)
    /// 2. Oldest voice in release phase (minimal disruption)
    /// 3. Oldest playing voice (last resort - voice stealing)
    /// </summary>
    private Voice AllocateVoice()
    {
        // 1. Prefer an idle voice
        foreach (var voice in _voices)
        {
            if (voice.EnvelopePhase == EnvelopePhase.Idle)
                return voice;
        }

        // 2. Prefer the oldest voice in release phase (released first)
        Voice? oldestReleasing = null;
        long oldestReleaseTicks = long.MaxValue;
        
        foreach (var voice in _voices)
        {
            if (voice.EnvelopePhase == EnvelopePhase.Release && voice.ReleaseStartTicks < oldestReleaseTicks)
            {
                oldestReleaseTicks = voice.ReleaseStartTicks;
                oldestReleasing = voice;
            }
        }
        
        if (oldestReleasing != null)
        {
            // Immediately silence and reuse
            oldestReleasing.EnvelopePhase = EnvelopePhase.Idle;
            return oldestReleasing;
        }

        // 3. Steal the oldest playing voice (started first)
        Voice? oldestPlaying = null;
        long oldestStartTicks = long.MaxValue;
        
        foreach (var voice in _voices)
        {
            if (voice.StartTimeTicks < oldestStartTicks)
            {
                oldestStartTicks = voice.StartTimeTicks;
                oldestPlaying = voice;
            }
        }
        
        if (oldestPlaying != null)
        {
            // Force immediate cutoff and reuse
            oldestPlaying.EnvelopePhase = EnvelopePhase.Idle;
            return oldestPlaying;
        }

        // Fallback (should never reach here)
        return _voices[0];
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
            
            // Clamp velocity to valid MIDI range
            velocity = Math.Clamp(velocity, 1, 127);

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

            // Check if this note is already playing (not idle, not releasing) - retrigger in place
            Voice? voice = null;
            foreach (var existingVoice in _voices)
            {
                if (existingVoice.MidiNote == midiNote && 
                    existingVoice.EnvelopePhase != EnvelopePhase.Idle &&
                    existingVoice.EnvelopePhase != EnvelopePhase.Release)
                {
                    // Reuse this voice for retriggering
                    voice = existingVoice;
                    break;
                }
            }
            
            // If not retriggering, allocate a new voice
            if (voice == null)
            {
                voice = AllocateVoice();
            }

            // Calculate pitch ratio
            double pitchRatio = region.GetPitchRatio(midiNote);

            // Calculate envelope parameters in samples
            // Clamp release to max 3 seconds to prevent stuck notes
            int attackSamples = Math.Max(1, (int)(_sampleRate * region.AmpegAttack));
            int holdSamples = (int)(_sampleRate * Math.Min(region.AmpegHold, 10f));
            int decaySamples = Math.Max(1, (int)(_sampleRate * region.AmpegDecay));
            int releaseSamples = Math.Max(1, Math.Min((int)(_sampleRate * region.AmpegRelease), _sampleRate * 3));
            
            // Sustain level - ensure minimum audible level for instruments with very low sustain
            float sustainLevel = region.AmpegSustain / 100f;
            if (sustainLevel < 0.01f && (region.AmpegDecay > 0.1f || region.AmpegHold < 0.1f))
            {
                sustainLevel = Math.Max(sustainLevel, 0.5f);
            }
            
            // Clamp pitch ratio to prevent extremely slow or fast playback
            double clampedPitchRatio = Math.Clamp(pitchRatio, 0.1, 10.0);

            // Calculate loop points (relative to sample start)
            int loopStart = region.LoopStart;
            int loopEnd = region.LoopEnd > 0 ? region.LoopEnd : (region.End > 0 ? region.End : samples.Length - 1);
            
            // Calculate velocity scaling (MIDI velocity 0-127 -> amplitude 0-1)
            // Use a curve that feels more natural (velocity squared for more dynamic range)
            float velocityScale = (velocity / 127f);
            velocityScale = velocityScale * velocityScale; // Square for more natural feel
            velocityScale = Math.Max(0.1f, velocityScale);  // Minimum 10% volume
            
            // Configure the voice with new note
            voice.MidiNote = midiNote;
            voice.Samples = samples;
            voice.Position = region.Offset;
            voice.EndPosition = region.End > 0 ? region.End : samples.Length - 1;
            voice.FractionalPosition = 0;
            voice.PitchRatio = clampedPitchRatio;
            voice.Volume = DbToLinear(region.Volume) * velocityScale;
            voice.LoopMode = region.LoopMode;
            voice.LoopStart = loopStart;
            voice.LoopEnd = loopEnd;
            voice.AttackSamples = attackSamples;
            voice.HoldSamples = holdSamples;
            voice.DecaySamples = decaySamples;
            voice.ReleaseSamples = releaseSamples;
            voice.SustainLevel = sustainLevel;
            voice.EnvelopePhase = EnvelopePhase.Attack;
            voice.EnvelopePosition = 0;
            voice.EnvelopeLevel = 0;
            voice.ReleaseStartLevel = 0;
            voice.StartTimeTicks = Stopwatch.GetTimestamp();
            voice.ReleaseStartTicks = 0;
            voice.IsFinished = false;
        }
    }

    /// <summary>
    /// Triggers a note-off event.
    /// </summary>
    public void NoteOff(int midiNote)
    {
        lock (_lock)
        {
            long now = Stopwatch.GetTimestamp();
            
            // Find all voices with this note that aren't already releasing or idle
            foreach (var voice in _voices)
            {
                if (voice.MidiNote == midiNote && 
                    voice.EnvelopePhase != EnvelopePhase.Release && 
                    voice.EnvelopePhase != EnvelopePhase.Idle)
                {
                    voice.EnvelopePhase = EnvelopePhase.Release;
                    voice.EnvelopePosition = 0;
                    voice.ReleaseStartTicks = now;
                    // Ensure release starts from at least a small level to prevent stuck silent voices
                    voice.ReleaseStartLevel = Math.Max(voice.EnvelopeLevel, 0.01f);
                }
            }
        }
    }

    /// <summary>
    /// Gets the current envelope level for a note (0.0 = silent, 1.0 = full).
    /// Returns 0 if the note is not currently playing.
    /// </summary>
    public float GetEnvelopeLevel(int midiNote)
    {
        lock (_lock)
        {
            // Find the voice playing this note with the highest envelope level
            float maxLevel = 0f;
            
            foreach (var voice in _voices)
            {
                if (voice.MidiNote == midiNote && voice.EnvelopePhase != EnvelopePhase.Idle)
                {
                    maxLevel = Math.Max(maxLevel, voice.EnvelopeLevel);
                }
            }
            
            return maxLevel;
        }
    }
    
    /// <summary>
    /// Stops all playing voices immediately.
    /// </summary>
    public void StopAll()
    {
        lock (_lock)
        {
            long now = Stopwatch.GetTimestamp();
            
            foreach (var voice in _voices)
            {
                if (voice.EnvelopePhase != EnvelopePhase.Release && voice.EnvelopePhase != EnvelopePhase.Idle)
                {
                    voice.EnvelopePhase = EnvelopePhase.Release;
                    voice.EnvelopePosition = 0;
                    voice.ReleaseStartTicks = now;
                    voice.ReleaseStartLevel = Math.Max(voice.EnvelopeLevel, 0.01f);
                }
            }
        }
    }

    /// <summary>
    /// Immediately stops all voices without release phase.
    /// </summary>
    public void KillAll()
    {
        lock (_lock)
        {
            foreach (var voice in _voices)
            {
                voice.EnvelopePhase = EnvelopePhase.Idle;
                voice.IsFinished = false;
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

        // Mix all voices
        for (int i = 0; i < buffer.Length; i++)
        {
            float sample = 0f;

            foreach (var voice in snapshot)
            {
                if (voice.EnvelopePhase == EnvelopePhase.Idle)
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

                // Handle looping and end of sample
                if (voice.LoopMode == LoopMode.LoopContinuous && voice.LoopEnd > voice.LoopStart)
                {
                    // Loop continuously while playing
                    if (voice.Position >= voice.LoopEnd)
                    {
                        voice.Position = voice.LoopStart + (voice.Position - voice.LoopEnd);
                    }
                }
                else if (voice.LoopMode == LoopMode.LoopSustain && voice.LoopEnd > voice.LoopStart)
                {
                    // Loop while in sustain phase, play to end during release
                    if (voice.EnvelopePhase != EnvelopePhase.Release && voice.Position >= voice.LoopEnd)
                    {
                        voice.Position = voice.LoopStart + (voice.Position - voice.LoopEnd);
                    }
                    else if (voice.Position >= voice.EndPosition)
                    {
                        voice.EnvelopePhase = EnvelopePhase.Idle;
                    }
                }
                else
                {
                    // No loop or one-shot: end when reaching end position
                    if (voice.Position >= voice.EndPosition)
                    {
                        voice.EnvelopePhase = EnvelopePhase.Idle;
                    }
                }
            }

            buffer[i] = sample;
        }

        // Mark finished voices as idle (instead of removing)
        lock (_lock)
        {
            foreach (var voice in _voices)
            {
                if (voice.IsFinished)
                {
                    voice.EnvelopePhase = EnvelopePhase.Idle;
                    voice.IsFinished = false;
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
                        voice.EnvelopePhase = EnvelopePhase.Idle;
                    }
                }
                break;
                
            case EnvelopePhase.Idle:
                level = 0;
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
        
        // Loop settings
        public LoopMode LoopMode { get; set; } = LoopMode.NoLoop;
        public int LoopStart { get; set; }
        public int LoopEnd { get; set; }
        
        // Envelope (AHDSR)
        public int AttackSamples { get; set; }
        public int HoldSamples { get; set; }
        public int DecaySamples { get; set; }
        public int ReleaseSamples { get; set; }
        public float SustainLevel { get; set; } = 1f;
        public EnvelopePhase EnvelopePhase { get; set; } = EnvelopePhase.Idle;
        public int EnvelopePosition { get; set; }
        public float EnvelopeLevel { get; set; }
        public float ReleaseStartLevel { get; set; }
        
        // Timing for voice allocation
        public long StartTimeTicks { get; set; }
        public long ReleaseStartTicks { get; set; }
        
        public bool IsFinished { get; set; }
    }

    private enum EnvelopePhase
    {
        Idle,       // Voice available for assignment
        Attack,
        Hold,
        Decay,
        Sustain,
        Release
    }
}
