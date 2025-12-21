using Xunit;
using MusicPad.Core.Sfz;
using MusicPad.Core.Audio;

namespace MusicPad.Tests.Export;

/// <summary>
/// Tests for offline audio rendering functionality.
/// </summary>
public class OfflineRenderingTests
{
    private const int SampleRate = 44100;
    
    [Fact]
    public void GenerateSamples_WithActiveNote_ProducesNonZeroOutput()
    {
        // Arrange
        var player = new SfzPlayer(SampleRate);
        player.LoadInstrument(CreateTestInstrument());
        var buffer = new float[512];
        
        // Act - Start a note and generate samples
        player.NoteOn(60, 100);
        player.GenerateSamples(buffer); // Process the NoteOn event
        
        // Assert - Buffer should contain non-zero samples
        var hasNonZero = buffer.Any(s => Math.Abs(s) > 0.001f);
        Assert.True(hasNonZero, "Expected non-zero samples when note is playing");
    }
    
    [Fact]
    public void GenerateSamples_WithNoNotes_ProducesSilence()
    {
        // Arrange
        var player = new SfzPlayer(SampleRate);
        player.LoadInstrument(CreateTestInstrument());
        var buffer = new float[512];
        
        // Act - Generate samples without any notes
        player.GenerateSamples(buffer);
        
        // Assert - Buffer should be silent
        var maxValue = buffer.Max(Math.Abs);
        Assert.True(maxValue < 0.001f, "Expected silence when no notes are playing");
    }
    
    [Fact]
    public void GenerateSamples_MultipleBuffers_ContinuesPlayback()
    {
        // Arrange
        var player = new SfzPlayer(SampleRate);
        player.LoadInstrument(CreateLoopingTestInstrument());
        var buffer1 = new float[512];
        var buffer2 = new float[512];
        var buffer3 = new float[512];
        
        // Act - Start a note and generate multiple buffers
        player.NoteOn(60, 100);
        player.GenerateSamples(buffer1);
        player.GenerateSamples(buffer2);
        player.GenerateSamples(buffer3);
        
        // Assert - All buffers should have audio (note still sustaining with loop)
        Assert.True(buffer1.Any(s => Math.Abs(s) > 0.001f), "Buffer 1 should have audio");
        Assert.True(buffer2.Any(s => Math.Abs(s) > 0.001f), "Buffer 2 should have audio");
        Assert.True(buffer3.Any(s => Math.Abs(s) > 0.001f), "Buffer 3 should have audio");
    }
    
    [Fact]
    public void GenerateSamples_AfterNoteOff_EventuallyReachesSilence()
    {
        // Arrange
        var player = new SfzPlayer(SampleRate);
        player.LoadInstrument(CreateTestInstrument());
        var buffer = new float[512];
        
        // Act - Play and release a note
        player.NoteOn(60, 100);
        player.GenerateSamples(buffer); // Process NoteOn
        player.NoteOff(60);
        player.GenerateSamples(buffer); // Process NoteOff, start release
        
        // Generate enough buffers to complete the release phase
        // At 44100 Hz and 512 samples per buffer, we need about 86 buffers for 1 second
        for (int i = 0; i < 100; i++)
        {
            player.GenerateSamples(buffer);
        }
        
        // Assert - Should eventually reach silence
        var maxValue = buffer.Max(Math.Abs);
        Assert.True(maxValue < 0.01f, $"Expected near-silence after release, got max {maxValue}");
    }
    
    [Fact]
    public void GenerateSamples_PolyphonicNotes_MixesTogether()
    {
        // Arrange
        var singleNoteBuffer = new float[512];
        var twoNoteBuffer = new float[512];
        
        // Act - Generate with one note
        var player1 = new SfzPlayer(SampleRate);
        player1.LoadInstrument(CreateTestInstrument());
        player1.NoteOn(60, 100);
        player1.GenerateSamples(singleNoteBuffer);
        var singleNoteEnergy = singleNoteBuffer.Sum(s => s * s);
        
        // Generate with two notes
        var player2 = new SfzPlayer(SampleRate);
        player2.LoadInstrument(CreateTestInstrument());
        player2.NoteOn(60, 100);
        player2.NoteOn(64, 100);
        player2.GenerateSamples(twoNoteBuffer);
        var twoNoteEnergy = twoNoteBuffer.Sum(s => s * s);
        
        // Assert - Two notes should have more energy than one
        Assert.True(twoNoteEnergy > singleNoteEnergy * 1.5f, 
            "Expected two notes to have more energy than one note");
    }
    
    [Fact]
    public void LowPassFilter_WhenEnabled_ReducesHighFrequencyContent()
    {
        // Arrange
        var lpf = new LowPassFilter(SampleRate);
        lpf.IsEnabled = true;
        lpf.Cutoff = 0.1f; // Very low cutoff
        
        // Create a test signal with high frequency content
        var buffer = new float[1024];
        for (int i = 0; i < buffer.Length; i++)
        {
            // Mix of low (100Hz) and high (5000Hz) frequencies
            buffer[i] = (float)(Math.Sin(2 * Math.PI * 100 * i / SampleRate) + 
                                Math.Sin(2 * Math.PI * 5000 * i / SampleRate));
        }
        var originalEnergy = buffer.Sum(s => s * s);
        
        // Act
        lpf.Process(buffer);
        var filteredEnergy = buffer.Sum(s => s * s);
        
        // Assert - Energy should be reduced due to high frequency removal
        Assert.True(filteredEnergy < originalEnergy * 0.8f, 
            "Expected filtered signal to have less energy");
    }
    
    [Fact]
    public void Reverb_WhenEnabled_AddsWetSignal()
    {
        // Arrange
        var reverb = new Reverb(SampleRate);
        reverb.IsEnabled = true;
        reverb.Level = 1.0f; // Max wet level
        
        // Create a signal burst
        var buffer = new float[4096];
        for (int i = 0; i < 512; i++)
        {
            buffer[i] = (float)Math.Sin(2 * Math.PI * 440 * i / SampleRate);
        }
        var originalEnergy = buffer.Sum(s => s * s);
        
        // Act - Process the signal
        reverb.Process(buffer);
        var processedEnergy = buffer.Sum(s => s * s);
        
        // Assert - Processed energy should differ (reverb adds/modifies signal)
        // At high levels, the output should have more energy due to reverb tails
        Assert.True(Math.Abs(processedEnergy - originalEnergy) > 0.1f, 
            "Expected reverb to modify signal energy");
    }
    
    [Fact]
    public void Delay_WhenEnabled_ModifiesSignal()
    {
        // Arrange
        var delay = new Delay(SampleRate);
        delay.IsEnabled = true;
        delay.Time = 0.0f; // Minimum delay time (50ms = 2205 samples)
        delay.Feedback = 0.5f;
        delay.Level = 0.8f;
        
        // Create a burst of audio
        var buffer = new float[8192]; // Enough for delay to show
        for (int i = 0; i < 1024; i++)
        {
            buffer[i] = (float)Math.Sin(2 * Math.PI * 440 * i / SampleRate);
        }
        
        // Track the input energy after the burst
        var lateInputEnergy = buffer.Skip(3000).Take(1000).Sum(s => s * s);
        Assert.Equal(0f, lateInputEnergy); // Should be zero in input
        
        // Act - Process the signal (should add delayed copies)
        delay.Process(buffer);
        
        // Check the part after the delay time (50ms = ~2205 samples)
        // At sample 3000-4000, we should have the delayed signal
        var lateProcessedEnergy = buffer.Skip(3000).Take(1000).Sum(s => s * s);
        
        // Assert - Late part should have delayed echoes
        Assert.True(lateProcessedEnergy > 0.1f, 
            $"Expected delay echoes in late buffer, got energy {lateProcessedEnergy}");
    }
    
    [Fact]
    public void Chorus_WhenEnabled_ModifiesSignal()
    {
        // Arrange
        var chorus = new Chorus(SampleRate);
        chorus.IsEnabled = true;
        chorus.Depth = 0.5f;
        chorus.Rate = 0.5f;
        
        // Create a test signal
        var buffer = new float[1024];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (float)Math.Sin(2 * Math.PI * 440 * i / SampleRate);
        }
        var originalBuffer = buffer.ToArray();
        
        // Act
        chorus.Process(buffer);
        
        // Assert - Signal should be different (modulated)
        var difference = 0f;
        for (int i = 0; i < buffer.Length; i++)
        {
            difference += Math.Abs(buffer[i] - originalBuffer[i]);
        }
        Assert.True(difference > 1f, "Expected chorus to modify the signal");
    }
    
    [Fact]
    public void Equalizer_Reset_ClearsState()
    {
        // Arrange
        var eq = new Equalizer(SampleRate);
        eq.SetGain(0, 0.5f); // Boost bass
        
        // Process some signal to build up filter state
        var buffer = new float[512];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (float)Math.Sin(2 * Math.PI * 100 * i / SampleRate);
        }
        eq.Process(buffer);
        
        // Act
        eq.Reset();
        
        // Process silence - should get silence out
        var silentBuffer = new float[512];
        eq.Process(silentBuffer);
        
        // Assert - Should be silent after reset
        var maxValue = silentBuffer.Max(Math.Abs);
        Assert.True(maxValue < 0.001f, "Expected silence after EQ reset");
    }
    
    [Fact]
    public void LowPassFilter_Reset_ClearsState()
    {
        // Arrange
        var lpf = new LowPassFilter(SampleRate);
        lpf.IsEnabled = true;
        lpf.Cutoff = 0.5f;
        
        // Process some signal to build up filter state
        var buffer = new float[512];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (float)Math.Sin(2 * Math.PI * 100 * i / SampleRate);
        }
        lpf.Process(buffer);
        
        // Act
        lpf.Reset();
        
        // Process silence - should get silence out
        var silentBuffer = new float[512];
        lpf.Process(silentBuffer);
        
        // Assert - Should be silent after reset
        var maxValue = silentBuffer.Max(Math.Abs);
        Assert.True(maxValue < 0.001f, "Expected silence after LPF reset");
    }
    
    private static SfzInstrument CreateTestInstrument()
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
            PitchKeycenter = 60,
            Offset = 0,
            End = 999,
            AmpegAttack = 0.001f,
            AmpegRelease = 0.1f,
            AmpegSustain = 100f
        };
        instrument.Regions.Add(region);
        
        // Generate sine wave samples
        var samples = new float[1000];
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * 440 * i / SampleRate);
        }
        instrument.TestSamples = samples;
        
        return instrument;
    }
    
    private static SfzInstrument CreateLoopingTestInstrument()
    {
        var instrument = new SfzInstrument
        {
            Name = "LoopingTest",
            BasePath = ""
        };
        
        // Create a test region with looping
        var region = new SfzRegion
        {
            LoKey = 0,
            HiKey = 127,
            PitchKeycenter = 60,
            Offset = 0,
            End = 4409, // ~100ms at 44100Hz
            LoopStart = 100,
            LoopEnd = 4400,
            LoopMode = LoopMode.LoopContinuous,
            AmpegAttack = 0.001f,
            AmpegRelease = 0.1f,
            AmpegSustain = 100f
        };
        instrument.Regions.Add(region);
        
        // Generate sine wave samples
        var samples = new float[4410];
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * 440 * i / SampleRate);
        }
        instrument.TestSamples = samples;
        
        return instrument;
    }
}
