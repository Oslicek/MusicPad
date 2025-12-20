using MusicPad.Core.Sfz;

namespace MusicPad.Tests.Sfz;

public class SfzPlayerPolyphonyTests
{
    private static SfzInstrument CreateTestInstrument()
    {
        var instrument = new SfzInstrument { Name = "Test", BasePath = "" };
        // Create a simple test sample (sine wave at 440Hz for 1 second at 44100Hz)
        var samples = new float[44100];
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = MathF.Sin(2 * MathF.PI * 440 * i / 44100f);
        }
        instrument.TestSamples = samples;
        
        // Add regions covering full MIDI range
        instrument.Regions.Add(new SfzRegion 
        { 
            LoKey = 0, 
            HiKey = 127,
            PitchKeycenter = 69, // A4 = 440Hz
            AmpegAttack = 0.001f,
            AmpegRelease = 0.01f
        });
        
        return instrument;
    }

    [Fact]
    public void NoteOn_SingleNote_ProducesSamples()
    {
        var player = new SfzPlayer(44100, maxVoices: 10);
        player.LoadInstrument(CreateTestInstrument());
        
        player.NoteOn(69, 100); // A4
        
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        // Should have non-zero samples
        Assert.Contains(buffer, v => Math.Abs(v) > 0.001f);
    }

    [Fact]
    public void NoteOn_TwoNotes_BothProduceSamples()
    {
        var player = new SfzPlayer(44100, maxVoices: 10);
        player.LoadInstrument(CreateTestInstrument());
        
        player.NoteOn(60, 100); // C4 (queued)
        player.NoteOn(64, 100); // E4 (queued)
        
        // Process buffer to trigger queued NoteOn events
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        Assert.Equal(2, player.ActiveVoiceCount);
        
        // Should have non-zero samples from both voices
        Assert.Contains(buffer, v => Math.Abs(v) > 0.001f);
    }

    [Fact]
    public void NoteOff_ReleasesCorrectVoice()
    {
        var player = new SfzPlayer(44100, maxVoices: 10);
        player.LoadInstrument(CreateTestInstrument());
        
        player.NoteOn(60, 100);
        player.NoteOn(64, 100);
        
        // Process buffer to trigger queued NoteOn events
        var buffer = new float[4000]; // ~90ms, enough to satisfy minimum hold time
        player.GenerateSamples(buffer);
        
        Assert.Equal(2, player.ActiveVoiceCount);
        
        player.NoteOff(60); // Release first note (queued)
        
        // After release phase completes, one voice should still be playing
        var buffer2 = new float[4096]; // Long enough for release
        player.GenerateSamples(buffer2);
        
        // One voice should still be playing (note 64)
        Assert.True(player.ActiveVoiceCount >= 1);
    }

    [Fact]
    public void MaxPolyphony_DropsOldestVoice()
    {
        var player = new SfzPlayer(44100, maxVoices: 2);
        player.LoadInstrument(CreateTestInstrument());
        
        player.NoteOn(60, 100);
        player.NoteOn(64, 100);
        player.NoteOn(67, 100); // Third note should drop the first
        
        // Process buffer to trigger queued NoteOn events
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        Assert.Equal(2, player.ActiveVoiceCount);
    }

    [Fact]
    public void StopAll_ReleasesAllVoices()
    {
        var player = new SfzPlayer(44100, maxVoices: 10);
        player.LoadInstrument(CreateTestInstrument());
        
        player.NoteOn(60, 100);
        player.NoteOn(64, 100);
        player.NoteOn(67, 100);
        
        // Process buffer to trigger queued NoteOn events
        var buffer1 = new float[512];
        player.GenerateSamples(buffer1);
        
        Assert.Equal(3, player.ActiveVoiceCount);
        
        player.StopAll();
        
        // All voices should be in release or finished
        var buffer2 = new float[4096];
        player.GenerateSamples(buffer2);
        
        // After buffer, voices should be done
        Assert.Equal(0, player.ActiveVoiceCount);
    }

    [Fact]
    public void Mix_TwoVoices_CombinesCorrectly()
    {
        var player = new SfzPlayer(44100, maxVoices: 10);
        player.LoadInstrument(CreateTestInstrument());
        
        player.NoteOn(60, 100);
        player.NoteOn(64, 100);
        
        // Warm up past attack
        var warmup = new float[1024];
        player.GenerateSamples(warmup);
        
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        // Peak values should be higher than single voice
        float maxSample = buffer.Max(Math.Abs);
        Assert.True(maxSample > 0.1f, "Mixed output should have significant amplitude");
    }

    [Fact]
    public void SameNoteOn_TwiceWithoutOff_DoesNotDuplicate()
    {
        var player = new SfzPlayer(44100, maxVoices: 10);
        player.LoadInstrument(CreateTestInstrument());
        
        player.NoteOn(60, 100);
        player.NoteOn(60, 100); // Same note again
        
        // Process buffer to trigger queued NoteOn events
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        // Should not create duplicate voice for same note
        Assert.Equal(1, player.ActiveVoiceCount);
    }

    [Fact]
    public void NoteOff_WithoutNoteOn_DoesNotCrash()
    {
        var player = new SfzPlayer(44100, maxVoices: 10);
        player.LoadInstrument(CreateTestInstrument());
        
        // Should not throw
        player.NoteOff(60);
        
        Assert.Equal(0, player.ActiveVoiceCount);
    }

    [Fact]
    public void GenerateSamples_NoVoices_ProducesSilence()
    {
        var player = new SfzPlayer(44100, maxVoices: 10);
        player.LoadInstrument(CreateTestInstrument());
        
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        Assert.All(buffer, v => Assert.Equal(0f, v));
    }

    [Fact]
    public void MaxVoices_DefaultIsTen()
    {
        var player = new SfzPlayer(44100);
        var instrument = CreateTestInstrument();
        player.LoadInstrument(instrument);
        
        // Play 10 notes
        for (int i = 60; i < 70; i++)
        {
            player.NoteOn(i, 100);
        }
        
        // Process buffer to trigger queued NoteOn events
        var buffer = new float[512];
        player.GenerateSamples(buffer);
        
        Assert.Equal(10, player.ActiveVoiceCount);
        
        // 11th note should steal oldest
        player.NoteOn(70, 100);
        player.GenerateSamples(buffer);
        
        Assert.Equal(10, player.ActiveVoiceCount);
    }
}

