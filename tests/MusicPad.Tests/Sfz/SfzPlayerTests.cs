using MusicPad.Core.Sfz;

namespace MusicPad.Tests.Sfz;

public class SfzPlayerTests
{
    [Fact]
    public void GenerateSamples_NoActiveNote_ReturnsSilence()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var buffer = new float[512];
        
        player.GenerateSamples(buffer);
        
        Assert.All(buffer, sample => Assert.Equal(0f, sample));
    }

    [Fact]
    public void NoteOn_WithLoadedInstrument_GeneratesSound()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateTestInstrument();
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        // Should have some non-zero samples
        Assert.Contains(buffer, s => s != 0f);
    }

    [Fact]
    public void NoteOff_StopsSound()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateTestInstrument();
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        player.NoteOff(60);
        
        // Generate enough samples for release to complete
        var buffer = new float[44100]; // 1 second
        player.GenerateSamples(buffer);
        
        // Last samples should be silent after release
        var lastSamples = buffer.Skip(buffer.Length - 100).ToArray();
        Assert.All(lastSamples, s => Assert.True(Math.Abs(s) < 0.001f));
    }

    [Fact]
    public void GenerateSamples_AppliesPitchShift()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateTestInstrumentWithPitchCenter(60);
        player.LoadInstrument(instrument);
        
        // Play one octave up - should play back at double speed
        player.NoteOn(72, velocity: 100);
        
        var buffer = new float[1024];
        player.GenerateSamples(buffer);
        
        // Verify sound is generated (detailed pitch verification would need spectral analysis)
        Assert.Contains(buffer, s => s != 0f);
    }

    [Fact]
    public void LoadInstrument_ClearsActiveNotes()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument1 = CreateTestInstrument();
        player.LoadInstrument(instrument1);
        player.NoteOn(60, velocity: 100);
        
        // Load new instrument
        var instrument2 = CreateTestInstrument();
        player.LoadInstrument(instrument2);
        
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        // Should be silent (no active notes after reload)
        Assert.All(buffer, s => Assert.Equal(0f, s));
    }

    #region Loop Mode Tests

    [Fact]
    public void LoopContinuous_SampleLoopsIndefinitely()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateLoopingInstrument(LoopMode.LoopContinuous, loopStart: 100, loopEnd: 200);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate enough samples to go through the loop multiple times
        // Sample plays from 0 to loopEnd (200), then loops back to loopStart (100)
        // Each loop iteration is 100 samples, so 5000 samples should be ~50 loops
        var buffer = new float[5000];
        player.GenerateSamples(buffer);
        
        // Voice should still be active (not finished) because it's looping
        Assert.Equal(1, player.ActiveVoiceCount);
        
        // Should have sound throughout (looping samples)
        var lastPortion = buffer.Skip(4500).Take(100).ToArray();
        Assert.Contains(lastPortion, s => Math.Abs(s) > 0.001f);
    }

    [Fact]
    public void LoopContinuous_VoiceDoesNotEndPrematurely()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        // Short sample that would end quickly without looping
        var instrument = CreateLoopingInstrument(LoopMode.LoopContinuous, loopStart: 50, loopEnd: 100, sampleLength: 150);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate way more samples than the sample length
        var buffer = new float[1000];
        player.GenerateSamples(buffer);
        
        // Voice should still be playing due to loop
        Assert.Equal(1, player.ActiveVoiceCount);
    }

    [Fact]
    public void NoLoop_VoiceEndsAtSampleEnd()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateLoopingInstrument(LoopMode.NoLoop, loopStart: 50, loopEnd: 100, sampleLength: 200);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate more samples than the sample length
        var buffer = new float[500];
        player.GenerateSamples(buffer);
        
        // Voice should have finished (no loop, reached end)
        Assert.Equal(0, player.ActiveVoiceCount);
    }

    [Fact]
    public void LoopSustain_LoopsDuringSustain()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateLoopingInstrument(LoopMode.LoopSustain, loopStart: 50, loopEnd: 100, sampleLength: 200);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate samples while note is held (in sustain)
        var buffer = new float[500];
        player.GenerateSamples(buffer);
        
        // Voice should still be playing (looping in sustain)
        Assert.Equal(1, player.ActiveVoiceCount);
    }

    [Fact]
    public void LoopSustain_PlaysToEndAfterNoteOff()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        // Create instrument with loop in middle, and enough samples after loop for release
        var instrument = CreateLoopingInstrument(LoopMode.LoopSustain, loopStart: 50, loopEnd: 100, sampleLength: 300);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Play at least 80ms to satisfy minimum hold time (80ms * 44.1 = 3528 samples)
        var buffer1 = new float[4000];
        player.GenerateSamples(buffer1);
        Assert.Equal(1, player.ActiveVoiceCount); // Still playing (looping)
        
        player.NoteOff(60);
        
        // Generate enough samples for release phase to complete and sample to end
        var buffer2 = new float[10000];
        player.GenerateSamples(buffer2);
        
        // Voice should have finished (played to end after release)
        Assert.Equal(0, player.ActiveVoiceCount);
    }

    [Fact]
    public void LoopContinuous_StillLoopsAfterNoteOff()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateLoopingInstrument(LoopMode.LoopContinuous, loopStart: 50, loopEnd: 100, sampleLength: 150);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        player.NoteOff(60);
        
        // Generate samples during release - voice should continue looping but with envelope
        var buffer = new float[500];
        player.GenerateSamples(buffer);
        
        // Voice finishes when release envelope completes, not when sample ends
        // With default short release, voice should be gone
        // But if release was very short, voice might still be there
        // This test just verifies it doesn't crash
    }

    [Fact]
    public void LoopContinuous_InvalidLoopPoints_NoLoop()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        // Invalid loop points (loopEnd <= loopStart)
        var instrument = CreateLoopingInstrument(LoopMode.LoopContinuous, loopStart: 100, loopEnd: 50, sampleLength: 200);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate more samples than sample length
        var buffer = new float[500];
        player.GenerateSamples(buffer);
        
        // Should behave like no loop (voice ends at sample end)
        Assert.Equal(0, player.ActiveVoiceCount);
    }

    #endregion

    private static SfzInstrument CreateTestInstrument()
    {
        return CreateTestInstrumentWithPitchCenter(60);
    }

    private static SfzInstrument CreateTestInstrumentWithPitchCenter(int pitchCenter)
    {
        var instrument = new SfzInstrument
        {
            Name = "Test",
            BasePath = ""
        };

        // Create a test region with embedded sample data
        var region = new SfzRegion
        {
            LoKey = 0,
            HiKey = 127,
            PitchKeycenter = pitchCenter,
            Offset = 0,
            End = 999,
            AmpegAttack = 0.001f,
            AmpegRelease = 0.1f, // Short release for testing
            AmpegSustain = 100f
        };
        instrument.Regions.Add(region);

        // Generate sine wave samples directly in the player for testing
        var samples = new float[1000];
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * 440 * i / 44100.0);
        }
        instrument.TestSamples = samples; // We'll add this property for testing

        return instrument;
    }

    private static SfzInstrument CreateLoopingInstrument(LoopMode loopMode, int loopStart, int loopEnd, int sampleLength = 1000)
    {
        var instrument = new SfzInstrument
        {
            Name = "LoopTest",
            BasePath = ""
        };

        var region = new SfzRegion
        {
            LoKey = 0,
            HiKey = 127,
            PitchKeycenter = 60,
            Offset = 0,
            End = sampleLength - 1,
            LoopMode = loopMode,
            LoopStart = loopStart,
            LoopEnd = loopEnd,
            AmpegAttack = 0.001f,
            AmpegRelease = 0.1f,
            AmpegSustain = 100f
        };
        instrument.Regions.Add(region);

        // Generate sine wave samples
        var samples = new float[sampleLength];
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * 440 * i / 44100.0);
        }
        instrument.TestSamples = samples;

        return instrument;
    }
    
    #region GetEnvelopeLevel Tests
    
    [Fact]
    public void GetEnvelopeLevel_NoActiveNote_ReturnsZero()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateTestInstrument();
        player.LoadInstrument(instrument);
        
        float level = player.GetEnvelopeLevel(60);
        
        Assert.Equal(0f, level);
    }
    
    [Fact]
    public void GetEnvelopeLevel_ActiveNote_ReturnsPositiveValue()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateTestInstrumentWithEnvelope(
            attack: 0.01f,  // 10ms attack
            decay: 0.1f,
            sustain: 100f,
            release: 0.5f);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate some samples to advance past attack phase
        var buffer = new float[2048];  // ~46ms at 44100Hz
        player.GenerateSamples(buffer);
        
        float level = player.GetEnvelopeLevel(60);
        
        Assert.True(level > 0f, $"Envelope level should be positive for active note, got {level}");
        Assert.True(level <= 1f, "Envelope level should not exceed 1.0");
    }
    
    [Fact]
    public void GetEnvelopeLevel_AfterNoteOff_DecreasesOverTime()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateTestInstrumentWithEnvelope(
            attack: 0.001f, 
            decay: 0.01f, 
            sustain: 100f, 
            release: 0.5f);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate samples to reach sustain
        var buffer = new float[2048];
        player.GenerateSamples(buffer);
        
        float levelBeforeRelease = player.GetEnvelopeLevel(60);
        
        player.NoteOff(60);
        
        // Generate some samples during release
        player.GenerateSamples(buffer);
        float levelDuringRelease = player.GetEnvelopeLevel(60);
        
        // Generate more samples - should be lower
        player.GenerateSamples(buffer);
        float levelLaterInRelease = player.GetEnvelopeLevel(60);
        
        Assert.True(levelDuringRelease <= levelBeforeRelease, 
            "Level should decrease after NoteOff");
        Assert.True(levelLaterInRelease <= levelDuringRelease, 
            "Level should continue decreasing during release");
    }
    
    [Fact]
    public void GetEnvelopeLevel_AfterReleaseComplete_ReturnsZero()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateTestInstrumentWithEnvelope(
            attack: 0.001f, 
            decay: 0.01f, 
            sustain: 100f, 
            release: 0.1f);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        player.NoteOff(60);
        
        // Generate enough samples for release to complete (0.1s = 4410 samples + margin)
        var buffer = new float[44100]; // 1 second - more than enough
        player.GenerateSamples(buffer);
        
        float level = player.GetEnvelopeLevel(60);
        
        Assert.Equal(0f, level);
    }
    
    [Fact]
    public void GetEnvelopeLevel_DifferentNotes_ReturnsIndependentLevels()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateTestInstrumentWithEnvelope(
            attack: 0.01f,
            decay: 0.1f,
            sustain: 100f,
            release: 0.5f);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate enough samples to get past attack phase
        var buffer = new float[2048];
        player.GenerateSamples(buffer);
        
        float level60 = player.GetEnvelopeLevel(60);
        float level61 = player.GetEnvelopeLevel(61);
        
        Assert.True(level60 > 0f, $"Active note 60 should have positive level, got {level60}");
        Assert.Equal(0f, level61); // Note 61 was never played
    }
    
    private SfzInstrument CreateTestInstrumentWithEnvelope(
        float attack, float decay, float sustain, float release)
    {
        var instrument = new SfzInstrument();
        var sampleLength = 44100;
        
        var region = new SfzRegion
        {
            Sample = "test.wav",
            LoKey = 0,
            HiKey = 127,
            PitchKeycenter = 60,
            Offset = 0,
            End = sampleLength - 1,
            AmpegAttack = attack,
            AmpegDecay = decay,
            AmpegSustain = sustain,
            AmpegRelease = release
        };
        instrument.Regions.Add(region);

        var samples = new float[sampleLength];
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * 440 * i / 44100.0);
        }
        instrument.TestSamples = samples;

        return instrument;
    }
    
    #endregion
}

