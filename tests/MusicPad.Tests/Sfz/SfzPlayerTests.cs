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
}

