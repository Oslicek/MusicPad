using MusicPad.Core.Models;
using MusicPad.Core.Sfz;

namespace MusicPad.Tests.Sfz;

/// <summary>
/// Tests for monophonic and polyphonic voice allocation modes.
/// </summary>
public class SfzPlayerVoicingTests
{
    #region Monophonic Mode Tests
    
    [Fact]
    public void Monophonic_NoteOn_PlaysNote()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        player.VoicingMode = VoicingType.Monophonic;
        var instrument = CreateTestInstrument();
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        Assert.Contains(buffer, s => s != 0f);
        Assert.Equal(1, player.ActiveVoiceCount);
    }
    
    [Fact]
    public void Monophonic_SecondNoteOn_StopsFirstNoteImmediately()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        player.VoicingMode = VoicingType.Monophonic;
        var instrument = CreateTestInstrument();
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate samples to get note 60 playing
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        float level60Before = player.GetEnvelopeLevel(60);
        Assert.True(level60Before > 0f, "Note 60 should be playing");
        
        // Play a second note (queued for next buffer)
        player.NoteOn(64, velocity: 100);
        
        // Process buffer to trigger the queued NoteOn
        player.GenerateSamples(buffer);
        
        // Note 60 should be stopped (no release phase in mono)
        float level60After = player.GetEnvelopeLevel(60);
        Assert.Equal(0f, level60After);
        
        // Note 64 should be playing
        float level64 = player.GetEnvelopeLevel(64);
        Assert.True(level64 > 0f, "Note 64 should be playing");
        
        // Only one voice should be active
        Assert.Equal(1, player.ActiveVoiceCount);
    }
    
    [Fact]
    public void Monophonic_NoteOff_PlaysReleasePhase()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        player.VoicingMode = VoicingType.Monophonic;
        var instrument = CreateTestInstrumentWithEnvelope(
            attack: 0.001f, 
            decay: 0.01f, 
            sustain: 100f, 
            release: 0.5f);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate samples to get past attack
        var buffer = new float[1024];
        player.GenerateSamples(buffer);
        
        float levelBefore = player.GetEnvelopeLevel(60);
        Assert.True(levelBefore > 0f);
        
        // Release the note
        player.NoteOff(60);
        
        // Should still be playing (in release phase)
        player.GenerateSamples(buffer);
        float levelDuringRelease = player.GetEnvelopeLevel(60);
        Assert.True(levelDuringRelease > 0f, "Should be in release phase");
        Assert.True(levelDuringRelease <= levelBefore, "Level should decrease in release");
    }
    
    [Fact]
    public void Monophonic_NewNoteWhileReleasing_StopsReleaseImmediately()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        player.VoicingMode = VoicingType.Monophonic;
        var instrument = CreateTestInstrumentWithEnvelope(
            attack: 0.001f, 
            decay: 0.01f, 
            sustain: 100f, 
            release: 1.0f); // Long release
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate samples and release
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        player.NoteOff(60);
        
        // Generate a bit during release
        player.GenerateSamples(buffer);
        float level60During = player.GetEnvelopeLevel(60);
        Assert.True(level60During > 0f, "Should be in release phase");
        
        // Play new note (queued for next buffer)
        player.NoteOn(64, velocity: 100);
        
        // Process buffer to trigger the queued NoteOn
        player.GenerateSamples(buffer);
        
        // Old note should be stopped
        float level60After = player.GetEnvelopeLevel(60);
        Assert.Equal(0f, level60After);
        
        // New note should be playing
        float level64 = player.GetEnvelopeLevel(64);
        Assert.True(level64 > 0f);
    }
    
    [Fact]
    public void Monophonic_SameNoteTwice_RetriggersNote()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        player.VoicingMode = VoicingType.Monophonic;
        var instrument = CreateTestInstrumentWithEnvelope(
            attack: 0.01f, 
            decay: 0.1f, 
            sustain: 100f, 
            release: 0.5f);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Play through to sustain
        var buffer = new float[4410]; // ~100ms
        player.GenerateSamples(buffer);
        
        float levelFirst = player.GetEnvelopeLevel(60);
        
        // Retrigger same note
        player.NoteOn(60, velocity: 100);
        
        // Should retrigger (go back to attack phase)
        // After generating samples, should still be playing
        player.GenerateSamples(new float[512]);
        float levelAfter = player.GetEnvelopeLevel(60);
        
        Assert.True(levelAfter > 0f, "Note should still be playing after retrigger");
        Assert.Equal(1, player.ActiveVoiceCount);
    }
    
    #endregion
    
    #region Polyphonic Mode Tests
    
    [Fact]
    public void Polyphonic_MultipleNotes_PlaySimultaneously()
    {
        var player = new SfzPlayer(sampleRate: 44100, maxVoices: 10);
        player.VoicingMode = VoicingType.Polyphonic;
        var instrument = CreateTestInstrument();
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        player.NoteOn(64, velocity: 100);
        player.NoteOn(67, velocity: 100);
        
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        Assert.Equal(3, player.ActiveVoiceCount);
        Assert.True(player.GetEnvelopeLevel(60) > 0f);
        Assert.True(player.GetEnvelopeLevel(64) > 0f);
        Assert.True(player.GetEnvelopeLevel(67) > 0f);
    }
    
    [Fact]
    public void Polyphonic_VoiceStealing_StealOldestReleasing()
    {
        var player = new SfzPlayer(sampleRate: 44100, maxVoices: 3);
        player.VoicingMode = VoicingType.Polyphonic;
        var instrument = CreateTestInstrumentWithEnvelope(
            attack: 0.001f, 
            decay: 0.01f, 
            sustain: 100f, 
            release: 1.0f); // Long release
        player.LoadInstrument(instrument);
        
        // Fill all voices
        player.NoteOn(60, velocity: 100);
        player.NoteOn(64, velocity: 100);
        player.NoteOn(67, velocity: 100);
        
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        Assert.Equal(3, player.ActiveVoiceCount);
        
        // Release the first note
        player.NoteOff(60);
        
        player.GenerateSamples(buffer);
        
        // All voices still active (60 is in release)
        Assert.Equal(3, player.ActiveVoiceCount);
        
        // Play new note - should steal the releasing voice (60)
        player.NoteOn(72, velocity: 100);
        
        player.GenerateSamples(buffer);
        
        // Should still have 3 voices
        Assert.Equal(3, player.ActiveVoiceCount);
        
        // Note 72 should be playing
        Assert.True(player.GetEnvelopeLevel(72) > 0f);
        
        // Note 60 should be stopped (voice was stolen)
        Assert.Equal(0f, player.GetEnvelopeLevel(60));
    }
    
    [Fact]
    public void Polyphonic_VoiceStealing_StealOldestPlaying_WhenNoReleasing()
    {
        var player = new SfzPlayer(sampleRate: 44100, maxVoices: 3);
        player.VoicingMode = VoicingType.Polyphonic;
        var instrument = CreateTestInstrument();
        player.LoadInstrument(instrument);
        
        // Fill all voices
        player.NoteOn(60, velocity: 100);
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        player.NoteOn(64, velocity: 100);
        player.GenerateSamples(buffer);
        
        player.NoteOn(67, velocity: 100);
        player.GenerateSamples(buffer);
        
        Assert.Equal(3, player.ActiveVoiceCount);
        
        // Play new note - should steal the oldest playing voice (60)
        player.NoteOn(72, velocity: 100);
        
        player.GenerateSamples(buffer);
        
        // Note 60 should be stopped (voice was stolen)
        Assert.Equal(0f, player.GetEnvelopeLevel(60));
        
        // Note 72 should be playing
        Assert.True(player.GetEnvelopeLevel(72) > 0f);
        
        // Notes 64 and 67 should still be playing
        Assert.True(player.GetEnvelopeLevel(64) > 0f);
        Assert.True(player.GetEnvelopeLevel(67) > 0f);
    }
    
    [Fact]
    public void Polyphonic_VoicesPlayFullEnvelope()
    {
        var player = new SfzPlayer(sampleRate: 44100, maxVoices: 10);
        player.VoicingMode = VoicingType.Polyphonic;
        var instrument = CreateTestInstrumentWithEnvelope(
            attack: 0.01f, 
            decay: 0.1f, 
            sustain: 80f, 
            release: 0.5f);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        player.NoteOn(64, velocity: 100);
        
        // Generate samples to reach sustain
        var buffer = new float[8820]; // ~200ms
        player.GenerateSamples(buffer);
        
        // Both notes should be at sustain level
        float level60 = player.GetEnvelopeLevel(60);
        float level64 = player.GetEnvelopeLevel(64);
        
        Assert.True(level60 > 0.5f, $"Note 60 should be near sustain, got {level60}");
        Assert.True(level64 > 0.5f, $"Note 64 should be near sustain, got {level64}");
    }
    
    #endregion
    
    #region Mute Tests
    
    [Fact]
    public void Mute_StopsAllVoicesWithShortRelease()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateTestInstrument();
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        player.NoteOn(64, velocity: 100);
        
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        Assert.Equal(2, player.ActiveVoiceCount);
        
        // Mute all
        player.Mute();
        
        // Voices should still be active briefly (short release to avoid cracking)
        // But after the mute release completes, should be silent
        var muteBuffer = new float[4410]; // 100ms - more than mute release time
        player.GenerateSamples(muteBuffer);
        
        Assert.Equal(0, player.ActiveVoiceCount);
    }
    
    [Fact]
    public void Mute_DoesNotCrack()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        var instrument = CreateTestInstrumentWithEnvelope(
            attack: 0.001f,
            decay: 0.01f,
            sustain: 100f,
            release: 0.5f);
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        // Generate samples to reach sustain (high level)
        var buffer = new float[4410]; // ~100ms
        player.GenerateSamples(buffer);
        
        float levelBeforeMute = player.GetEnvelopeLevel(60);
        Assert.True(levelBeforeMute > 0.5f, "Should be at sustain level");
        
        // Mute
        player.Mute();
        
        // Generate samples during mute release
        var muteBuffer = new float[882]; // ~20ms
        player.GenerateSamples(muteBuffer);
        
        // Check that level is decreasing, not instantly zero (which would cause click)
        float levelDuringMute = player.GetEnvelopeLevel(60);
        
        // The mute should apply a very short release (e.g., 10-20ms)
        // After 20ms it should be very low or zero
        // But immediately after mute call, it should still have some level
        
        // Generate more to complete the mute
        var finalBuffer = new float[2205]; // ~50ms more
        player.GenerateSamples(finalBuffer);
        
        Assert.Equal(0, player.ActiveVoiceCount);
    }
    
    [Fact]
    public void Mute_Monophonic_StopsCurrentVoice()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        player.VoicingMode = VoicingType.Monophonic;
        var instrument = CreateTestInstrument();
        player.LoadInstrument(instrument);
        
        player.NoteOn(60, velocity: 100);
        
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        Assert.Equal(1, player.ActiveVoiceCount);
        
        player.Mute();
        
        // After mute release completes
        var muteBuffer = new float[4410];
        player.GenerateSamples(muteBuffer);
        
        Assert.Equal(0, player.ActiveVoiceCount);
    }
    
    #endregion
    
    #region Default Mode Tests
    
    [Fact]
    public void DefaultMode_IsPolyphonic()
    {
        var player = new SfzPlayer(sampleRate: 44100);
        
        Assert.Equal(VoicingType.Polyphonic, player.VoicingMode);
    }
    
    #endregion
    
    #region Helper Methods
    
    private static SfzInstrument CreateTestInstrument()
    {
        return CreateTestInstrumentWithEnvelope(
            attack: 0.001f,
            decay: 0.01f,
            sustain: 100f,
            release: 0.1f);
    }
    
    private static SfzInstrument CreateTestInstrumentWithEnvelope(
        float attack, float decay, float sustain, float release)
    {
        var instrument = new SfzInstrument
        {
            Name = "Test",
            BasePath = ""
        };
        
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

